using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports
{
    public interface IInvoiceLedgerReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        Task<IEnumerable<InvoiceLedgerReportDTO>> GetInvoiceLedgerReportAsync(
            string where,
            string where2,
            string date,
            string calculationDays,
            string calculationPer,
            string companyId);
    }
}
