using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitarieGradingComparisonReportService : ISolitarieGradingComparisonReportService
    {
        private readonly ISolitarieGradingComparisonReportRepository _repo;

        public SolitarieGradingComparisonReportService(ISolitarieGradingComparisonReportRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repo.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<GradingComparisonReportRowDto>> GetReportAsync(
            GradingComparisonReportFilterDto filter,
            string companyId)
        {
            return _repo.GetReportAsync(filter, companyId);
        }
    }
}
