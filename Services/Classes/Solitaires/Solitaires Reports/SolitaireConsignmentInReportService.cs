using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireConsignmentInReportService : ISolitaireConsignmentInReportService
    {
        private readonly ISolitaireConsignmentInReportRepository _repository;

        public SolitaireConsignmentInReportService(ISolitaireConsignmentInReportRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repository.GetDropdownsAsync(companyId);
        }

        public Task<IEnumerable<SolitaireConsignmentInReportRowDto>> GetReportAsync(
            SolitaireConsignmentInReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }

        public Task<IEnumerable<SolitaireConsignmentInSummaryRowDto>> GetSummaryAsync(
            SolitaireConsignmentInReportFilterDto filter,
            string companyId)
        {
            return _repository.GetSummaryAsync(filter, companyId);
        }
    }
}
