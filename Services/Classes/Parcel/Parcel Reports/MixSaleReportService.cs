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
    public class MixSaleReportService : IMixSaleReportService
    {
        private readonly IMixSaleReportRepository _repository;

        public MixSaleReportService(IMixSaleReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        

        public async Task<MixSaleReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetSaleReportDropDownAsync(companyId);

            var dataList = rawData.Select(x => new Dia_CoreAPI.DTOs.BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixSaleReportDropdownsDTO
            {
                Currency = dataList.Where(x => x.Type == "Currency").ToList(),
                Shape = dataList.Where(x => x.Type == "Shape").ToList(),
                Color = dataList.Where(x => x.Type == "Color").ToList(),
                Clarity = dataList.Where(x => x.Type == "Clarity").ToList(),
                Size = dataList.Where(x => x.Type == "Size").ToList(),
                Sieve = dataList.Where(x => x.Type == "Sieve").ToList()
            };
        }

        public async Task<IEnumerable<MixSaleReportResultDTO>> GetGeneralSaleParcelReportAsync(MixSaleReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetGeneralSaleParcelReportAsync(strWhere, companyId);
        }

        private static string BuildWhereClause(MixSaleReportFilterDTO filter)
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
                    sb.Append($" AND VER.CurCode IN (''{filter.Currency}'')");

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    sb.Append($" AND VER.InvNo IN (''{filter.InvNo}'')");

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    sb.Append($" AND VER.Party IN (''{filter.Party}'')");

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    sb.Append($" AND VER.Broker IN (''{filter.Broker}'')");

                if (!string.IsNullOrWhiteSpace(filter.SalesPerson))
                    sb.Append($" AND VER.SalesPerson IN (''{filter.SalesPerson}'')");

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Trim()
                        .Replace("\r\n", ",")
                        .Replace("\n", ",")
                        .Replace("\r", ",")
                        .Replace(",", "''''");

                    sb.Append($" AND ( ver.Item IN (''{strTmpNewLine}'') OR ver.Stock_ID IN (''{strTmpNewLine}'') ) ");
                }

                AppendInClause(sb, filter.Shape, "VER.ShapeId");
                AppendInClause(sb, filter.Color, "VER.ColorId");
                AppendInClause(sb, filter.Clarity, "VER.ClarityId");
                AppendInClause(sb, filter.Sieve, "VER.SieveId");
                AppendInClause(sb, filter.Size, "VER.SizeId");

                sb.Append(" and ver.trntype = ''CSALE''");
            }
            catch
            {
                // Dynamic execution safety fallback
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