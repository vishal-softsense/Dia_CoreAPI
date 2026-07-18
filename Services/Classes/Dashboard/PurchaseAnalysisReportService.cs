using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;

namespace Dia_CoreAPI.Services.Classes.Dashboard
{
    public class PurchaseAnalysisReportService : IPurchaseAnalysisReportService
    {
        private readonly IPurchaseAnalysisReportRepository _repository;

        public PurchaseAnalysisReportService(IPurchaseAnalysisReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            return await _repository.GetDropdownDataAsync(companyId);
        }

        public async Task<List<PurchaseAnalysisReportDto>> GetPurchaseAnalysisReportAsync(PurchaseAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetPurchaseAnalysisReportAsync(filter, companyId);
        }
    }
}
