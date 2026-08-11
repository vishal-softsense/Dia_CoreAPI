using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports
{
    public interface ITrialBalanceSheetReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<TrialBalanceSheetReportDTO>> GetTrialBalanceSheetReportAsync(DateTime toDate, string companyId);
    }
}
