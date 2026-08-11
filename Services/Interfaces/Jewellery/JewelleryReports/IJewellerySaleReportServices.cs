using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewellerySaleReportService
    {
        Task<JewellerySaleReportDropdownsDto> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewellerySaleReportRowDto>> GetReportAsync(
            JewellerySaleReportFilterDto filter,
            string companyId);
    }
}
