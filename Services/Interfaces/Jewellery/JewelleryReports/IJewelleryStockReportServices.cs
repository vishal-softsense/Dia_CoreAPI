using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryStockReportServices
    {
        Task<IEnumerable<JewelleryStockSearchResponseDto>> GetJewelleryStockReportAsync(JewelleryStockSearchFilterDto filter, string companyId);
        Task<JewelleryStockDropdownsDto> GetJewelleryStockDropdownsAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> GetStockIdsAsync(string searchText, string companyId);
    }
}
