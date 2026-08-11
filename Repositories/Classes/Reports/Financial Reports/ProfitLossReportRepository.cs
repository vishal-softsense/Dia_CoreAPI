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
    public class ProfitLossReportRepository : IProfitLossReportRepository
    {
        private readonly ISqlDataAccess _db;

        public ProfitLossReportRepository(ISqlDataAccess db)
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
        /// Matches MVC TrialBalSheetProcessor.GetTradingAcDataList1 → EXEC [SPReportTAPL]
        /// </summary>
        public async Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReportAsync(
            DateTime toDate,
            string type,
            string calType,
            string companyId)
        {
            var escapedType = (type ?? string.Empty).Replace("'", "''");
            var escapedCalType = (calType ?? string.Empty).Replace("'", "''");
            string sql =
                $"EXEC [SPReportTAPL] @TDate = '{toDate:yyyy-MM-dd}'," +
                $"@Type = '{escapedType}',@CalType = '{escapedCalType}'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        private static ProfitLossReportDTO MapRow(dynamic row)
        {
            return new ProfitLossReportDTO
            {
                DAccGrpName = GetString(row, "DAccGrpName", "dAccGrpName"),
                DMBalAmt = GetDecimal(row, "DMBalAmt", "dMBalAmt"),
                DSBalAmt = GetDecimal(row, "DSBalAmt", "dSBalAmt"),
                CAccGrpName = GetString(row, "CAccGrpName", "cAccGrpName"),
                CMBalAmt = GetDecimal(row, "CMBalAmt", "cMBalAmt"),
                CSBalAmt = GetDecimal(row, "CSBalAmt", "cSBalAmt")
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
