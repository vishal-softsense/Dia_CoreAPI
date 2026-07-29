using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireStockSearchReportRepository
    {
        Task<IEnumerable<SolitaireStockSearchReportResponseDTO>> GetStockSearchReportAsync(SolitaireStockSearchReportDTO filter, string companyId);
        Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> GetSolitaireLotNoListAsync(string searchby, string companyId);
        Task<bool> UpdateLocationAsync(LocationUpdateDTO dto, string companyId);
        Task<bool> UpdateBoxAndGroupAsync(BoxGroupUpdateDTO dto, string companyId);
        Task<bool> SendStockListEmailAsync(SendSolitaireStockEmailDTO dto, string companyId);
        Task<EmailSettingsDTO> GetEmailSettingsAsync(string companyId);
        Task<IEnumerable<SolitaireStockSearchHistoryRowDto>> GetStockSearchHistoryAsync(
            SolitaireStockSearchHistoryFilterDto filter,
            string companyId);
    }
}
