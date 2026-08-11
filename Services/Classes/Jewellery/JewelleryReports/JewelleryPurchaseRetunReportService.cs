using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryPurchaseRetunReportService : IJewelleryPurchaseRetunReportService
    {
        private readonly IJewelleryPurchaseRetunReportRepository _repository;

        public JewelleryPurchaseRetunReportService(IJewelleryPurchaseRetunReportRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<JewelleryPurchaseRetunReportRowDto>> GetReportAsync(
            JewelleryPurchaseRetunReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }
    }
}
