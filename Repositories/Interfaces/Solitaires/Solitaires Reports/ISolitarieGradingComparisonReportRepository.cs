using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitarieGradingComparisonReportRepository
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);
        Task<IEnumerable<GradingComparisonReportRowDto>> GetReportAsync(GradingComparisonReportFilterDto filter, string companyId);
        Task<MakeLabWeightDiffEntryResultDto> MakeLabWeightDiffEntryAsync(MakeLabWeightDiffEntryRequestDto request, string companyId);
    }
}
