using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Financial_Reports
{
    public class MonthExpenseReportRepository : IMonthExpenseReportRepository
    {
        private readonly ISqlDataAccess _db;

        public MonthExpenseReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

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

        /// <summary>
        /// Matches MVC ReportProcessor.GetMothExpenseReport → EXEC [GetMonthExpenseReport]
        /// </summary>
        public async Task<IEnumerable<MonthExpenseReportDTO>> GetMonthExpenseReportAsync(string companyId)
        {
            const string sql = "EXEC [GetMonthExpenseReport]";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        private static MonthExpenseReportDTO MapRow(dynamic row)
        {
            return new MonthExpenseReportDTO
            {
                AccGrpCode = GetInt(row, "AccGrpCode", "accGrpCode"),
                AccGrpName = GetString(row, "AccGrpName", "accGrpName"),
                AccEffCode = GetString(row, "AccEffCode", "accEffCode"),
                AccTypeDisc = GetString(row, "AccTypeDisc", "accTypeDisc"),
                ExpYear = GetInt(row, "ExpYear", "expYear"),
                ExpMonth = GetInt(row, "ExpMonth", "expMonth"),
                ExpMonthName = GetString(row, "ExpMonthName", "expMonthName"),
                StockCurrencyAmount = GetDecimal(row, "StockCurrencyAmount", "stockCurrencyAmount"),
                MasterCurrencyAmount = GetDecimal(row, "MasterCurrencyAmount", "masterCurrencyAmount")
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
            return GetRawValue(row, keys)?.ToString();
        }

        private static int GetInt(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out int n)
                ? n
                : 0;
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
    }
}
