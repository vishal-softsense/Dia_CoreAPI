using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Dashboard
{
    public interface IMonthAnalysisReportRepository
    {
        Task<List<MonthAnalysisReportDto>> GetMonthAnalysisReportAsync(MonthAnalysisRequestDto filter, string companyId);
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
    }
}
