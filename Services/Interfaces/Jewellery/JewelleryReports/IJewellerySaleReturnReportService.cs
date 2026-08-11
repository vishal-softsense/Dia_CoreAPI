using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewellerySaleReturnReportService
    {
        Task<JewellerySaleReturnReportDropdownsDto> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewellerySaleReturnReportRowDto>> GetReportAsync(
            JewellerySaleReturnReportFilterDto filter,
            string companyId);
    }
}
