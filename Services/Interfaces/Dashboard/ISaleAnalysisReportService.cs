using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Dashboard
{
    public interface ISaleAnalysisReportService
    {
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
        Task<List<SaleAnalysisReportDto>> GetSaleAnalysisReportAsync(SaleAnalysisRequestDto filter, string companyId);
        Task<List<SaleAnalysisReportDto>> GetSaleAnalysisPivotDataAsync(SaleAnalysisRequestDto filter, string companyId);
    }
}
