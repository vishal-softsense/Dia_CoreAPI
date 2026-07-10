using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireStockSearchReportService : ISolitaireStockSearchReportService
    {
        private readonly ISolitaireStockSearchReportRepository _repo;

        public SolitaireStockSearchReportService(ISolitaireStockSearchReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SolitaireStockSearchReportResponseDTO>> GetStockSearchReportAsync(SolitaireStockSearchReportDTO filter, string companyId)
        {
            return await _repo.GetStockSearchReportAsync(filter, companyId);
        }



        public async Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            return await _repo.GetStockSearchDropdownAsync(companyId);
        }

        public async Task<bool> UpdateLocationAsync(LocationUpdateDTO dto, string companyId)
        {
            return await _repo.UpdateLocationAsync(dto, companyId);
        }

        public async Task<bool> UpdateBoxAndGroupAsync(BoxGroupUpdateDTO dto, string companyId)
        {
            return await _repo.UpdateBoxAndGroupAsync(dto, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetSolitaireLotNoListAsync(string searchby, string companyId)
        {
            return await _repo.GetSolitaireLotNoListAsync(searchby, companyId);
        }

        public async Task<bool> SendStockListEmailAsync(SendSolitaireStockEmailDTO dto, string companyId)
        {
            return await _repo.SendStockListEmailAsync(dto, companyId);
        }

        public async Task<EmailSettingsDTO> GetEmailSettingsAsync(string companyId)
        {
            return await _repo.GetEmailSettingsAsync(companyId);
        }
    }
}
