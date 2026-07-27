using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireAdminStockReportService : ISolitaireAdminStockReportService
    {
        private readonly ISolitaireAdminStockReportRepository _repo;

        public SolitaireAdminStockReportService(ISolitaireAdminStockReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SolitaireAdminStockReportResponseDTO>> GetAdminStockSearchReportAsync(SolitaireAdminStockReportDto filter, string companyId)
        {
            return await _repo.GetAdminStockSearchReportAsync(filter, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAdminStockSearchDropdownAsync(string companyId)
        {
            return await _repo.GetAdminStockSearchDropdownAsync(companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAdminSolitaireLotNoListAsync(string searchby, string companyId)
        {
            return await _repo.GetAdminSolitaireLotNoListAsync(searchby, companyId);
        }

        public async Task<bool> UpdateLocationAsync(SolitaireAdminLocationUpdateDTO dto, string companyId)
        {
            return await _repo.UpdateLocationAsync(dto, companyId);
        }

        public async Task<bool> UpdateBoxAndGroupAsync(SolitaireAdminBoxGroupUpdateDTO dto, string companyId)
        {
            return await _repo.UpdateBoxAndGroupAsync(dto, companyId);
        }
    }
}
