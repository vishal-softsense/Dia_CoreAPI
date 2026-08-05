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
    public class MixPurchaseReportService : IMixPurchaseReportService
    {
        private readonly IMixPurchaseReportRepository _repository;

        public MixPurchaseReportService(IMixPurchaseReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<MixPurchaseReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            // Project repository items to Dia_CoreAPI.DTOs.BasicDataDto to prevent type conversion errors
            var dataList = rawData.Select(x => new Dia_CoreAPI.DTOs.BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixPurchaseReportDropdownsDTO
            {
                Shape = dataList.Where(x => x.Type == "Shape").ToList(),
                Size = dataList.Where(x => x.Type == "Size").ToList(),
                Color = dataList.Where(x => x.Type == "Color").ToList(),
                Clarity = dataList.Where(x => x.Type == "Clarity").ToList(),
                Sieve = dataList.Where(x => x.Type == "Sieve").ToList(),
                Currency = dataList.Where(x => x.Type == "Currency").ToList()
            };
        }

        public async Task<IEnumerable<MixPurchaseReportResultDTO>> GetGeneralPurchaseParcelReportAsync(MixPurchaseReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetGeneralPurchaseParcelReportAsync(strWhere, companyId);
        }

        private static string BuildWhereClause(MixPurchaseReportFilterDTO filter)
        {
            var sb = new StringBuilder();

            try
            {
                if (filter == null) return string.Empty;

                if (filter.FromDate.HasValue && filter.ToDate.HasValue)
                {
                    sb.Append($" AND Date BETWEEN ''{filter.FromDate.Value:yyyy-MM-dd}'' And ''{filter.ToDate.Value:yyyy-MM-dd}''");
                }

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    sb.Append($" AND CurCode IN (''{filter.Currency}'')");

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    sb.Append($" AND InvNo IN (''{filter.InvNo}'')");

                if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "ALL")
                    sb.Append($" AND invType IN (''{filter.Type}'')");

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    sb.Append($" AND Party IN (''{filter.Party}'')");

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    sb.Append($" AND Broker IN (''{filter.Broker}'')");

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Trim()
                        .Replace("\r\n", ",")
                        .Replace("\n", ",")
                        .Replace("\r", ",")
                        .Replace(",", "'',''");

                    sb.Append($" AND ( Item IN (''{strTmpNewLine}'') OR Stock_ID IN (''{strTmpNewLine}'') ) ");
                }

                AppendInClause(sb, filter.Shape, "ShapeId");
                AppendInClause(sb, filter.Color, "ColorId");
                AppendInClause(sb, filter.Clarity, "ClarityId");
                AppendInClause(sb, filter.Symmetry, "SymId");
                AppendInClause(sb, filter.Sieve, "sieveid");

                sb.Append(" and trntype = ''CPUR''");
            }
            catch
            {
                // Return partial string in case of dynamic execution or format errors
            }

            return sb.ToString();
        }

        private static void AppendInClause(StringBuilder sb, List<string>? values, string column)
        {
            if (values == null || values.Count == 0) return;

            string joined = string.Join("'',''", values.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrEmpty(joined))
            {
                sb.Append($" AND {column} IN (''{joined}'')");
            }
        }
    }
}