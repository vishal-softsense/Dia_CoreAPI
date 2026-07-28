using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireStockHistoryReportService : ISolitaireStockHistoryReportService
    {
        private readonly ISolitaireStockHistoryReportRepository _repo;

        public SolitaireStockHistoryReportService(ISolitaireStockHistoryReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SolitaireStockHistoryReportResponseDTO>> GetStockHistoryReportAsync(
            SolitaireStockHistoryReportDto filter,
            string companyId)
        {
            var spRows = await _repo.GetStockHistoryReportAsync(filter, companyId);

            // MVC formatting: TrnDate is dd-MM-yyyy
            return spRows.Select(row => new SolitaireStockHistoryReportResponseDTO
            {
                TrnDate = row.Trndate.HasValue ? row.Trndate.Value.ToString("dd-MM-yyyy") : null,
                Trntype = row.Trntype,
                Perticulors = row.RelatedRemark,
                LotNo = row.Lotnosearch,

                InCts = row.InCts,
                InRate = row.InRate,
                InAmount = row.InAmount,

                OutCts = row.OutCts,
                OutRate = row.OutRate,
                OutAmount = row.OutAmount,

                BalCts = row.BalCts,
                BalRate = row.BalRate,
                BalAmount = row.BalAmount,

                invcode = row.Invcode
            }).ToList();
        }
    }
}
