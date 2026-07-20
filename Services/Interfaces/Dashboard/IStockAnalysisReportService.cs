using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Dashboard
{
    public interface IStockAnalysisReportService
    {
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
        Task<List<StockAnalysisReportDto>> GetReportAsync(StockAnalysisRequestDto filter, string companyId);
        Task<List<StockAnalysisPivotByFilterDto>> GetPivotByFilterAsync(StockAnalysisRequestDto filter, string companyId);
        Task<List<StockAnalysisPivotFixedDto>> GetPivotFixedAsync(StockAnalysisRequestDto filter, string companyId);
    }
}
