using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireConsignmentInReportService
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);

        Task<IEnumerable<SolitaireConsignmentInReportRowDto>> GetReportAsync(
            SolitaireConsignmentInReportFilterDto filter,
            string companyId);

        Task<IEnumerable<SolitaireConsignmentInSummaryRowDto>> GetSummaryAsync(
            SolitaireConsignmentInReportFilterDto filter,
            string companyId);
    }
}
