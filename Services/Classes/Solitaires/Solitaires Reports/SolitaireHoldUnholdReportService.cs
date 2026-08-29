using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireHoldUnholdReportService : ISolitaireHoldUnholdReportService
    {
        private readonly ISolitaireHoldUnholdReportRepository _repository;

        public SolitaireHoldUnholdReportService(ISolitaireHoldUnholdReportRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<SolitaireHoldUnholdReportRowDto>> SearchAsync(
            SolitaireHoldUnholdReportFilterDto filter,
            string companyId)
        {
            return _repository.SearchAsync(filter, companyId);
        }

        public Task<bool> HoldRestoreAsync(SolitaireHoldUnholdActionDto action, string companyId)
        {
            return _repository.HoldRestoreAsync(action, companyId);
        }
    }
}
