using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports
{
    public class MixConsignmentInRtnReportService : IMixConsignmentInRtnReportService
    {
        private readonly IMixConsignmentInRtnReportRepository _repository;

        public MixConsignmentInRtnReportService(IMixConsignmentInRtnReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        

        public async Task<MixConsignmentInRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            var dataList = rawData.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixConsignmentInRtnReportDropdownsDTO
            {
                Currency = dataList.Where(x => x.Type == "Currency").ToList(),
                Type = dataList.Where(x => x.Type == "Type").ToList(),
                SubType = dataList.Where(x => x.Type == "SubType").ToList(),
                Category = dataList.Where(x => x.Type == "Category").ToList(),
                Shape = dataList.Where(x => x.Type == "Shape").ToList(),
                Color = dataList.Where(x => x.Type == "Color").ToList(),
                Size = dataList.Where(x => x.Type == "Size").ToList(),
                Clarity = dataList.Where(x => x.Type == "Clarity").ToList(),
                LAB = dataList.Where(x => x.Type == "LAB").ToList(),
                CPS = dataList.Where(x => x.Type == "CPS").ToList(),
                HA = dataList.Where(x => x.Type == "HA").ToList(),
                Proportion = dataList.Where(x => x.Type == "Proportion").ToList(),
                Polish = dataList.Where(x => x.Type == "Polish").ToList(),
                Symmetry = dataList.Where(x => x.Type == "Symmetry").ToList(),
                Fluorescence = dataList.Where(x => x.Type == "Fluorescence").ToList(),
                FlColor = dataList.Where(x => x.Type == "FlColor").ToList(),
                Brown = dataList.Where(x => x.Type == "Brown").ToList(),
                Green = dataList.Where(x => x.Type == "Green").ToList(),
                Milky = dataList.Where(x => x.Type == "Milky").ToList(),
                Luster = dataList.Where(x => x.Type == "Luster").ToList(),
                Natts = dataList.Where(x => x.Type == "Natts").ToList(),
                Shade = dataList.Where(x => x.Type == "Shade").ToList(),
                Rough = dataList.Where(x => x.Type == "Rough").ToList(),
                Mines = dataList.Where(x => x.Type == "Mines").ToList(),
                ProductType = dataList.Where(x => x.Type == "ProductType").ToList(),
                Sieve = dataList.Where(x => x.Type == "Sieve").ToList()
            };
        }

        public async Task<IEnumerable<MixConsignmentInRtnReportResultDTO>> GetMixMemoInRtnReportAsync(MixConsignmentInRtnReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetMixMemoInRtnReportAsync(strWhere, companyId);
        }

        private static string BuildWhereClause(MixConsignmentInRtnReportFilterDTO filter)
        {
            if (filter == null)
                return string.Empty;

            var sb = new StringBuilder();

            // Date Filter
            if (!string.IsNullOrWhiteSpace(filter.FromDate) &&
                !string.IsNullOrWhiteSpace(filter.ToDate))
            {
                sb.Append($" AND A.SPDate BETWEEN ''{filter.FromDate}'' AND ''{filter.ToDate}''");
            }

            // Currency
            if (!string.IsNullOrWhiteSpace(filter.Currency))
                sb.Append($" AND B.CurCode IN (''{filter.Currency.Trim()}'')");

            // Invoice No
            if (!string.IsNullOrWhiteSpace(filter.InvNo))
                sb.Append($" AND CM.InvNo IN (''{filter.InvNo.Trim()}'')");

            // Type
            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                !filter.Type.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" AND A.InvType IN (''{filter.Type.Trim()}'')");
            }

            // Party
            if (!string.IsNullOrWhiteSpace(filter.Party))
                sb.Append($" AND CMParty.AccGrpName IN (''{filter.Party.Trim()}'')");

            // Broker
            if (!string.IsNullOrWhiteSpace(filter.Broker))
                sb.Append($" AND tbroker.AccGrpName IN (''{filter.Broker.Trim()}'')");

            // Lot No (supports comma/new line separated values)
            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                var lots = filter.LotNo
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace("\r", ",")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"''{x.Trim()}''");

                string lotList = string.Join(",", lots);

                sb.Append($" AND (D.LotNo IN ({lotList}) OR TJ.Stock_ID IN ({lotList}))");
            }

            // Fixed Condition
            sb.Append(" AND A.TrnType = ''MixMemoInRtn''");

            return sb.ToString();
        }
    }
}