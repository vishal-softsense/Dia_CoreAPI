using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Classes.Reports.Financial_Reports
{
    public class BalanceSheetReportService : IBalanceSheetReportService
    {
        private readonly IBalanceSheetReportRepository _repository;

        public BalanceSheetReportService(IBalanceSheetReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public Task<IEnumerable<BalanceSheetReportDTO>> GetBalanceSheetReportAsync(
            BalanceSheetReportFilterDTO filter,
            bool hideZeroBalance,
            string companyId)
        {
            // MVC BalanceSheetController: Value == "true" → Type = "1", else "0"
            string type = hideZeroBalance ? "1" : "0";
            return _repository.GetBalanceSheetReportAsync(
                filter.ToDate,
                type,
                filter.CalType ?? string.Empty,
                companyId);
        }
    }
}
