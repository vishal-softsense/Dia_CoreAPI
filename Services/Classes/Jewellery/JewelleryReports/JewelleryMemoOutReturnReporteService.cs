using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryMemoOutReturnReporteService : IJewelleryMemoOutReturnReporteService
    {
        private readonly IJewelleryMemoOutReturnReporteRepository _repository;

        public JewelleryMemoOutReturnReporteService(IJewelleryMemoOutReturnReporteRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<JewelleryMemoOutReturnReporteRowDto>> GetReportAsync(
            JewelleryMemoOutReturnReporteFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }
    }
}
