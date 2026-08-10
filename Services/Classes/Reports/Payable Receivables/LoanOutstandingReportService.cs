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
    public class LoanOutstandingReportService : ILoanOutstandingReportService
    {
        private readonly ILoanOutstandingReportRepository _repository;

        public LoanOutstandingReportService(ILoanOutstandingReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<LoanOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId)
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

            return new LoanOutstandingReportDropdownsDto
            {
                Currency = currencies
            };
        }

        public async Task<List<LoanOutstandingReportResultDto>> GetLoanOutstandingReportAsync(
            LoanOutstandingReportFilterDto filter,
            string companyId)
        {
            long mCurCode = await _repository.GetCurCodeAsync("MasterCurrencyRate=1", companyId);
            long sCurCode = await _repository.GetCurCodeAsync("StockCurrencyRate=1", companyId);

            string curCode = string.IsNullOrEmpty(filter.Currency) ? sCurCode.ToString() : filter.Currency;

            string curMasterOperator = await _repository.GetMCurOperatorAsync(curCode, companyId);

            if (curMasterOperator == "/")
                curMasterOperator = "*";
            else
                curMasterOperator = "/";

            double curMasterCurrencyRate = await _repository.GetMCurRateAsync(curCode, companyId);

            string strWhere = BuildLoanOutstandingFilter(filter);
            strWhere += ") as Qry ";

            if (filter.PaidInvoice == false)
            {
                if (filter.LoanGiven == true)
                    strWhere += "  WHERE Remainamount +ExDiff > 1 ";
                else
                    strWhere += "  WHERE Remainamount -ExDiff > 1 ";
            }

            string operatorForQuery = curMasterOperator;
            if (filter.Summary == true)
            {
                operatorForQuery = await _repository.GetMCurOperatorAsync(curCode, companyId);
            }

            return await _repository.GetLoanOutstandingDataAsync(
                strWhere,
                curCode,
                sCurCode,
                mCurCode,
                operatorForQuery,
                curMasterCurrencyRate,
                companyId);
        }

        private static string BuildLoanOutstandingFilter(LoanOutstandingReportFilterDto filter)
        {
            var sb = new StringBuilder();

            try
            {
                if (filter.LoanTaken == true)
                    sb.Append(" where 1=1 and  tsp.TrnType IN (''LT'') ");
                else
                    sb.Append(" where 1=1 and tsp.TrnType IN (''LG'') ");

                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                    sb.Append(" AND tsp.trnDate BETWEEN ''" + filter.FromDate + "'' AND ''" + filter.ToDate + "''");

                if (!string.IsNullOrEmpty(filter.Party))
                    sb.Append(" AND tparty.AccGrpName IN (''" + filter.Party + "'')");

                if (!string.IsNullOrEmpty(filter.Broker))
                    sb.Append(" AND tbroker.AccGrpCode IN (''" + filter.Broker + "'')");
            }
            catch
            {
                return "false";
            }

            return sb.ToString();
        }
    }
}
