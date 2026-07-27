using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireAdminStockReportService
    {
        Task<IEnumerable<SolitaireAdminStockReportResponseDTO>> GetAdminStockSearchReportAsync(SolitaireAdminStockReportDto filter, string companyId);
        Task<IEnumerable<BasicDataDto>> GetAdminStockSearchDropdownAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> GetAdminSolitaireLotNoListAsync(string searchby, string companyId);
        Task<bool> UpdateLocationAsync(SolitaireAdminLocationUpdateDTO dto, string companyId);
        Task<bool> UpdateBoxAndGroupAsync(SolitaireAdminBoxGroupUpdateDTO dto, string companyId);
    }
}
