using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables
{
    public interface IBrokerageOutstandingReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<BrokerageOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId);
        Task<List<BrokerageOutstandingReportResultDto>> GetBrokerageOutstandingReportAsync(
            BrokerageOutstandingReportFilterDto filter,
            string companyId);
    }
}
