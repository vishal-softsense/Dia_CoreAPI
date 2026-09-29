using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixConsignmentReturnRepository : IMixConsignmentReturnRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixConsignmentReturnRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<MixConsignmentReturnPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();
            return new MixConsignmentReturnPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type"))
            };
        }

        public async Task<List<MixConsignmentReturnRowDto>> GetListAsync(
            MixConsignmentReturnFilterRequest filter, string companyId)
        {
            // MVC: Exec [GetmixmemoDataForreturn] @Where ='{strWhere}'
            string strWhere = BuildWhereClause(filter ?? new());
            string sql = $"EXEC [GetmixmemoDataForreturn] @Where = '{strWhere}'";
            var raw = await _sqlDataAccess.SelectRawSql<MixConsignmentReturnRawRow>(sql, companyId);

            // MVC controller maps Item/ItemName by StockType (Diamond vs Jewellery)
            return raw.Select(row => new MixConsignmentReturnRowDto
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
            MixConsignmentReturnSaveRequest request, string userId, string companyId)
        {
            var items = (request.Items ?? new())
                .Where(x => x.Chk && x.NReturn > 0)
                .ToList();

            if (items.Count == 0)
                return (false, "Please select at least one stone with New Return > 0.");

            if (!int.TryParse(companyId, out int companyIdInt))
                return (false, "Invalid company.");

            int usercode = 0;
            int.TryParse(userId, out usercode);

            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                // MVC: GetMaxInvNoMemoRtn(" And TrnType=''MixMemoInRtn'' ") then + 1
                string invNoStr = await ExecuteCommonDataScalarAsync(
                    conn, trans, "InvCode", " And TrnType=''MixMemoInRtn'' ");
                int.TryParse(invNoStr, out int maxInvNo);
                maxInvNo = maxInvNo + 1;

                DateTime rtnDate = request.RtnDate.Date.Add(DateTime.Now.TimeOfDay);

                double.TryParse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurID"), out double mCurCode);
                double.TryParse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurID"), out double sCurCode);

                foreach (var im in items)
                {
                    if (im.SPTCode <= 0) continue;

                    int maxItemCode = 0;
                    if (string.Equals(im.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                    {
                        string itemCodeStr = await ExecuteCommonDataScalarAsync(
                            conn, trans, "StockCode", $" AND Stock_ID = ''{EscapeSql(im.LotNo)}''");
                        int.TryParse(itemCodeStr, out maxItemCode);
                    }
                    else
                    {
                        string itemCodeStr = await ExecuteCommonDataScalarAsync(
                            conn, trans, "ItemCode", $" AND LotNo = ''{EscapeSql(im.LotNo)}''");
                        int.TryParse(itemCodeStr, out maxItemCode);
                    }

                    // MVC: UpdateMemo(SPTCode, RtnDate, NReturn, "memortnp")
                    using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@code", im.SPTCode));
                        cmd.Parameters.Add(new SqlParameter("@RtnDate", rtnDate));
                        cmd.Parameters.Add(new SqlParameter("@rtncts", im.NReturn));
                        cmd.Parameters.Add(new SqlParameter("@Actiontype", "memortnp"));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    float.TryParse(
                        await ExecuteCurrencyDetailScalarAsync(conn, trans, "McurBaseRate", im.CurCode.ToString(CultureInfo.InvariantCulture)),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out float mCurBaseRate);
                    float.TryParse(
                        await ExecuteCurrencyDetailScalarAsync(conn, trans, "ScurBaseRate", im.CurCode.ToString(CultureInfo.InvariantCulture)),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out float sCurBaseRate);

                    string mCurOperator = await ExecuteCurrencyDetailScalarAsync(
                        conn, trans, "MCurOperator", " and ID=" + im.CurCode);
                    string sCurOperator = await ExecuteCurrencyDetailScalarAsync(
                        conn, trans, "SCurOperator", " and ID=" + im.CurCode);

                    if (string.IsNullOrWhiteSpace(mCurOperator)) mCurOperator = "*";
                    if (string.IsNullOrWhiteSpace(sCurOperator)) sCurOperator = "*";

                    float amount = im.Price * im.NReturn;
                    var amtRes = GetConvertAmount(
                        im.CurCode, amount, mCurBaseRate, mCurOperator,
                        sCurBaseRate, sCurOperator, mCurCode, sCurCode);

                    string itemName = im.ItemName ?? "";

                    // Ported exactly from MVC MixConsignmentReturnController.Consignmentrtnpartial
                    using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@SPdate", rtnDate));
                        cmd.Parameters.Add(new SqlParameter("@TrnType", "MixMemoInRtn"));
                        cmd.Parameters.Add(new SqlParameter("@InvType", "ALL"));
                        cmd.Parameters.Add(new SqlParameter("@InvCode", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@ItemCode", maxItemCode));
                        cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                        cmd.Parameters.Add(new SqlParameter("@Carats", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@Pcs", (object)0));
                        cmd.Parameters.Add(new SqlParameter("@Rate", im.Price));
                        cmd.Parameters.Add(new SqlParameter("@MRP", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@PMRPDown", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@ReturnCarats", im.NReturn));
                        cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@amount", amount));
                        cmd.Parameters.Add(new SqlParameter("@ExportNo", ""));
                        cmd.Parameters.Add(new SqlParameter("@Less1", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@Less2", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@Less3", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@CurCode", im.CurCode.ToString(CultureInfo.InvariantCulture)));
                        cmd.Parameters.Add(new SqlParameter("@memotrancode", im.SPTCode));
                        cmd.Parameters.Add(new SqlParameter("@StockEffected", true));
                        cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                        cmd.Parameters.Add(new SqlParameter("@spcarats", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@FItemCode", 0));
                        cmd.Parameters.Add(new SqlParameter("@TItemCode", 0));
                        cmd.Parameters.Add(new SqlParameter("@FItemRate", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@TItemRate", 0.0));
                        cmd.Parameters.Add(new SqlParameter("@FromSuratCode", 0));
                        cmd.Parameters.Add(new SqlParameter("@AssorterCode", 0));
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
                        cmd.Parameters.Add(new SqlParameter("@StockType", im.StockType ?? "Diamond"));
                        cmd.Parameters.Add(new SqlParameter("@InvDescription", ""));
                        cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                trans.Commit();
                return (true, "MixConsigmentReturn Sucessfull");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Ported from MVC MixConsignmentReturnController.ConsignmentlistForrtnpartial.
        /// Quotes doubled (''value'') because @Where is embedded into dynamic SQL inside the SP.
        /// </summary>
        private static string BuildWhereClause(MixConsignmentReturnFilterRequest filter)
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
                // MVC Mix: keep spaces; replace newlines with commas; match LotNo OR Stock_ID
                string strTmpNewLine = filter.LotNo.Trim();
                strTmpNewLine = strTmpNewLine
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace("\r", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                strWhere += " AND ( IM.LotNo IN (''" + strTmpNewLine + "'') " +
                            " OR TJ.Stock_ID IN (''" + strTmpNewLine + "'') ) ";
            }

            strWhere += " and SP.TrnType = ''CMemoIn'' ";

            if (!filter.ClearMemo)
                strWhere += " and Format((SPT.carats - (SPT.ReturnCarats +SPT.spcarats)),''0.000'')<>''0.000''";

            return strWhere;
        }

        private async Task<string> ExecuteCommonDataScalarAsync(
            SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetAllCommonData", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            string cleanedWhere = where?.Replace("''", "'") ?? "";
            cmd.Parameters.Add(new SqlParameter("@Where", cleanedWhere));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private async Task<string> ExecuteCurrencyDetailScalarAsync(
            SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetCurrencyDetail", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            string cleanedWhere = where?.Replace("''", "'") ?? "";
            cmd.Parameters.Add(new SqlParameter("@Where", cleanedWhere));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private static (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode, double amount, double mCurBaseRate, string mCurOperator,
            double sCurBaseRate, string sCurOperator, double mCurCode, double sCurCode)
        {
            double mCurAmt = 0, sCurAmt = 0;

            if (curCode == mCurCode)
            {
                mCurAmt = amount;
                sCurAmt = sCurOperator == "*"
                    ? amount * sCurBaseRate
                    : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
            }
            else if (curCode == sCurCode)
            {
                sCurAmt = amount;
                mCurAmt = mCurOperator == "*"
                    ? amount * mCurBaseRate
                    : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
            }
            else
            {
                mCurAmt = mCurOperator == "*"
                    ? amount * mCurBaseRate
                    : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
                sCurAmt = sCurOperator == "*"
                    ? amount * sCurBaseRate
                    : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
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
    }
}
