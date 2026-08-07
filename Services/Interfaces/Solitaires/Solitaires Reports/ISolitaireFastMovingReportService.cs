using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireFastMovingReportService
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);
        Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportAsync(SolitaireFastMovingReportFilterDto filter, string companyId);
        Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportDisplayAsync(SolitaireFastMovingReportFilterDto filter, string companyId);
    }
}
