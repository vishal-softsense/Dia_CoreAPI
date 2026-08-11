using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewellerySaleReportService : IJewellerySaleReportService
    {
        private readonly IJewellerySaleReportRepository _repository;

        public JewellerySaleReportService(IJewellerySaleReportRepository repository)
        {
            _repository = repository;
        }

        public Task<JewellerySaleReportDropdownsDto> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<JewellerySaleReportRowDto>> GetReportAsync(
            JewellerySaleReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }
    }
}
