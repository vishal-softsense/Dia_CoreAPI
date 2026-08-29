using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixMemoOutReturnRepository : IMixMemoOutReturnRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixMemoOutReturnRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<MixMemoOutReturnPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();
            return new MixMemoOutReturnPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type"))
            };
        }

        public async Task<List<MixMemoOutReturnRowDto>> GetListAsync(
            MixMemoOutReturnFilterRequest filter, string companyId)
        {
            // Match MVC: Exec [GetmixmemoDataForreturn] @Where ='{strWhere}'
            string strWhere = BuildWhereClause(filter ?? new());
            string sql = $"EXEC [GetmixmemoDataForreturn] @Where = '{strWhere}'";
            var raw = await _sqlDataAccess.SelectRawSql<MixMemoReturnRawRow>(sql, companyId);

            return raw.Select(row => new MixMemoOutReturnRowDto
            {
                Date = row.Date,
                MemoNo = row.MemoNo,
                Party = row.Party,
                Broker = row.Broker,
                Item = string.Equals(row.StockType, "Diamond", StringComparison.OrdinalIgnoreCase)
                    ? row.Item
                    : row.JewItem,
                carats = row.carats,
                Pcs = row.Pcs,
                Price = row.Price,
                Sale = row.Sale,
                Return = row.Return,
                Balance = row.Balance,
                SPCode = row.SPCode,
                SPTCode = row.SPTCode,
                ItemCode = row.ItemCode,
                ItemName = string.Equals(row.StockType, "Diamond", StringComparison.OrdinalIgnoreCase)
                    ? row.ItemName
                    : row.JewItemName,
                MNo_BillNo = row.MNo_BillNo,
                CurCode = row.CurCode,
                StockType = row.StockType
            }).ToList();
        }

        public async Task<(bool Success, string Message)> ReturnAsync(
            MixMemoOutReturnSaveRequest request, string userId, string companyId)
        {
            var items = (request.Items ?? new()).Where(x => x.Chk && x.NReturn > 0).ToList();
            if (items.Count == 0)
                return (false, "Please select at least one stone with return carats > 0.");

            if (!int.TryParse(userId, out int usercode))
                usercode = 0;
            if (!int.TryParse(companyId, out int companyIdInt))
                companyIdInt = 0;

            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                // MVC: GetMaxInvNoMemoRtn(" And TrnType=''MixMemoRtnP''") + 1
                string invNoStr = await ExecuteCommonDataScalarAsync(
                    conn, trans, "InvCode", " And TrnType='MixMemoRtnP'");
                int maxInvNo = (int.TryParse(invNoStr, out var invParsed) ? invParsed : 0) + 1;

                DateTime rtnDate = request.RtnDate.Date.Add(DateTime.Now.TimeOfDay);

                double mCurCode = double.TryParse(
                    await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurID"), out var mId) ? mId : 0;
                double sCurCode = double.TryParse(
                    await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurID"), out var sId) ? sId : 0;

                foreach (var im in items)
                {
                    if (im.SPTCode <= 0) continue;

                    int maxItemCode = 0;
                    string stockType = im.StockType ?? "";
                    string lotKey = (im.LotNo ?? "").Replace("'", "''");

                    if (string.Equals(stockType, "Diamond", StringComparison.OrdinalIgnoreCase))
                    {
                        // ExecuteCommonDataScalarAsync converts '' → ' for SP params (same as MixMemoOut)
                        string itemCodeStr = await ExecuteCommonDataScalarAsync(
                            conn, trans, "ItemCode", $" AND LotNo = ''{lotKey}''");
                        maxItemCode = int.TryParse(itemCodeStr, out var ic) ? ic : 0;
                    }
                    else if (string.Equals(stockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                    {
                        string itemCodeStr = await ExecuteCommonDataScalarAsync(
                            conn, trans, "StockCode", $" AND Stock_ID = ''{lotKey}''");
                        maxItemCode = int.TryParse(itemCodeStr, out var jc) ? jc : 0;
                    }

                    // SP_UpdateMemo Actiontype = memortnp, rtncts = NReturn
                    using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@code", im.SPTCode));
                        cmd.Parameters.Add(new SqlParameter("@RtnDate", rtnDate));
                        cmd.Parameters.Add(new SqlParameter("@rtncts", im.NReturn));
                        cmd.Parameters.Add(new SqlParameter("@Actiontype", "memortnp"));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    float mCurBaseRate = await GetCurrencyRateAsync(conn, trans, "McurBaseRate", im.CurCode.ToString());
                    float sCurBaseRate = await GetCurrencyRateAsync(conn, trans, "ScurBaseRate", im.CurCode.ToString());
                    string mCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + im.CurCode);
                    string sCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + im.CurCode);
                    if (string.IsNullOrWhiteSpace(mCurOperator)) mCurOperator = "*";
                    if (string.IsNullOrWhiteSpace(sCurOperator)) sCurOperator = "*";

                    float amount = im.Price * im.NReturn;
                    var amtRes = GetConvertAmount(im.CurCode, amount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, mCurCode, sCurCode);

                    // Match MVC InsertSalePurTran(..., InvCode:0, RoughCode:MaxInvNo, ...)
                    // Cast int 0 as (object) so SqlParameter(string, object) is used — not SqlParameter(string, SqlDbType).
                    using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@SPdate", rtnDate));
                        cmd.Parameters.Add(new SqlParameter("@TrnType", "MixMemoRtnP"));
                        cmd.Parameters.Add(new SqlParameter("@InvType", "ALL"));
                        cmd.Parameters.Add(new SqlParameter("@InvCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@ItemCode", maxItemCode));
                        cmd.Parameters.Add(new SqlParameter("@ItemName", im.ItemName ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@Carats", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@Pcs", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.Price)));
                        cmd.Parameters.Add(new SqlParameter("@MRP", 0d));
                        cmd.Parameters.Add(new SqlParameter("@PMRPDown", 0d));
                        cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(im.NReturn)));
                        cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0d));
                        cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(amount)));
                        cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                        cmd.Parameters.Add(new SqlParameter("@Less1", 0d));
                        cmd.Parameters.Add(new SqlParameter("@Less2", 0d));
                        cmd.Parameters.Add(new SqlParameter("@Less3", 0d));
                        cmd.Parameters.Add(new SqlParameter("@CurCode", im.CurCode.ToString()));
                        cmd.Parameters.Add(new SqlParameter("@memotrancode", im.SPTCode));
                        cmd.Parameters.Add(new SqlParameter("@StockEffected", true));
                        cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                        cmd.Parameters.Add(new SqlParameter("@spcarats", 0d));
                        cmd.Parameters.Add(new SqlParameter("@FItemCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@TItemCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@FItemRate", 0d));
                        cmd.Parameters.Add(new SqlParameter("@TItemRate", 0d));
                        cmd.Parameters.Add(new SqlParameter("@FromSuratCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@AssorterCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@RoughCode", maxInvNo));
                        cmd.Parameters.Add(new SqlParameter("@MemoRemark", ""));
                        cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
                        cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Convert.ToDouble(amtRes.MCurAmt.ToString("N2"))));
                        cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
                        cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Convert.ToDouble(amtRes.SCurAmt.ToString("N2"))));
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                        cmd.Parameters.Add(new SqlParameter("@CompanyID", companyIdInt));
                        cmd.Parameters.Add(new SqlParameter("@PShape", ""));
                        cmd.Parameters.Add(new SqlParameter("@PSize", ""));
                        cmd.Parameters.Add(new SqlParameter("@PSieve", ""));
                        cmd.Parameters.Add(new SqlParameter("@PColor", ""));
                        cmd.Parameters.Add(new SqlParameter("@PClarity", ""));
                        cmd.Parameters.Add(new SqlParameter("@COGS", 0f));
                        cmd.Parameters.Add(new SqlParameter("@StockType", stockType));
                        cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                trans.Commit();
                return (true, "MixMemoReturn Sucessfull");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Ported from MVC GeneralMemoOutReturnController.MixMemolistForrtn.
        /// Doubled quotes (''value'') match MVC Exec [GetmixmemoDataForreturn] @Where ='{strWhere}'.
        /// </summary>
        private static string BuildWhereClause(MixMemoOutReturnFilterRequest filter)
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
                string strTmpNewLine = filter.LotNo.Trim();
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\r", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                strWhere += " AND ( IM.LotNo IN (''" + strTmpNewLine + "'') " +
                            " OR TJ.Stock_ID IN (''" + strTmpNewLine + "'') ) ";
            }

            strWhere += " and SP.TrnType = ''CMemoOut'' ";

            if (!filter.ClearMemo)
                strWhere += "AND FORMAT((SPT.carats -(SPT.ReturnCarats + ISNULL(MemoToSale.LiveSPcarats, 0.000))), ''0.000'') <> ''0.000''";
            else
                strWhere += "AND FORMAT((SPT.carats -(SPT.ReturnCarats + ISNULL(MemoToSale.LiveSPcarats, 0.000))), ''0.000'') = ''0.000''";

            return strWhere;
        }

        private async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetAllCommonData", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private async Task<string> ExecuteCurrencyDetailScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetCurrencyDetail", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private async Task<float> GetCurrencyRateAsync(SqlConnection conn, SqlTransaction trans, string type, string curCode)
        {
            string result = await ExecuteCurrencyDetailScalarAsync(conn, trans, type, curCode);
            return float.TryParse(result, out var rate) ? rate : 0f;
        }

        private static (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode, double amount, double mCurBaseRate, string mCurOperator,
            double sCurBaseRate, string sCurOperator, double mCurCode, double sCurCode)
        {
            double mCurAmt = 0, sCurAmt = 0;

            if (curCode == mCurCode)
            {
                mCurAmt = amount;
                sCurAmt = sCurOperator == "*" ? amount * sCurBaseRate : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
            }
            else if (curCode == sCurCode)
            {
                sCurAmt = amount;
                mCurAmt = mCurOperator == "*" ? amount * mCurBaseRate : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
            }
            else
            {
                mCurAmt = mCurOperator == "*" ? amount * mCurBaseRate : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
                sCurAmt = sCurOperator == "*" ? amount * sCurBaseRate : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
            }

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt)) mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt)) sCurAmt = 0;
            return (mCurAmt, sCurAmt);
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

        private class MixMemoReturnRawRow
        {
            public string? Date { get; set; }
            public string? MemoNo { get; set; }
            public string? Party { get; set; }
            public string? Broker { get; set; }
            public string? Item { get; set; }
            public string? JewItem { get; set; }
            public float carats { get; set; }
            public float Pcs { get; set; }
            public float Price { get; set; }
            public float Sale { get; set; }
            public float Return { get; set; }
            public float Balance { get; set; }
            public float SPCode { get; set; }
            public float SPTCode { get; set; }
            public float ItemCode { get; set; }
            public string? ItemName { get; set; }
            public string? JewItemName { get; set; }
            public string? MNo_BillNo { get; set; }
            public float CurCode { get; set; }
            public string? StockType { get; set; }
        }
    }
}
