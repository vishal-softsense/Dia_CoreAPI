using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Services.Classes.Reports.Financial_Reports
{
    public class LedgerReportService : ILedgerReportService
    {
        private static readonly string[] JvDateFormats =
        {
            "dd-MM-yyyy", "dd/MM/yyyy", "dd-MMM-yyyy", "yyyy-MM-dd",
            "dd MMM yyyy", "dd-MMM-yy", "MM/dd/yyyy", "yyyy-MM-ddTHH:mm:ss"
        };

        private readonly ILedgerReportRepository _repository;

        public LedgerReportService(ILedgerReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<LedgerReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var dataList = (await _repository.GetTransactionDropDownAsync(companyId)).ToList();

            return new LedgerReportDropdownsDTO
            {
                Currency = dataList.Where(x => string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase)).ToList()
            };
        }

        public async Task<IEnumerable<DualLedgerRowDTO>> GetLedgerReportViewsAsync(
            LedgerReportFilterDTO filter,
            string companyId)
        {
            return await _repository.GetLedgerDataDatewiseAsync(
                filter.Party,
                filter.FromDate,
                filter.ToDate,
                companyId);
        }

        public async Task<IEnumerable<LedgerListDetailDTO>> GetLedgerReporList1Async(
            LedgerReportFilterDTO filter,
            string companyId)
        {
            if (!string.IsNullOrWhiteSpace(filter.AccountName))
            {
                // Match MVC AccReportController GetLedgerReporList1 account lookup
                string whereClause = $" And AccGrpName IN(''{filter.AccountName}'')";
                int accountCode = await _repository.GetAccountCodeAsync(whereClause, companyId);
                filter.Party = accountCode;
            }

            // Match MVC date format for GetLedgerSSRS
            string fromDate = filter.FromDate.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
            string toDate = filter.ToDate.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);

            return await _repository.GetLedgerDataList1Async(fromDate, toDate, filter.Party, companyId);
        }

        /// <summary>
        /// Matches MVC AccReportController.LedgerReportPrintJS model build.
        /// </summary>
        public async Task<LedgerPrintDTO> GetLedgerReportPrintAsync(LedgerReportFilterDTO filter, string companyId)
        {
            string fromDate = filter.FromDate.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
            string toDate = filter.ToDate.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
            string balanceToDate = filter.ToDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            // Logical where (repository escapes quotes when embedding in EXEC string — same as MVC ''123'')
            string addressWhere = "and AccGrpCode='" + filter.Party + "'";

            var itemsTask = _repository.GetLedgerDataList1Async(fromDate, toDate, filter.Party, companyId);
            var balanceTask = _repository.GetFinalAmountAsync(balanceToDate, addressWhere, companyId);
            var addressTask = _repository.GetAccountAddressAsync(addressWhere, companyId);

            await Task.WhenAll(itemsTask, balanceTask, addressTask);

            var rows = (await itemsTask).ToList();
            decimal balance = await balanceTask;
            var address = await addressTask;

            string dateRange =
                $"{filter.FromDate:dd/MM/yyyy} to {filter.ToDate:dd/MM/yyyy}";

            var model = new LedgerPrintDTO
            {
                PartyName = filter.PartyName,
                DateRange = dateRange,
                GenerateDate = filter.ToDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                ThrouhDate = dateRange,
                ToDate = filter.ToDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                OpeningBalance = balance,
                BalanceAmt = balance,
                AddressLine1 = address?.AddressLine1,
                AddressLine2 = address?.AddressLine2,
                AddressLine3 = address?.AddressLine3,
                Items = rows.Select(MapPrintItem).ToList()
            };

            return model;
        }

        private static LedgerPrintItemDTO MapPrintItem(LedgerListDetailDTO row)
        {
            return new LedgerPrintItemDTO
            {
                JVDate = FormatPrintJvDate(row.JVDate ?? row.FormattedJvDate),
                VType = row.VType,
                Account = row.Account,
                InvNo = row.InvNo,
                StockDebit = row.StockDebit,
                StockCredit = row.StockCredit,
                StockFinalAmount = row.StockFinalAmount
            };
        }

        private static string? FormatPrintJvDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (DateTime.TryParseExact(
                    raw.Trim(),
                    JvDateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime exact))
            {
                return exact.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }

            return raw.Trim();
        }
    }
}
