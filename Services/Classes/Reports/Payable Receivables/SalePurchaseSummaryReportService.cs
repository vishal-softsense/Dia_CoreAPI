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
    public class SalePurchaseSummaryReportService : ISalePurchaseSummaryReportService
    {
        private readonly ISalePurchaseSummaryReportRepository _repository;

        public SalePurchaseSummaryReportService(ISalePurchaseSummaryReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<SalePurchaseSummaryReportDropdownsDto> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetDropdownDataAsync(companyId);

            var list = rawData
                .Select(x => new BasicDataDto
                {
                    ID = x.ID,
                    Name = x.Name,
                    Type = x.Type,
                    Priority = x.Priority,
                    TextName = x.TextName
                })
                .ToList();

            static List<BasicDataDto> ByType(List<BasicDataDto> data, string type) =>
                data.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return new SalePurchaseSummaryReportDropdownsDto
            {
                Currency = ByType(list, "Currency"),
                Type = ByType(list, "Type"),
                SubType = ByType(list, "SubType"),
                Seller = ByType(list, "Seller"),
                Buyer = ByType(list, "Buyer")
            };
        }

        public async Task<List<SalePurchaseSummaryReportResultDto>> GetSalePurchaseSummaryReportAsync(
            SalePurchaseSummaryReportFilterDto filter,
            string companyId)
        {
            string strWhere = BuildSummaryReportFilter(filter);
            return await _repository.GetSalePurchaseSummaryDataAsync(strWhere, companyId);
        }

        private static string BuildSummaryReportFilter(SalePurchaseSummaryReportFilterDto filter)
        {
            var strWhere = string.Empty;

            try
            {
                if (filter.Sale)
                    strWhere += " AND TrnType = ''Sale''";
                else if (filter.PUR)
                    strWhere += " AND TrnType = ''PUR'' ";
                else if (filter.SaleRtn)
                    strWhere += " AND TrnType = ''SaleRtn'' ";
                else if (filter.PurRtn)
                    strWhere += " AND TrnType = ''PurRtn'' ";
                else if (filter.PSALE)
                    strWhere += " AND TrnType = ''PSALE'' ";
                else if (filter.PPUR)
                    strWhere += " AND TrnType = ''PPUR'' ";
                else if (filter.PSaleRtn)
                    strWhere += " AND TrnType = ''PSaleRtn'' ";
                else if (filter.MemoOut)
                    strWhere += " AND TrnType = ''MemoOut'' ";

                if (filter.FromDate != null && filter.ToDate != null)
                    strWhere += " AND Trndate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (filter.Currency != null)
                    strWhere += " AND CurCode IN (''" + filter.Currency + "'')";

                if (filter.InvTypeId != 0)
                    strWhere += " AND InvtypeId IN (''" + filter.InvTypeId + "'')";

                if (filter.SubTypeId != null)
                    strWhere += " AND Subtype IN (''" + filter.SubTypeId + "'')";

                if (filter.PartyCode != 0)
                    strWhere += " AND partycode IN (''" + filter.PartyCode + "'')";

                if (filter.BrokerCode != 0)
                    strWhere += " AND BrokerCode IN (''" + filter.BrokerCode + "'')";

                if (filter.ExpPartyCode != 0)
                    strWhere += " AND ExpPartyCode IN (''" + filter.ExpPartyCode + "'')";

                if (filter.SellerCode != 0)
                    strWhere += " AND SellerCode IN (''" + filter.SellerCode + "'')";

                if (filter.BuyerCode != 0)
                    strWhere += " AND SellerCode IN (''" + filter.BuyerCode + "'')";
            }
            catch
            {
                return string.Empty;
            }

            return strWhere;
        }
    }
}
