using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireHoldUnholdReportService
    {
        Task<IEnumerable<SolitaireHoldUnholdReportRowDto>> SearchAsync(
            SolitaireHoldUnholdReportFilterDto filter,
            string companyId);

        Task<bool> HoldRestoreAsync(SolitaireHoldUnholdActionDto action, string companyId);
    }
}
