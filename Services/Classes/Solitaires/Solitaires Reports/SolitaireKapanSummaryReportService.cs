using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireKapanSummaryReportService : ISolitaireKapanSummaryReportService
    {
        private readonly ISolitaireKapanSummaryReportRepository _repository;

        public SolitaireKapanSummaryReportService(ISolitaireKapanSummaryReportRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<SolitaireKapanSummaryReportRowDto>> GetReportAsync(
            SolitaireKapanSummaryReportFilterDto filter,
            string companyId)
        {
            return _repository.GetReportAsync(filter, companyId);
        }
    }
}
