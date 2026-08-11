using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Financial_Reports
{
    public class LedgerReportRepository : ILedgerReportRepository
    {
        private readonly ISqlDataAccess _db;

        public LedgerReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        #region Permission
        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _db.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }
        #endregion

        #region Dropdowns
        public Task<IEnumerable<BasicDataDto>> GetTransactionDropDownAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchase",
                new Dictionary<string, object>(),
                companyId);
        }
        #endregion

        #region Account Code Lookup
        public async Task<int> GetAccountCodeAsync(string whereClause, string companyId)
        {
            // SelectDataFromSP<dynamic> does not map columns — use ADO dynamic (ExpandoObject) like other reports
            var escapedWhere = (whereClause ?? string.Empty).Replace("'", "'");
            var sql = $"EXEC [GetAllCommonData] @Type='Party', @Where = '{escapedWhere}'";
            var result = await _db.SelectDataAdoDynamic(sql, companyId);
            var firstRow = result.FirstOrDefault();
            if (firstRow != null)
            {
                var val = GetRawValue(firstRow, "AccGrpCode", "accgrpCode", "Code", "code");
                if (val != null)
                {
                    int.TryParse(Convert.ToString(val, CultureInfo.InvariantCulture), out int code);
                    return code;
                }
            }
            return 0;
        }
        #endregion

        #region Report Data (Dynamic / List)
        public async Task<IEnumerable<DualLedgerRowDTO>> GetLedgerDataDatewiseAsync(
            int partyCode,
            DateTime fromDate,
            DateTime toDate,
            string companyId)
        {
            // MVC: SP_H_LedgerDatewiseReport @MainAccCode, @TrnDate, @ToDate
            string sql =
                $"EXEC [SP_H_LedgerDatewiseReport] @MainAccCode={partyCode}," +
                $"@TrnDate='{fromDate:yyyy-MM-dd}',@ToDate='{toDate:yyyy-MM-dd}'";

            var dynamicRows = await _db.SelectDataAdoDynamic(sql, companyId);
            return dynamicRows.Select(MapDualLedgerRow).ToList();
        }

        private static DualLedgerRowDTO MapDualLedgerRow(dynamic row)
        {
            return new DualLedgerRowDTO
            {
                Date = NormalizeLedgerCell(GetRawValue(row, "Date", "date", "TrnDate", "JVDate")),
                DueDate = NormalizeLedgerCell(GetRawValue(row, "DueDate", "dueDate")),
                VType = GetString(row, "VType", "vType"),
                Account = GetString(row, "Account", "account"),
                Carats = NormalizeLedgerCell(GetRawValue(row, "Carats", "carats")),
                SDebitAmount = NormalizeLedgerCell(GetRawValue(row, "SDebitAmount", "sDebitAmount", "SDebit")),
                MDebitAmount = NormalizeLedgerCell(GetRawValue(row, "MDebitAmount", "mDebitAmount", "MDebit")),
                InvDescription = GetString(row, "InvDescription", "invDescription", "Description"),

                Date1 = NormalizeLedgerCell(GetRawValue(row, "Date1", "date1", "JVDate1", "TrnDate1")),
                DueDate1 = NormalizeLedgerCell(GetRawValue(row, "DueDate1", "dueDate1")),
                VType1 = GetString(row, "VType1", "vType1"),
                Account1 = GetString(row, "Account1", "account1", "CreditAccount", "creditAccount"),
                Carats1 = NormalizeLedgerCell(GetRawValue(row, "Carats1", "carats1")),
                SCreditAmount = NormalizeLedgerCell(GetRawValue(row, "SCreditAmount", "sCreditAmount", "SCredit")),
                MCreditAmount = NormalizeLedgerCell(GetRawValue(row, "MCreditAmount", "mCreditAmount", "MCredit")),
                InvDescription1 = GetString(row, "InvDescription1", "invDescription1", "Description1"),

                GCode = NormalizeLedgerCell(GetRawValue(row, "GCode", "gCode")),
                VCode = NormalizeLedgerCell(GetRawValue(row, "VCode", "vCode", "Vcode", "vcode")),
                GCode1 = NormalizeLedgerCell(GetRawValue(row, "GCode1", "gCode1")),
                VCode1 = NormalizeLedgerCell(GetRawValue(row, "VCode1", "vCode1"))
            };
        }

        private static object? NormalizeLedgerCell(object? value)
        {
            if (value == null || value == DBNull.Value) return null;
            return value switch
            {
                DateTime dt => dt,
                DateTimeOffset dto => dto.DateTime,
                byte or short or int or long or float or double or decimal => value,
                bool => value,
                string s => s,
                Guid g => g.ToString(),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }

        public async Task<IEnumerable<LedgerListDetailDTO>> GetLedgerDataList1Async(
            string fromDate,
            string toDate,
            int partyCode,
            string companyId)
        {
            // Match MVC LedgerProcessor.GetLedgerDataList1
            string sql =
                $"EXEC [GetLedgerSSRS] @TDate='{toDate}',@FDate='{fromDate}',@AccCode='{partyCode}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapLedgerListDetailRow).ToList();
        }

        public async Task<decimal> GetFinalAmountAsync(string toDate, string whereClause, string companyId)
        {
            // MVC LedgerProcessor.GetFinalAmount
            var escapedWhere = (whereClause ?? string.Empty).Replace("'", "''");
            var escapedToDate = (toDate ?? string.Empty).Replace("'", "''");
            string sql =
                $"EXEC [GetLedgerReportSbalance] @Type='GetStockBalAmount'," +
                $"@Where='{escapedWhere}',@ToDate='{escapedToDate}'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            var first = rows.FirstOrDefault();
            if (first == null) return 0m;

            if (first is IDictionary<string, object> dict)
            {
                var firstVal = dict.Values.FirstOrDefault(v => v != null && v != DBNull.Value);
                if (firstVal != null &&
                    decimal.TryParse(Convert.ToString(firstVal, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amt))
                {
                    return amt;
                }
            }

            return 0m;
        }

        public async Task<LedgerAccountAddressDTO?> GetAccountAddressAsync(string whereClause, string companyId)
        {
            // MVC LedgerProcessor.GetAccountAddress
            var escapedWhere = (whereClause ?? string.Empty).Replace("'", "''");
            string sql =
                $"EXEC [GetAllCommonData] @Type='GetAddressesFromAccount', @Where = '{escapedWhere}'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            var first = rows.FirstOrDefault();
            if (first == null) return null;

            return new LedgerAccountAddressDTO
            {
                AddressLine1 = GetString(first, "AddressLine1", "addressLine1"),
                AddressLine2 = GetString(first, "AddressLine2", "addressLine2"),
                AddressLine3 = GetString(first, "AddressLine3", "addressLine3")
            };
        }
        #endregion

        #region Mapping Helpers
        private static LedgerListDetailDTO MapLedgerListDetailRow(dynamic row)
        {
            return new LedgerListDetailDTO
            {
                Vcode = GetLong(row, "vcode", "Vcode", "VCode"),
                JVDate = GetString(row, "JVDate", "jvDate"),
                VType = GetString(row, "VType", "vType"),
                Account = GetString(row, "Account", "account"),
                StockDebit = GetDecimal(row, "StockDebit", "stockDebit"),
                StockCredit = GetDecimal(row, "StockCredit", "stockCredit"),
                StockFinalAmount = GetDecimal(row, "StockFinalAmount", "stockFinalAmount"),
                MasterDebit = GetDecimal(row, "MasterDebit", "masterDebit"),
                MasterCredit = GetDecimal(row, "MasterCredit", "masterCredit"),
                MasterFinalAmount = GetDecimal(row, "MasterFinalAmount", "masterFinalAmount"),
                FormattedJvDate = GetString(row, "FormattedJvDate", "formattedJvDate"),
                InvNo = GetString(row, "InvNo", "invNo"),
                GCode = GetLong(row, "GCode", "gCode", "gcode")
            };
        }

        private static object? GetRawValue(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k =>
                        string.Equals((k ?? string.Empty).Trim(), key, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        var value = dict[match];
                        if (value != null && value != DBNull.Value) return value;
                    }
                }
            }
            return null;
        }

        private static string? GetString(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            return v?.ToString();
        }

        private static long GetLong(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out long result) ? result : 0;
        }

        private static decimal GetDecimal(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0m;
            return decimal.TryParse(
                Convert.ToString(v, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal n)
                ? n
                : 0m;
        }
        #endregion
    }
}
