using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables;
using Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.Services.Classes.Reports.Payable_Receivables
{
    public class CommissionOutstandingReportService : ICommissionOutstandingReportService
    {
        private readonly ICommissionOutstandingReportRepository _repository;

        public CommissionOutstandingReportService(ICommissionOutstandingReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<CommissionOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetDropdownDataAsync(companyId);

            var currencies = rawData
                .Where(x => string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase))
                .Select(x => new BasicDataDto
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = x.Type,
                    Priority = x.Priority,
                    TextName = x.TextName
                })
                .ToList();

            var salesPersons = await _repository.GetSalesPersonsAsync(companyId);

            return new CommissionOutstandingReportDropdownsDto
            {
                Currency = currencies,
                SalesPersons = salesPersons
            };
        }

        public async Task<List<CommissionOutstandingReportResultDto>> GetCommissionOutstandingReportAsync(
            CommissionOutstandingReportFilterDto filter,
            string companyId)
        {
            long mCurCode = await _repository.GetCurCodeAsync("MasterCurrencyRate=1", companyId);
            long sCurCode = await _repository.GetCurCodeAsync("StockCurrencyRate=1", companyId);

            string curCode = string.IsNullOrEmpty(filter.Currency) ? sCurCode.ToString() : filter.Currency;
            string curMasterOperator = await _repository.GetMCurOperatorAsync(curCode, companyId);

            if ((filter.Brokerage == true && filter.Summary != true) || (filter.Adat == true && filter.Summary != true))
                curMasterOperator = "/";
            else
                curMasterOperator = "*";

            double curMasterCurrencyRate = await _repository.GetMCurRateAsync(mCurCode.ToString(), companyId);

            string strWhere = BuildAdatFilter(filter);
            strWhere += ") as Qry ";

            if (filter.PaidBrokerage == false)
                strWhere += "Where 1=1";
            else
                strWhere += "where AdatRemain >1";

            if (filter.PaidInvoice == true)
                strWhere += " and [MasterCurrencyInvRemainAmt] < 1 ";

            return await _repository.GetCommissionOutstandingDataAsync(
                strWhere,
                mCurCode,
                sCurCode,
                mCurCode,
                curMasterOperator,
                curMasterCurrencyRate,
                companyId);
        }

        private static string BuildAdatFilter(CommissionOutstandingReportFilterDto filter)
        {
            var sb = new StringBuilder();

            try
            {
                sb.Append("where tsp.TrnType in(''PurRtn'',''SJEW'',''PSaleRtn'',''CMemoIn'',''SALE'',''LG'' ,''PSALE'',''CMemoOut'',''PMemoIn'',''SaleRtn'',''PPUR'',''MJEW'',''MemoOut'',''PJEW'',''PUR'',''MemoIn'',''JMemoOut'',''CSALE'',''PartyHold'',''PMemoOut'',''PPurRtn'',''JMemoIn'',''LT'',''CSaleRtn'',''CPUR'',''CPurrtn'')");

                if (filter.FromDate != null && filter.ToDate != null)
                    sb.Append(" AND tsp.trnDate BETWEEN ''" + filter.FromDate + "'' AND ''" + filter.ToDate + "''");

                if (!string.IsNullOrEmpty(filter.Party))
                    sb.Append(" AND tparty.AccGrpName IN (''" + filter.Party + "'')");

                if (!string.IsNullOrEmpty(filter.AdatName))
                    sb.Append(" AND tadat.accgrpname IN (''" + filter.AdatName + "'')");

                if (!string.IsNullOrEmpty(filter.SalesPerson))
                    sb.Append(" AND tsp.SalesPerson IN (''" + filter.SalesPerson + "'')");

                sb.Append(" AND tsp.DeleteFlag<>1 ");
            }
            catch
            {
                return "false";
            }

            return sb.ToString();
        }
    }
}
