using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireConsignmentInReturnRepository : ISolitaireConsignmentInReturnRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SolitaireConsignmentInReturnRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            string where = actionType switch
            {
                "insert" => " InsertData = 1 ",
                "edit" => " EditData = 1 ",
                "delete" => " DeleteData = 1 ",
                "navigate" => " NavigateData = 1 ",
                _ => " SelectData = 1 "
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission", parameters, companyId);

            return result.Count();
        }

        public async Task<SolitaireConsignmentInReturnPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();
            return new SolitaireConsignmentInReturnPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type"))
            };
        }

        public async Task<List<SolitaireConsignmentInReturnRowDto>> GetListAsync(
            SolitaireConsignmentInReturnFilterRequest filter, string companyId)
        {
            // Match MVC exactly: Exec [GetmemoDataForreturn] @Where ='{strWhere}'
            // MVC embeds @Where in a SQL string literal, so filters must use doubled quotes (''value'').
            string strWhere = BuildWhereClause(filter ?? new());
            string sql = $"EXEC [GetmemoDataForreturn] @Where = '{strWhere}'";
            var result = await _sqlDataAccess.SelectRawSql<SolitaireConsignmentInReturnRowDto>(sql, companyId);
            return result.ToList();
        }

        public async Task<(bool Success, string Message)> ReturnAsync(
            SolitaireConsignmentInReturnSaveRequest request, string companyId)
        {
            var items = (request.Items ?? new()).Where(x => x.Chk).ToList();
            if (items.Count == 0)
                return (false, "Please select at least one stone to return.");

            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                // MVC: RtnDate = date part + current time of day
                DateTime rtnDate = request.RtnDate.Date.Add(DateTime.Now.TimeOfDay);

                foreach (var im in items)
                {
                    if (im.SPTCode <= 0) continue;

                    using var cmd = new SqlCommand("SP_UpdateMemo", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", im.SPTCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnDate", rtnDate));
                    cmd.Parameters.Add(new SqlParameter("@rtncts", im.Balance));
                    cmd.Parameters.Add(new SqlParameter("@Actiontype", "memortn"));
                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
                return (true, "Consignment Return Sucessfull");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Ported from MVC ConsignmentReturnController.ConsignmentlistForrtn.
        /// Quotes are doubled (''value'') because @Where is embedded into dynamic SQL inside the SP
        /// the same way MVC builds: Exec [GetmemoDataForreturn] @Where ='{strWhere}'.
        /// </summary>
        private static string BuildWhereClause(SolitaireConsignmentInReturnFilterRequest filter)
        {
            string strWhere = "";

            if (!string.IsNullOrWhiteSpace(filter.InvNo))
                strWhere += " AND  sp.invno  IN (''" + EscapeSql(filter.InvNo.Trim()) + "'')";

            if (!string.IsNullOrWhiteSpace(filter.BillNo))
                strWhere += " AND sp.BillNo IN (''" + EscapeSql(filter.BillNo.Trim()) + "'')";

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                !string.Equals(filter.Type, "All", StringComparison.OrdinalIgnoreCase))
                strWhere += " AND SPT.invtype IN (''" + EscapeSql(filter.Type.Trim()) + "'')";

            if (!string.IsNullOrWhiteSpace(filter.Party))
                strWhere += " AND acmp.AccGrpName IN (''" + EscapeSql(filter.Party.Trim()) + "'')";

            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo.Replace(" ", "");
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                strWhere += " AND Im.LotNo IN (''" + strTmpNewLine + "'')";
            }

            strWhere += " and SP.TrnType = ''MemoIn'' ";

            // MVC: only when ClearMemo == false (checkbox unchecked)
            if (!filter.ClearMemo)
                strWhere += " and Format((SPT.carats - (SPT.ReturnCarats +SPT.spcarats)),''0.000'')<>''0.000''";

            return strWhere;
        }

        private static string EscapeSql(string value) =>
            (value ?? "").Replace("'", "''");

        private static List<BasicDataDto> MapBasic(IEnumerable<StockBasic> source) =>
            source.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Type = x.Type,
                Priority = x.Priority,
                Name = x.Name,
                TextName = x.TextName
            }).ToList();
    }
}
