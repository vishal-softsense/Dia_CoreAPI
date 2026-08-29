using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireHoldUnholdReportRepository
    {
        Task<IEnumerable<SolitaireHoldUnholdReportRowDto>> SearchAsync(
            SolitaireHoldUnholdReportFilterDto filter,
            string companyId);

        Task<bool> HoldRestoreAsync(SolitaireHoldUnholdActionDto action, string companyId);
    }
}
