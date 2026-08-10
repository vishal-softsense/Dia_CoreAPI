using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables
{
    public interface ILoanOutstandingReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<LoanOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId);
        Task<List<LoanOutstandingReportResultDto>> GetLoanOutstandingReportAsync(
            LoanOutstandingReportFilterDto filter,
            string companyId);
    }
}
