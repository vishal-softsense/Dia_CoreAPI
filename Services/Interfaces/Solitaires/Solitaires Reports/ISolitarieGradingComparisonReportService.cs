using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitarieGradingComparisonReportService
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);
        Task<IEnumerable<GradingComparisonReportRowDto>> GetReportAsync(GradingComparisonReportFilterDto filter, string companyId);
    }
}
