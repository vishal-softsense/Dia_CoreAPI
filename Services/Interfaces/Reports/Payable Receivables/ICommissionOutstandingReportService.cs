using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables
{
    public interface ICommissionOutstandingReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<CommissionOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId);
        Task<List<CommissionOutstandingReportResultDto>> GetCommissionOutstandingReportAsync(
            CommissionOutstandingReportFilterDto filter,
            string companyId);
    }
}
