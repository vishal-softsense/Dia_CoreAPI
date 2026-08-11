using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Classes.Reports.Financial_Reports
{
    public class InvoiceLedgerReportService : IInvoiceLedgerReportService
    {
        private readonly IInvoiceLedgerReportRepository _repository;

        public InvoiceLedgerReportService(IInvoiceLedgerReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public Task<IEnumerable<InvoiceLedgerReportDTO>> GetInvoiceLedgerReportAsync(
            InvoiceLedgerReportFilterDTO filter,
            string companyId)
        {
            var (where, where2) = GenerateInvoiceLedgerWhereClause(filter);
            string date = filter.ToDate?.ToString("yyyy-MM-dd") ?? "";

            return _repository.GetInvoiceLedgerReportAsync(
                where,
                where2,
                date,
                filter.intedatadays ?? "",
                filter.intedataper ?? "",
                companyId);
        }

        /// <summary>
        /// Matches MVC AccReportController.GenerateInvoiceLedgerWhereClause.
        /// </summary>
        public static (string where, string where2) GenerateInvoiceLedgerWhereClause(InvoiceLedgerReportFilterDTO filter)
        {
            string where = "";
            string where2 = "";

            if (filter.FromDate != null && filter.ToDate != null)
            {
                where += $" AND tsp.TrnDate BETWEEN ''{filter.FromDate:yyyy-MM-dd}'' AND ''{filter.ToDate:yyyy-MM-dd}''";
            }

            if (filter.FromDueChkBox == true)
            {
                where += $" AND tsp.DueDate BETWEEN ''{filter.FromDateDue:yyyy-MM-dd}'' AND ''{filter.ToDateDue:yyyy-MM-dd}''";
            }

            if (!string.IsNullOrEmpty(filter.PartyName))
            {
                where2 += $" AND PartyName = ''{EscapeNested(filter.PartyName)}''";
            }

            if (!string.IsNullOrEmpty(filter.BrokerName))
            {
                where2 += $" AND BrokerName = ''{EscapeNested(filter.BrokerName)}''";
            }

            if (!filter.PaidInvoice)
            {
                where2 += @"
                 AND result.Code IN (
                        SELECT tsp.Code
                        FROM TabSalePur tsp
                        LEFT JOIN tabPayRec p 
                            ON tsp.Code = p.InvCode 
                           AND p.DeleteFlag = 0
                        WHERE tsp.DeleteFlag = 0
                        GROUP BY tsp.Code, tsp.StockCurrencyFinalAmount
                        HAVING 
                            ISNULL(SUM(p.StockCurrencyAmount),0) 
                            < tsp.StockCurrencyFinalAmount
                    )";
            }

            if (!string.IsNullOrEmpty(filter.InvNo))
            {
                where2 += $" AND result.InvNo = ''{EscapeNested(filter.InvNo)}'' ";
            }

            if (!string.IsNullOrEmpty(filter.BillNo))
            {
                where2 += $" AND result.BillNo = ''{EscapeNested(filter.BillNo)}'' ";
            }

            return (where, where2);
        }

        private static string EscapeNested(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
