using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewellerySaleReturnReportService : IJewellerySaleReturnReportService
    {
        private readonly IJewellerySaleReturnReportRepository _repository;

        public JewellerySaleReturnReportService(IJewellerySaleReturnReportRepository repository)
        {
            _repository = repository;
        }

        public Task<JewellerySaleReturnReportDropdownsDto> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<JewellerySaleReturnReportRowDto>> GetReportAsync(
            JewellerySaleReturnReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }
    }
}
