using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports
{
    public interface ILedgerReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<LedgerReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<DualLedgerRowDTO>> GetLedgerReportViewsAsync(LedgerReportFilterDTO filter, string companyId);
        Task<IEnumerable<LedgerListDetailDTO>> GetLedgerReporList1Async(LedgerReportFilterDTO filter, string companyId);
        Task<LedgerPrintDTO> GetLedgerReportPrintAsync(LedgerReportFilterDTO filter, string companyId);
    }
}