using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports
{
    public interface IBalanceSheetReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BalanceSheetReportDTO>> GetBalanceSheetReportAsync(
            BalanceSheetReportFilterDTO filter,
            bool hideZeroBalance,
            string companyId);
    }
}
