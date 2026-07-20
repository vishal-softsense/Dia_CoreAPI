using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;

namespace Dia_CoreAPI.Services.Classes.Dashboard
{
    public class StockAnalysisReportService : IStockAnalysisReportService
    {
        private readonly IStockAnalysisReportRepository _repository;

        public StockAnalysisReportService(IStockAnalysisReportRepository repository)
        {
            _repository = repository;
        }

        public Task<List<BasicData>> GetDropdownDataAsync(string companyId)
            => _repository.GetDropdownDataAsync(companyId);

        public Task<List<StockAnalysisReportDto>> GetReportAsync(StockAnalysisRequestDto filter, string companyId)
            => _repository.GetReportAsync(filter, companyId);

        public Task<List<StockAnalysisPivotByFilterDto>> GetPivotByFilterAsync(StockAnalysisRequestDto filter, string companyId)
            => _repository.GetPivotByFilterAsync(filter, companyId);

        public Task<List<StockAnalysisPivotFixedDto>> GetPivotFixedAsync(StockAnalysisRequestDto filter, string companyId)
            => _repository.GetPivotFixedAsync(filter, companyId);
    }
}
