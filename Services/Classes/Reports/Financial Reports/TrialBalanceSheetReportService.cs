using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Classes.Reports.Financial_Reports
{
    public class TrialBalanceSheetReportService : ITrialBalanceSheetReportService
    {
        private readonly ITrialBalanceSheetReportRepository _repository;

        public TrialBalanceSheetReportService(ITrialBalanceSheetReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public Task<IEnumerable<TrialBalanceSheetReportDTO>> GetTrialBalanceSheetReportAsync(
            TrialBalanceSheetReportFilterDTO filter,
            string companyId)
        {
            return _repository.GetTrialBalanceSheetReportAsync(filter.ToDate, companyId);
        }
    }
}
