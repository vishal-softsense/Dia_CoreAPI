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
            var rows = await _repo.GetReportAsync(filter, companyId);

            if (filter?.LabWeightDifference == true)
                return BuildLabWeightDiffRows(rows);

            return rows;
        }

        public Task<MakeLabWeightDiffEntryResultDto> MakeLabWeightDiffEntryAsync(
            MakeLabWeightDiffEntryRequestDto request,
            string companyId)
        {
            return _repo.MakeLabWeightDiffEntryAsync(request, companyId);
        }

        /// <summary>
        /// Builds Lab Weight Difference rows from grading comparison pairs
        /// (first row carats = Old Weight, second row carats = New Weight per LotNo).
        /// </summary>
        internal static List<LabWeightDiffRowDto> BuildLabWeightDiffRows(
            IEnumerable<GradingComparisonReportRowDto> reportRows)
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

                var oldWeight = ParseCarats(group[0].carats);
                var newWeight = ParseCarats(group[1].carats);
                var difference = Math.Round(Math.Abs(oldWeight - newWeight), 3, MidpointRounding.AwayFromZero);

                result.Add(new LabWeightDiffRowDto
                {
                    LotNo = lotKey,
                    OldWeight = Math.Round(oldWeight, 3, MidpointRounding.AwayFromZero),
                    NewWeight = Math.Round(newWeight, 3, MidpointRounding.AwayFromZero),
                    Difference = difference
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
