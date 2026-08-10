using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables
{
    public interface IOutstandingReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<OutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId);
        Task<List<OutstandingReportResultDto>> GetOutstandingReportAsync(OutstandingReportFilterDto filter, string companyId);
    }
}
