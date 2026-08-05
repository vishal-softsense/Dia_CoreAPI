using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryStockReportServices : IJewelleryStockReportServices
    {
        private readonly IJewelleryStockReportRepository _repo;

        public JewelleryStockReportServices(IJewelleryStockReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<JewelleryStockSearchResponseDto>> GetJewelleryStockReportAsync(JewelleryStockSearchFilterDto filter, string companyId)
        {
            return await _repo.GetJewelleryStockReportAsync(filter, companyId);
        }

        public async Task<JewelleryStockDropdownsDto> GetJewelleryStockDropdownsAsync(string companyId)
        {
            return await _repo.GetJewelleryStockDropdownsAsync(companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetStockIdsAsync(string searchText, string companyId)
        {
            return await _repo.GetStockIdsAsync(searchText, companyId);
        }
    }
}
