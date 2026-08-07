using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireFastMovingReportRepository : ISolitaireFastMovingReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireFastMovingReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAdminStockSearch",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportAsync(
            SolitaireFastMovingReportFilterDto filter,
            string companyId)
        {
            var where = BuildFilterWhere(filter);
            var sql = $"EXEC [SP_FastMovingReport] @Where = '{where}' ,@FDate = '{filter.FromDate}' ,@TDate= '{filter.ToDate}',@Type= 'Result'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        public async Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportDisplayAsync(
            SolitaireFastMovingReportFilterDto filter,
            string companyId)
        {
            var where = BuildFilterWhere(filter);

            var colStr = "";
            if (filter.Color != null && filter.Color.Count > 0)
            {
                string colorList = "";
                filter.Color.ForEach(el => colorList += "''" + el + "'',");
                if (colorList.Trim().Length > 1)
                    colorList = colorList.Remove(colorList.Length - 1);
                colStr = "im.colorid  In ( " + colorList + ")";
            }

            var claStr = "";
            if (filter.Clarity != null && filter.Clarity.Count > 0)
            {
                string clarityList = "";
                filter.Clarity.ForEach(el => clarityList += "''" + el + "'',");
                if (clarityList.Trim().Length > 1)
                    clarityList = clarityList.Remove(clarityList.Length - 1);
                claStr = "im.clarityId  In ( " + clarityList + ")";
            }

            var sql = $"EXEC [SP_FastMovingReport] @Color = '{colStr}' ,@Clarity = '{claStr}' ,@Where= '{where}',@FDate = '{filter.FromDate}' ,@TDate= '{filter.ToDate}',@Type= 'Display'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        private static string BuildFilterWhere(SolitaireFastMovingReportFilterDto filter)
        {
            var where = "";
            if (filter == null) return where;

            AppendInClause(ref where, filter.Shape, "im.ShapeId");
            AppendInClause(ref where, filter.Size, "im.SizeId");
            AppendInClause(ref where, filter.Polish, "im.PolishId");
            AppendInClause(ref where, filter.Proportion, "im.PropId");
            AppendInClause(ref where, filter.Symmetry, "im.SymId");
            AppendInClause(ref where, filter.LAB, "im.certId");
            AppendInClause(ref where, filter.Fluorescence, "im.FLId");

            return where;
        }

        private static void AppendInClause(ref string where, List<string>? values, string column)
        {
            if (values == null || values.Count == 0)
                return;

            try
            {
                string joined = "";
                values.ForEach(el => joined += "''" + el + "'',");
                if (joined.Trim().Length > 1)
                    joined = joined.Remove(joined.Length - 1);

                if (!string.IsNullOrEmpty(joined))
                    where += " And " + column + " In ( " + joined + ")";
            }
            catch
            {
                // Swallow
            }
        }

        private static SolitaireFastMovingReportRowDto MapReportRow(dynamic row)
        {
            return new SolitaireFastMovingReportRowDto
            {
                ClarityName = GetString(row, "ClarityName", "clarityname"),
                ColorName = GetString(row, "ColorName", "colorname"),
                Opening_Cts = GetDouble(row, "Opening_Cts", "opening_cts"),
                Opening_Pcs = GetDouble(row, "Opening_Pcs", "opening_pcs"),
                Opening_pcsper = GetDouble(row, "Opening_pcsper", "opening_pcsper"),
                Opening_USDperCts = GetDouble(row, "Opening_USDperCts", "opening_usdpercts"),
                Opening_DiscPct = GetDouble(row, "Opening_DiscPct", "opening_discpct"),
                Opening_Age_Total = GetDouble(row, "Opening_Age_Total", "opening_age_total"),
                Purchase_Cts = GetDouble(row, "Purchase_Cts", "purchase_cts"),
                Purchase_Pcs = GetDouble(row, "Purchase_Pcs", "purchase_pcs"),
                Purchase_pcsper = GetDouble(row, "Purchase_pcsper", "purchase_pcsper"),
                Purchase_USDperCts = GetDouble(row, "Purchase_USDperCts", "purchase_usdpercts"),
                Purchase_DiscPct = GetDouble(row, "Purchase_DiscPct", "purchase_discpct"),
                Purchase_Age_Total = GetDouble(row, "Purchase_Age_Total", "purchase_age_total"),
                Sale_Cts = GetDouble(row, "Sale_Cts", "sale_cts"),
                Sale_Pcs = GetDouble(row, "Sale_Pcs", "sale_pcs"),
                Sale_pcsper = GetDouble(row, "Sale_pcsper", "sale_pcsper"),
                Sale_USDperCts = GetDouble(row, "Sale_USDperCts", "sale_usdpercts"),
                Sale_DiscPct = GetDouble(row, "Sale_DiscPct", "sale_discpct"),
                Sale_Age_Total = GetDouble(row, "Sale_Age_Total", "sale_age_total"),
                Closing_Cts = GetDouble(row, "Closing_Cts", "closing_cts"),
                Closing_Pcs = GetDouble(row, "Closing_Pcs", "closing_pcs"),
                Closing_pcsper = GetDouble(row, "Closing_pcsper", "closing_pcsper"),
                Closing_USDperCts = GetDouble(row, "Closing_USDperCts", "closing_usdpercts"),
                Closing_DiscPct = GetDouble(row, "Closing_DiscPct", "closing_discpct"),
                Closing_Age_Total = GetDouble(row, "Closing_Age_Total", "closing_age_total"),
                LotNo = GetString(row, "LotNo", "lotno"),
                TrnType = GetString(row, "TrnType", "trntype"),
                SPdate = GetString(row, "SPdate", "spdate"),
                pcs = GetInt(row, "pcs", "pcs"),
                Carats = GetDouble(row, "Carats", "carats"),
                mrp = GetDouble(row, "mrp", "mrp"),
                Amount = GetDouble(row, "Amount", "amount"),
                Disc = GetDouble(row, "Disc", "disc"),
                Age = GetInt(row, "Age", "age")
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

        private static int GetInt(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        private static double GetDouble(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out double n) ? n : 0;
        }
    }
}
