using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryConsignMentInReportService
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
