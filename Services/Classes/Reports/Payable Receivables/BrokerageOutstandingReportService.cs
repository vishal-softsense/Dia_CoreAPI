using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables;
using Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.Services.Classes.Reports.Payable_Receivables
{
    public class BrokerageOutstandingReportService : IBrokerageOutstandingReportService
    {
        private readonly IBrokerageOutstandingReportRepository _repository;

        public BrokerageOutstandingReportService(IBrokerageOutstandingReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<BrokerageOutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId)
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

            return new BrokerageOutstandingReportDropdownsDto
            {
                Currency = currencies
            };
        }

        public async Task<List<BrokerageOutstandingReportResultDto>> GetBrokerageOutstandingReportAsync(
            BrokerageOutstandingReportFilterDto filter,
            string companyId)
        {
            long mCurCode = await _repository.GetCurCodeAsync("MasterCurrencyRate=1", companyId);
            long sCurCode = await _repository.GetCurCodeAsync("StockCurrencyRate=1", companyId);

            string curCode;
            if (filter.Currency == null)
                curCode = sCurCode.ToString();
            else
                curCode = filter.Currency;

            string curMasterOperator = await _repository.GetMCurOperatorAsync(curCode, companyId);

            if ((filter.Brokerage == true && !filter.Summary == true) || (filter.Adat == true && !filter.Summary == true))
                curMasterOperator = "/";
            else
                curMasterOperator = "*";

            double curMasterCurrencyRate = await _repository.GetMCurRateAsync(mCurCode.ToString(), companyId);

            string strWhere = BroAdatOutstandingReport(filter);

            strWhere += ") as Qry ";
            if (filter.PaidBrokerage == false)
            {
                strWhere += "Where 1=1";
            }
            else
            {
                strWhere += "where BrokerRemain >1";
            }

            if (filter.PaidInvoice == true)
            {
                strWhere += " and [MasterCurrencyInvRemainAmt] < 1 ";
            }

            return await _repository.GetBrokerageOutstandingDataAsync(
                strWhere,
                mCurCode,
                sCurCode,
                mCurCode,
                curMasterOperator,
                curMasterCurrencyRate,
                companyId);
        }

        public string BroAdatOutstandingReport(BrokerageOutstandingReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                strWhere += "where tsp.TrnType in(''PurRtn'',''SJEW'',''PSaleRtn'',''CMemoIn'',''SALE'',''LG'' ,''PSALE'',''CMemoOut'',''PMemoIn'',''SaleRtn'',''PPUR'',''MJEW'',''MemoOut'',''PJEW'',''PUR'', ''MemoIn'',''JMemoOut'',''CSALE'',''PartyHold'',''PMemoOut'',''PPurRtn'',''JMemoIn'',''LT'',''CSaleRtn'',''CPUR'',''CPurrtn'')";
                if (filter.FromDate != null && filter.ToDate != null)
                    strWhere += " AND tsp.trnDate BETWEEN ''" + filter.FromDate + "'' AND ''" + filter.ToDate + "''";

                if (!string.IsNullOrEmpty(filter.Party))
                    strWhere += " AND tparty.AccGrpName IN (''" + filter.Party + "'')";

                if (!string.IsNullOrEmpty(filter.Broker))
                    strWhere += " AND tbroker.AccGrpName IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrEmpty(filter.SalesPerson))
                    strWhere += " AND tsp.SalesPerson IN (''" + filter.SalesPerson + "'')";

                strWhere += " AND  tsp.deleteflag<>1";
            }
            catch (Exception)
            {
                return "false";
            }
            return strWhere;
        }
    }
}
