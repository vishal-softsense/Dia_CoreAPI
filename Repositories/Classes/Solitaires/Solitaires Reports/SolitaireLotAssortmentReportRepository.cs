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
    public class SolitaireLotAssortmentReportRepository : ISolitaireLotAssortmentReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireLotAssortmentReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireLotAssortmentLotGroupDto>> GetReportAsync(
            SolitaireLotAssortmentReportFilterDto filter,
            string companyId)
        {
            var lotList = (filter?.FLotList ?? new List<string>())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allData = new List<SolitaireLotAssortmentLotGroupDto>();

            if (lotList.Count == 0)
            {
                var allReportData = await GetLotAssortReportAsync("", companyId);

                var groupedLots = allReportData
                    .GroupBy(r => r.FromLotNo)
                    .Select(g => new SolitaireLotAssortmentLotGroupDto
                    {
                        LotNo = string.IsNullOrWhiteSpace(g.Key) ? "Unknown" : g.Key,
                        Data = g.ToList()
                    })
                    .ToList();

                allData.AddRange(groupedLots);
            }
            else
            {
                foreach (var lot in lotList)
                {
                    var fItemCode = await GetMaxItemCodeAsync(lot, companyId);
                    var where = "";

                    if (!string.IsNullOrWhiteSpace(fItemCode) &&
                        int.TryParse(fItemCode, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxFItemCode) &&
                        maxFItemCode > 0)
                    {
                        where = " AND tspt.FItemCode = " + maxFItemCode;
                    }

                    var reportData = await GetLotAssortReportAsync(where, companyId);
                    allData.Add(new SolitaireLotAssortmentLotGroupDto
                    {
                        LotNo = lot,
                        Data = reportData.ToList()
                    });
                }
            }

            return allData;
        }

        private async Task<IEnumerable<SolitaireLotAssortmentReportRowDto>> GetLotAssortReportAsync(
            string where,
            string companyId)
        {
            var safeWhere = (where ?? "").Replace("'", "''");
            var sql = $"EXEC [SP_LotAssortmentReport] @where = '{safeWhere}' ";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        private async Task<string> GetMaxItemCodeAsync(string lotNo, string companyId)
        {
            var safeLot = (lotNo ?? "").Replace("'", "''");
            var param = new Dictionary<string, object>
            {
                { "@Type", "ItemCode" },
                { "@Where", " AND LotNo = '" + safeLot + "'" }
            };

            var result = await _db.SelectDataFromSPSingle<string>("GetALLCommonData", param, companyId);
            return result ?? "";
        }

        private static SolitaireLotAssortmentReportRowDto MapRow(dynamic row)
        {
            return new SolitaireLotAssortmentReportRowDto
            {
                LotNo = GetString(row, "LotNo", "lotNo"),
                FromLotCode = GetString(row, "FromLotCode", "fromLotCode"),
                Shape = GetString(row, "Shape", "shape"),
                Color = GetString(row, "Color", "color"),
                Size = GetString(row, "Size", "size"),
                Clarity = GetString(row, "Clarity", "clarity"),
                TransferDate = GetString(row, "TransferDate", "transferDate"),
                FromLotNo = GetString(row, "FromLotNo", "fromLotNo"),
                ToLotNo = GetString(row, "ToLotNo", "toLotNo"),
                Carats = GetDouble(row, "Carats", "carats"),
                Rate = GetDouble(row, "Rate", "rate"),
                AmountCl = GetDouble(row, "AmountCl", "amountCl")
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
