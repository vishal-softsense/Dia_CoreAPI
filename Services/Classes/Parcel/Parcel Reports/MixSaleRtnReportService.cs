using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports
{
    public class MixSaleRtnReportService : IMixSaleRtnReportService
    {
        private readonly IMixSaleRtnReportRepository _repository;

        public MixSaleRtnReportService(IMixSaleRtnReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        

        public async Task<MixSaleRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            var dataList = rawData.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixSaleRtnReportDropdownsDTO
            {
                Currency = dataList.Where(x => x.Type == "Currency").ToList(),
                Shape = dataList.Where(x => x.Type == "Shape").ToList(),
                Color = dataList.Where(x => x.Type == "Color").ToList(),
                Clarity = dataList.Where(x => x.Type == "Clarity").ToList(),
                Size = dataList.Where(x => x.Type == "Size").ToList(),
                Sieve = dataList.Where(x => x.Type == "Sieve").ToList()
            };
        }

        public async Task<IEnumerable<MixSaleRtnReportResultDTO>> GetGeneralSaleRtnRptAsync(MixSaleRtnReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetGeneralSaleRtnRptAsync(strWhere, companyId);
        }

        private static string BuildWhereClause(MixSaleRtnReportFilterDTO filter)
        {
            if (filter == null)
                return string.Empty;

            var sb = new StringBuilder();

            // Date Filter
            if (!string.IsNullOrWhiteSpace(filter.FromDate) &&
                !string.IsNullOrWhiteSpace(filter.ToDate))
            {
                sb.Append($" AND Date BETWEEN ''{filter.FromDate}'' AND ''{filter.ToDate}''");
            }

            // Currency
            if (!string.IsNullOrWhiteSpace(filter.Currency))
                sb.Append($" AND VER.CurCode IN (''{filter.Currency.Trim()}'')");

            // Invoice No
            if (!string.IsNullOrWhiteSpace(filter.InvNo))
                sb.Append($" AND VER.InvNo IN (''{filter.InvNo.Trim()}'')");

            // Party
            if (!string.IsNullOrWhiteSpace(filter.Party))
                sb.Append($" AND VER.Party IN (''{filter.Party.Trim()}'')");

            // Broker
            if (!string.IsNullOrWhiteSpace(filter.Broker))
                sb.Append($" AND VER.Broker IN (''{filter.Broker.Trim()}'')");

            // Lot No (supports comma/newline separated values)
            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                var lots = filter.LotNo
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace("\r", ",")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"''{x.Trim()}''");

                string lotList = string.Join(",", lots);

                sb.Append($" AND (VER.Item IN ({lotList}) OR VER.Stock_ID IN ({lotList}))");
            }

            // Multi Select Filters
            AppendInClause(sb, filter.Shape, "VER.ShapeId");
            AppendInClause(sb, filter.Color, "VER.ColorId");
            AppendInClause(sb, filter.Clarity, "VER.ClarityId");
            AppendInClause(sb, filter.Size, "VER.SizeId");
            AppendInClause(sb, filter.Sieve, "VER.SieveId");

            // Fixed Condition
            sb.Append(" AND VER.TrnType = ''CSaleRtn''");

            return sb.ToString();
        }

        private static void AppendInClause(StringBuilder sb, List<string>? values, string column)
        {
            if (values == null)
                return;

            var list = values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"''{x.Trim()}''")
                .ToList();

            if (list.Count > 0)
            {
                sb.Append($" AND {column} IN ({string.Join(",", list)})");
            }
        }
    }
}