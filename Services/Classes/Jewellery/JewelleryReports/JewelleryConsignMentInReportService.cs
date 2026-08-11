using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryConsignMentInReportService : IJewelleryConsignMentInReportService
    {
        private readonly IJewelleryConsignMentInReportRepository _repository;

        public JewelleryConsignMentInReportService(IJewelleryConsignMentInReportRepository repository)
        {
            _repository = repository;
        }

        public Task<JewelleryConsignMentInReportDropdownsDto> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<JewelleryConsignMentInReportRowDto>> GetReportAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }

        public Task<IEnumerable<JewelleryConsignMentInSummaryRowDto>> GetSummaryAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId)
        {
            return _repository.GetSummaryAsync(filter, companyId);
        }
    }
}
