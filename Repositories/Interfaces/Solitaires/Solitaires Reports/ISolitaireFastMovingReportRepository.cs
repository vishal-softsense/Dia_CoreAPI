using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireFastMovingReportRepository
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);
        Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportAsync(SolitaireFastMovingReportFilterDto filter, string companyId);
        Task<IEnumerable<SolitaireFastMovingReportRowDto>> GetReportDisplayAsync(SolitaireFastMovingReportFilterDto filter, string companyId);
    }
}
