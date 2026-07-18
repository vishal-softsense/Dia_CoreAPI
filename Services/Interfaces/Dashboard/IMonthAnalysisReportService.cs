using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Dashboard
{
    public interface IMonthAnalysisReportService
    {
        Task<List<MonthAnalysisReportDto>> GetMonthAnalysisReportAsync(MonthAnalysisRequestDto filter, string companyId);
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
    }
}
