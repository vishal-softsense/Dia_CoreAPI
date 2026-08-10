using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports
{
    public interface ILedgerReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetTransactionDropDownAsync(string companyId);
        Task<int> GetAccountCodeAsync(string whereClause, string companyId);
        Task<IEnumerable<DualLedgerRowDTO>> GetLedgerDataDatewiseAsync(int partyCode, DateTime fromDate, DateTime toDate, string companyId);
        Task<IEnumerable<LedgerListDetailDTO>> GetLedgerDataList1Async(string fromDate, string toDate, int partyCode, string companyId);
        Task<decimal> GetFinalAmountAsync(string toDate, string whereClause, string companyId);
        Task<LedgerAccountAddressDTO?> GetAccountAddressAsync(string whereClause, string companyId);
    }
}