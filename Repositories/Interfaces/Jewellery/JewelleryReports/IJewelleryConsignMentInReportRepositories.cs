using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryConsignMentInReportRepository
    {
        Task<JewelleryConsignMentInReportDropdownsDto> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewelleryConsignMentInReportRowDto>> GetReportAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId);

        Task<IEnumerable<JewelleryConsignMentInSummaryRowDto>> GetSummaryAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId);
    }
}
