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
    public class OutstandingReportService : IOutstandingReportService
    {
        private readonly IOutstandingReportRepository _repository;

        public OutstandingReportService(IOutstandingReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<OutstandingReportDropdownsDto> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetDropdownDataAsync(companyId);

            var dataList = rawData
                .Where(x => !string.Equals(x.Type, "Account", StringComparison.OrdinalIgnoreCase))
                .Select(x => new BasicDataDto
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = x.Type,
                    Priority = x.Priority,
                    TextName = x.TextName
                }).ToList();

            var typeList = dataList
                .Where(x => string.Equals(x.Type, "Type", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var productTypeList = dataList
                .Where(x => string.Equals(x.Type, "ProductType", StringComparison.OrdinalIgnoreCase))
                .ToList();

            string[] requiredProductTypes = { "Parcel", "General", "Solitaries", "Jewellery" };
            var productNames = new HashSet<string>(
                productTypeList.Select(t => (t.Name ?? t.ID ?? "").Trim()).Where(n => n.Length > 0),
                StringComparer.OrdinalIgnoreCase);

            foreach (var req in requiredProductTypes)
            {
                if (productNames.Contains(req)) continue;
                productTypeList.Add(new BasicDataDto
                {
                    ID = req,
                    Name = req,
                    Type = "ProductType",
                    Priority = 0,
                    TextName = req
                });
                productNames.Add(req);
            }

            var orderedProductTypes = new List<BasicDataDto>();
            foreach (var req in requiredProductTypes)
            {
                var match = productTypeList.FirstOrDefault(t =>
                    string.Equals((t.Name ?? t.ID ?? "").Trim(), req, StringComparison.OrdinalIgnoreCase));
                if (match != null) orderedProductTypes.Add(match);
            }
            foreach (var extra in productTypeList)
            {
                if (requiredProductTypes.Any(r =>
                    string.Equals((extra.Name ?? extra.ID ?? "").Trim(), r, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                orderedProductTypes.Add(extra);
            }

            var salesPersons = await _repository.GetSalesPersonsAsync(companyId);

            return new OutstandingReportDropdownsDto
            {
                Currency = dataList.Where(x => string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase)).ToList(),
                Type = typeList,
                SubType = dataList.Where(x => string.Equals(x.Type, "SubType", StringComparison.OrdinalIgnoreCase)).ToList(),
                Category = dataList.Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase)).ToList(),
                ProductType = orderedProductTypes,
                Account = new List<BasicDataDto>(),
                SalesPersons = salesPersons
            };
        }

        public async Task<List<OutstandingReportResultDto>> GetOutstandingReportAsync(
            OutstandingReportFilterDto filter,
            string companyId)
        {
            long mCurCode = await _repository.GetCurCodeAsync("MasterCurrencyRate=1", companyId);
            long sCurCode = await _repository.GetCurCodeAsync("StockCurrencyRate=1", companyId);

            string curCode;
            if (string.IsNullOrEmpty(filter.Currency))
            {
                curCode = sCurCode.ToString();
            }
            else
            {
                curCode = filter.Currency;
            }

            string curMasterOperator = await _repository.GetMCurOperatorAsync(curCode, companyId);
            double curMasterCurrencyRate = await _repository.GetMCurRateAsync(curCode, companyId);

            string strWhere = BuildOutstandingFilter(filter);
            strWhere += ") as Qry where 1=1 ";

            if (filter.PaidInvoice != true)
            {
                strWhere += " and RemainAmount +ExDiff > 1  ";
            }

            string displayCur = "0";
            if (filter.CheckAllCur == true)
            {
                displayCur = string.IsNullOrEmpty(filter.DispCurrency) ? "0" : filter.DispCurrency;
            }

            if (filter.Summary == true)
            {
                strWhere += " Group By Party ";
                return await _repository.GetOutstandingSummDataAsync(
                    strWhere,
                    curCode,
                    sCurCode,
                    mCurCode,
                    curMasterOperator,
                    curMasterCurrencyRate,
                    companyId);
            }

            return await _repository.GetOutstandingDataAsync(
                strWhere,
                curCode,
                sCurCode,
                mCurCode,
                curMasterOperator,
                curMasterCurrencyRate,
                displayCur,
                companyId);
        }

        private static string BuildOutstandingFilter(OutstandingReportFilterDto filter)
        {
            var sb = new StringBuilder();

            try
            {
                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                    sb.Append(" AND tsp.trnDate BETWEEN ''" + filter.FromDate + "'' AND ''" + filter.ToDate + "''");

                if (filter.FromDueChkBox == true)
                {
                    sb.Append(" AND tsp.DueDate  Between ''" + filter.FromDateDue + "'' AND ''" + filter.ToDateDue + "''");
                }

                if (!string.IsNullOrEmpty(filter.Currency))
                    sb.Append(" AND tsp.CurCode IN (''" + filter.Currency + "'')");

                if (filter.Payable == true)
                    sb.Append(" AND tsp.TrnType IN (''Pur'', ''SaleRtn'', ''PPur'',''PJEW'',''CPur'')");
                else
                    sb.Append(" AND tsp.TrnType IN (''Sale'', ''PurRtn'', ''PSale'',''SJEW'',''CSale'')");

                if (!string.IsNullOrEmpty(filter.Party))
                    sb.Append(" AND tparty.AccGrpCode IN (''" + filter.Party + "'')");

                if (!string.IsNullOrEmpty(filter.Adat))
                    sb.Append(" AND tadat.AccGrpCode IN (''" + filter.Adat + "'')");

                if (!string.IsNullOrEmpty(filter.Broker))
                    sb.Append(" AND tbroker.AccGrpCode IN (''" + filter.Broker + "'')");

                if (!string.IsNullOrEmpty(filter.Seller))
                    sb.Append(" AND Selleruser.AccGrpCode IN (''" + filter.Seller + "'')");

                if (!string.IsNullOrEmpty(filter.SalesPerson))
                    sb.Append(" AND tsp1.SalesPerson IN (''" + filter.SalesPerson + "'')");

                if (!string.IsNullOrEmpty(filter.Type))
                    sb.Append(" AND invtype IN (''" + filter.Type + "'')");

                if (!string.IsNullOrEmpty(filter.TrnType))
                {
                    string formattedTrnType = filter.TrnType.Replace(",", "'',''");
                    sb.Append(" AND tsp.TrnType IN (''" + formattedTrnType + "'')");
                }
                else if (!string.IsNullOrEmpty(filter.ProductType))
                    sb.Append(" AND ProductType IN (''" + filter.ProductType + "'')");

                if (!string.IsNullOrEmpty(filter.SubType))
                    sb.Append(" AND SubType IN (''" + filter.SubType + "'')");

                if (!string.IsNullOrEmpty(filter.Category))
                    sb.Append(" AND Category IN (''" + filter.Category + "'')");

                if (!string.IsNullOrEmpty(filter.BillNo))
                    sb.Append(" AND BillNo IN (''" + filter.BillNo + "'')");

                if (!string.IsNullOrEmpty(filter.InvNo))
                    sb.Append(" AND InvNo IN (''" + filter.InvNo + "'')");
            }
            catch
            {
                return "false";
            }

            return sb.ToString();
        }
    }
}
