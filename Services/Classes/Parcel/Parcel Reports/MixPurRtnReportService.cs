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
    public class MixPurRtnReportService : IMixPurRtnReportService
    {
        private readonly IMixPurRtnReportRepository _repository;

        public MixPurRtnReportService(IMixPurRtnReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        

        public async Task<MixPurRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            var dataList = rawData.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixPurRtnReportDropdownsDTO
            {
                Shape = dataList.Where(x => x.Type == "Shape").ToList(),
                Size = dataList.Where(x => x.Type == "Size").ToList(),
                Color = dataList.Where(x => x.Type == "Color").ToList(),
                Clarity = dataList.Where(x => x.Type == "Clarity").ToList(),
                Sieve = dataList.Where(x => x.Type == "Sieve").ToList(),
                Currency = dataList.Where(x => x.Type == "Currency").ToList()
            };
        }

        public async Task<IEnumerable<MixPurRtnReportResultDTO>> GetMixPurchaseReturnReportAsync(MixPurRtnReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetMixPurchaseReturnReportAsync(strWhere, companyId);
        }

        private static string BuildWhereClause(MixPurRtnReportFilterDTO filter)
        {
            if (filter == null)
                return string.Empty;

            var sb = new StringBuilder();

            // Date
            if (!string.IsNullOrWhiteSpace(filter.FromDate) &&
                !string.IsNullOrWhiteSpace(filter.ToDate))
            {
                sb.Append($" AND Date BETWEEN ''{filter.FromDate}'' AND ''{filter.ToDate}''");
            }

            // Single Value Filters
            if (!string.IsNullOrWhiteSpace(filter.Currency))
                sb.Append($" AND CurCode IN (''{filter.Currency.Trim()}'')");

            if (!string.IsNullOrWhiteSpace(filter.InvNo))
                sb.Append($" AND InvNo IN (''{filter.InvNo.Trim()}'')");

            if (!string.IsNullOrWhiteSpace(filter.Type))
                sb.Append($" AND InvType IN (''{filter.Type.Trim()}'')");

            if (!string.IsNullOrWhiteSpace(filter.Party))
                sb.Append($" AND Party IN (''{filter.Party.Trim()}'')");

            if (!string.IsNullOrWhiteSpace(filter.Broker))
                sb.Append($" AND Broker IN (''{filter.Broker.Trim()}'')");

            // Lot No (supports comma/new line separated values)
            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                var lots = filter.LotNo
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"''{x.Trim()}''");

                sb.Append($" AND Item IN ({string.Join(",", lots)})");
            }

            // Multi Select Filters
            AppendInClause(sb, filter.Shape, "ShapeId");
            AppendInClause(sb, filter.Color, "ColorId");
            AppendInClause(sb, filter.Clarity, "ClarityId");
            AppendInClause(sb, filter.Symmetry, "SymId");
            AppendInClause(sb, filter.Sieve, "SieveId");

            // Fixed Condition
            sb.Append(" AND TrnType = ''CPurrtn''");

            return sb.ToString();
        }

        private static void AppendInClause(StringBuilder sb, IEnumerable<string>? values, string columnName)
        {
            if (values == null)
                return;

            var list = values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"''{x.Trim()}''")
                .ToList();

            if (list.Count > 0)
            {
                sb.Append($" AND {columnName} IN ({string.Join(",", list)})");
            }
        }
    }
}