using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Classes.Reports.Financial_Reports
{
    public class ProfitLossReportService : IProfitLossReportService
    {
        private readonly IProfitLossReportRepository _repository;

        public ProfitLossReportService(IProfitLossReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReportAsync(
            ProfitLossReportFilterDTO filter,
            bool hideZeroBalance,
            string companyId)
        {
            // MVC TradingAccountController: value == "true" → type = "1", else "0"
            string type = hideZeroBalance ? "1" : "0";
            return _repository.GetProfitLossReportAsync(
                filter.ToDate,
                type,
                filter.CalType ?? string.Empty,
                companyId);
        }
    }
}
