using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireKapanSummaryReportRepository : ISolitaireKapanSummaryReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireKapanSummaryReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireKapanSummaryReportRowDto>> GetReportAsync(
            SolitaireKapanSummaryReportFilterDto filter,
            string companyId)
        {
            var where = BuildKapanWhere(filter);
            // MVC: ReportProcessor.GetKapanSummaryReport => EXEC [sp_KapanSummary] @Where
            var sql = $"EXEC [sp_KapanSummary] @Where = '{where}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        // Ported from MVC: ReportController.KapanSummaryReportFilter
        private static string BuildKapanWhere(SolitaireKapanSummaryReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (filter == null) return strWhere;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND CONVERT(date,  tpt.[SPdate])  BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    var filterType = (filter.FilterType ?? "").Trim().ToLowerInvariant();
                    if (filterType == "include")
                        strWhere += " AND v.KapanNo IN (''" + strTmpNewLine + "'')";
                    else if (filterType == "exclude")
                        strWhere += " AND v.KapanNo not IN (''" + strTmpNewLine + "'')";
                }
            }
            catch
            {
                // Swallow like MVC — return partial where
            }

            return strWhere;
        }

        private static SolitaireKapanSummaryReportRowDto MapRow(dynamic row)
        {
            return new SolitaireKapanSummaryReportRowDto
            {
                KapanNo = GetString(row, "KapanNo", "kapanNo"),

                OP_Pcs = GetDouble(row, "OP_Pcs", "op_Pcs", "op_pcs"),
                OP_Carats = GetDouble(row, "OP_Carats", "op_Carats", "op_carats"),
                OP_Amount = GetDouble(row, "OP_Amount", "op_Amount", "op_amount"),
                OP_AvgRate = GetDouble(row, "OP_AvgRate", "op_AvgRate", "op_avgrate"),
                OP_AvgRapa = GetDouble(row, "OP_AvgRapa", "op_AvgRapa", "op_avgrapa"),
                OP_AvgBack = GetDouble(row, "OP_AvgBack", "op_AvgBack", "op_avgback"),

                Sale_Pcs = GetDouble(row, "Sale_Pcs", "sale_Pcs", "sale_pcs"),
                Sale_Carats = GetDouble(row, "Sale_Carats", "sale_Carats", "sale_carats"),
                Sale_Amount = GetDouble(row, "Sale_Amount", "sale_Amount", "sale_amount"),
                Sale_AvgRate = GetDouble(row, "Sale_AvgRate", "sale_AvgRate", "sale_avgrate"),
                Sale_AvgRapa = GetDouble(row, "Sale_AvgRapa", "sale_AvgRapa", "sale_avgrapa"),
                Sale_AvgBack = GetDouble(row, "Sale_AvgBack", "sale_AvgBack", "sale_avgback"),

                Stock_Pcs = GetDouble(row, "Stock_Pcs", "stock_Pcs", "stock_pcs"),
                Stock_Carats = GetDouble(row, "Stock_Carats", "stock_Carats", "stock_carats"),
                Stock_Amount = GetDouble(row, "Stock_Amount", "stock_Amount", "stock_amount"),
                Stock_AvgRate = GetDouble(row, "Stock_AvgRate", "stock_AvgRate", "stock_avgrate"),
                Stock_AvgRapa = GetDouble(row, "Stock_AvgRapa", "stock_AvgRapa", "stock_avgrapa"),
                Stock_AvgBack = GetDouble(row, "Stock_AvgBack", "stock_AvgBack", "stock_avgback"),

                MemoOut_Pcs = GetDouble(row, "MemoOut_Pcs", "memoOut_Pcs", "memoout_pcs"),
                MemoOut_Carats = GetDouble(row, "MemoOut_Carats", "memoOut_Carats", "memoout_carats"),
                MemoOut_Amount = GetDouble(row, "MemoOut_Amount", "memoOut_Amount", "memoout_amount"),
                MemoOut_AvgRate = GetDouble(row, "MemoOut_AvgRate", "memoOut_AvgRate", "memoout_avgrate"),
                MemoOut_AvgRapa = GetDouble(row, "MemoOut_AvgRapa", "memoOut_AvgRapa", "memoout_avgrapa"),
                MemoOut_AvgBack = GetDouble(row, "MemoOut_AvgBack", "memoOut_AvgBack", "memoout_avgback")
            };
        }

        private static object? GetRawValue(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
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

        private static double GetDouble(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            double n;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n)
                ? n
                : 0;
        }
    }
}
