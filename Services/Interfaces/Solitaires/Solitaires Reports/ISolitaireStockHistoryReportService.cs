using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireStockHistoryReportService
    {
        Task<IEnumerable<SolitaireStockHistoryReportResponseDTO>> GetStockHistoryReportAsync(
            SolitaireStockHistoryReportDto filter,
            string companyId);
    }
}
