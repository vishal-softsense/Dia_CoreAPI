using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports
{
    public interface IMonthExpenseReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<MonthExpenseReportDTO>> GetMonthExpenseReportAsync(string companyId);
    }
}
