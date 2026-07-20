using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;

namespace Dia_CoreAPI.Services.Classes.Dashboard
{
    public class SaleAnalysisReportService : ISaleAnalysisReportService
    {
        private readonly ISaleAnalysisReportRepository _repository;

        public SaleAnalysisReportService(ISaleAnalysisReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            return await _repository.GetDropdownDataAsync(companyId);
        }

        public async Task<List<SaleAnalysisReportDto>> GetSaleAnalysisReportAsync(SaleAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetSaleAnalysisReportAsync(filter, companyId);
        }

        public async Task<List<SaleAnalysisReportDto>> GetSaleAnalysisPivotDataAsync(SaleAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetSaleAnalysisPivotDataAsync(filter, companyId);
        }
    }
}
