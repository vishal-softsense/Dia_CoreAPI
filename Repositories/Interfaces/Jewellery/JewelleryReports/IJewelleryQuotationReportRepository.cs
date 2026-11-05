using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryQuotationReportRepository
    {
        Task<IEnumerable<JewelleryStockSearchResponseDto>> GetJewelleryQuotationReportAsync(JewelleryStockSearchFilterDto filter, string companyId);
        Task<JewelleryStockDropdownsDto> GetJewelleryQuotationDropdownsAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> GetStockIdsAsync(string searchText, string companyId);
    }
}
