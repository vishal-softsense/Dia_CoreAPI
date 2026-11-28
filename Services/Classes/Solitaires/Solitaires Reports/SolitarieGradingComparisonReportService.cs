using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitarieGradingComparisonReportService : ISolitarieGradingComparisonReportService
    {
        private readonly ISolitarieGradingComparisonReportRepository _repo;

        public SolitarieGradingComparisonReportService(ISolitarieGradingComparisonReportRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repo.GetDropdownsAsync(companyId);
        }

        public async Task<object> GetReportAsync(GradingComparisonReportFilterDto filter, string companyId)
        {
            var rows = (await _repo.GetReportAsync(filter, companyId)).ToList();

            if (filter?.LabWeightDifference == true)
            {
                var lotNos = rows
                    .Select(GetLotKey)
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var lotsWithDiffEntry = await _repo.GetLotsWithLabWeightDiffEntryAsync(lotNos, companyId);
                return BuildLabWeightDiffRows(rows, lotsWithDiffEntry);
            }

            return rows;
        }

        public Task<MakeLabWeightDiffEntryResultDto> MakeLabWeightDiffEntryAsync(
            MakeLabWeightDiffEntryRequestDto request,
            string companyId)
        {
            return _repo.MakeLabWeightDiffEntryAsync(request, companyId);
        }

        /// <summary>
        /// Builds Lab Weight Difference rows from grading comparison pairs.
        /// Uses LabW as Old Weight when a Lab Weight Diff Entry exists for the LotNo;
        /// otherwise uses the original first-row Carats as Old Weight.
        /// Second-row Carats is New Weight.
        /// </summary>
        internal static List<LabWeightDiffRowDto> BuildLabWeightDiffRows(
            IEnumerable<GradingComparisonReportRowDto> reportRows,
            ISet<string>? lotsWithDiffEntry = null)
        {
            var result = new List<LabWeightDiffRowDto>();
            if (reportRows == null)
                return result;

            var list = reportRows.ToList();
            int i = 0;
            while (i < list.Count)
            {
                var lotKey = GetLotKey(list[i]);
                var group = new List<GradingComparisonReportRowDto>();

                while (i < list.Count && GetLotKey(list[i]) == lotKey)
                {
                    group.Add(list[i]);
                    i++;
                }

                if (string.IsNullOrWhiteSpace(lotKey) || group.Count < 2)
                    continue;

                var hasEntry = lotsWithDiffEntry != null && lotsWithDiffEntry.Contains(lotKey);
                var rawLabW = group.Select(r => r.LabW).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                var labW = Math.Round(ParseCarats(rawLabW), 3, MidpointRounding.AwayFromZero);
                var originalCarats = Math.Round(ParseCarats(group[0].carats), 3, MidpointRounding.AwayFromZero);

                var oldWeight = hasEntry ? labW : originalCarats;
                var newWeight = Math.Round(ParseCarats(group[1].carats), 3, MidpointRounding.AwayFromZero);
                var difference = Math.Round(Math.Abs(oldWeight - newWeight), 3, MidpointRounding.AwayFromZero);

                result.Add(new LabWeightDiffRowDto
                {
                    LotNo = lotKey,
                    LabW = labW,
                    OldWeight = oldWeight,
                    NewWeight = newWeight,
                    Difference = difference,
                    HasLabWeightDiffEntry = hasEntry
                });
            }

            return result;
        }

        private static string GetLotKey(GradingComparisonReportRowDto row)
        {
            if (!string.IsNullOrWhiteSpace(row.LotNo))
                return row.LotNo.Trim();
            if (!string.IsNullOrWhiteSpace(row.Ref))
                return row.Ref.Trim();
            return string.Empty;
        }

        private static decimal ParseCarats(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
                return inv;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var cur))
                return cur;

            return 0m;
        }
    }
}
