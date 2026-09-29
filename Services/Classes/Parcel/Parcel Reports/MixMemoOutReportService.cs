using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.Repositories;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports
{
    public class MixMemoOutReportService : IMixMemoOutReportService
    {
        private readonly IMixMemoOutReportRepository _repository;

        public MixMemoOutReportService(IMixMemoOutReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        

        public async Task<MixMemoOutReportDropdownsDTO> GetDropdownDataAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            var dataList = rawData.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Name = x.Name,
                Type = x.Type
            }).ToList();

            return new MixMemoOutReportDropdownsDTO
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

        public async Task<IEnumerable<MixMemoOutReportResultDTO>> GetMixMemoReportAsync(MixMemoOutReportFilterDTO filter, string companyId)
        {
            string strWhere = BuildWhereClause(filter);
            return await _repository.GetMixMemoReportAsync(strWhere, "", companyId);
        }
        private static string BuildWhereClause(MixMemoOutReportFilterDTO filter)
        {
            var sb = new StringBuilder();

            try
            {
                if (filter == null) return string.Empty;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                {
                    sb.Append($" AND tm.trndate BETWEEN ''{filter.FromDate}'' And ''{filter.ToDate}''");
                }

                if (filter.ChkRtnDate)
                {
                    if (!string.IsNullOrWhiteSpace(filter.FromRtnDate) && !string.IsNullOrWhiteSpace(filter.ToRtnDate))
                    {
                        sb.Append($" AND convert(date,tmtran.MemoReturnDate) BETWEEN ''{filter.FromRtnDate}'' And ''{filter.ToRtnDate}''");
                    }
                }

                // MVC MixMemoReportFilter: Print Date checkbox → filter RoundDate
                if (filter.FromRChkBox)
                {
                    sb.Append($" AND tm.RoundDate BETWEEN ''{filter.RDate}'' And ''{filter.RToDate}''");
                }

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    sb.Append($" AND tm.CurCode IN (''{filter.Currency}'')");

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    sb.Append($" AND tm.InvNo IN (''{filter.InvNo}'')");

                if (!string.IsNullOrWhiteSpace(filter.Type) && !filter.Type.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    sb.Append($" AND tm.invtype IN (''{filter.Type}'')");

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    sb.Append($" AND tparty.AccGrpname IN (''{filter.Party}'')");

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    sb.Append($" AND tbroker.AccGrpname IN (''{filter.Broker}'')");

                if (!string.IsNullOrWhiteSpace(filter.Tracking))
                    sb.Append($" AND tm.Tracking IN (''{filter.Tracking}'')");

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Trim()
                        .Replace("\r\n", ",")
                        .Replace("\n", ",")
                        .Replace("\r", ",")
                        .Replace(",", "'',''");

                    sb.Append($" AND ( titem.LOTNO IN (''{strTmpNewLine}'') OR TJ.Stock_ID IN (''{strTmpNewLine}'') ) ");
                }

                AppendInClause(sb, filter.Shape, "tshp.ShapeId");
                AppendInClause(sb, filter.Color, "tcol.ColorId");
                AppendInClause(sb, filter.Clarity, "tcla.ClarityId");
                AppendInClause(sb, filter.Size, "tsize.SizeId");
                AppendInClause(sb, filter.Proportion, "titem.PropId");
                AppendInClause(sb, filter.Polish, "titem.PolishId");
                AppendInClause(sb, filter.Symmetry, "titem.SymId");
                AppendInClause(sb, filter.LAB, "titem.CertId");
                AppendInClause(sb, filter.Fluorescence, "titem.FLId");
                AppendInClause(sb, filter.Milky, "titem.MilkyId");
                AppendInClause(sb, filter.Luster, "titem.LusterId");
                AppendInClause(sb, filter.Natts, "titem.NattsId");
                AppendInClause(sb, filter.Mines, "Minesid");
                AppendInClause(sb, filter.ProductType, "ProductType");
                AppendInClause(sb, filter.Sieve, "titem.SieveId");

                if (!filter.Clear)
                {
                    sb.Append(" AND(tmtran.carats - ISNULL(MemoToSale.LiveSPcarats, 0.000) - ISNULL(tmtran.ReturnCarats, 0.000)) > 0");
                }

                sb.Append(" and tm.trntype = ''CMemoOut''");
            }
            catch
            {
                // Catch potential runtime formatting issues
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