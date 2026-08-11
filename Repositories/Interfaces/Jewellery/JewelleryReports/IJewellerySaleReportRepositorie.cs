using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewellerySaleReportRepository
    {
        Task<JewellerySaleReportDropdownsDto> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewellerySaleReportRowDto>> GetReportAsync(
            JewellerySaleReportFilterDto filter,
            string companyId);
    }
}
