using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Repositories.Classes.ParcelDashboard
{
    public class ParcelSaleAnalysisRepository : IParcelSaleAnalysisRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelSaleAnalysisRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        private static bool HasText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private string BuildSaleWhereClause(ParcelSaleAnalysisRequestDto filter)
        {
            string strWhere = "";

            if (HasText(filter.FromDate) && HasText(filter.ToDate))
            {
                strWhere += " AND TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }

            if (HasText(filter.Currency))
            {
                strWhere += " AND tspt.CurCode IN (''" + filter.Currency + "'')";
            }

            if (HasText(filter.InvNo))
            {
                strWhere += " AND InvNo IN (''" + filter.InvNo + "'')";
            }

            if (HasText(filter.Party))
            {
                strWhere += " AND tCust.AccGrpName IN (''" + filter.Party + "'')";
            }

            if (HasText(filter.Broker))
            {
                strWhere += " AND tCust.AccGrpName IN (''" + filter.Broker + "'')";
            }

            if (HasText(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo!
                    .Replace(" ", ",")
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace(",", "'',''");
                strWhere += " AND Item IN (''" + strTmpNewLine + "'')";
            }

            if (filter.Shape != null && filter.Shape.Count > 0)
            {
                string shapeValues = string.Join("'',''", filter.Shape.Where(s => HasText(s)));
                if (HasText(shapeValues))
                {
                    strWhere += " And Shp In (''" + shapeValues + "'')";
                }
            }

            if (filter.Color != null && filter.Color.Count > 0)
            {
                string colorValues = string.Join("'',''", filter.Color.Where(c => HasText(c)));
                if (HasText(colorValues))
                {
                    strWhere += " And Col In (''" + colorValues + "'')";
                }
            }

            if (filter.Clarity != null && filter.Clarity.Count > 0)
            {
                string clarityValues = string.Join("'',''", filter.Clarity.Where(c => HasText(c)));
                if (HasText(clarityValues))
                {
                    strWhere += " And Cla In (''" + clarityValues + "'')";
                }
            }

            if (filter.Symmetry != null && filter.Symmetry.Count > 0)
            {
                string symValues = string.Join("'',''", filter.Symmetry.Where(s => HasText(s)));
                if (HasText(symValues))
                {
                    strWhere += " And Sym In (''" + symValues + "'')";
                }
            }

            if (filter.ShapeGroup != null && filter.ShapeGroup.Count > 0)
            {
                string sgValues = string.Join("'',''", filter.ShapeGroup.Where(s => HasText(s)));
                if (HasText(sgValues))
                {
                    strWhere += " And ShapeGroupId In (''" + sgValues + "'')";
                }
            }

            if (filter.ColorGroup != null && filter.ColorGroup.Count > 0)
            {
                string cgValues = string.Join("'',''", filter.ColorGroup.Where(c => HasText(c)));
                if (HasText(cgValues))
                {
                    strWhere += " And ColorGroupId In (''" + cgValues + "'')";
                }
            }

            if (filter.Size != null && filter.Size.Count > 0)
            {
                string sizeValues = string.Join("'',''", filter.Size.Where(s => HasText(s)));
                if (HasText(sizeValues))
                {
                    strWhere += " And Sieve In (''" + sizeValues + "'')";
                }
            }

            if (filter.Sieve != null && filter.Sieve.Count > 0)
            {
                string sieveValues = string.Join("'',''", filter.Sieve.Where(s => HasText(s)));
                if (HasText(sieveValues))
                {
                    strWhere += " And Sieve In (''" + sieveValues + "'')";
                }
            }

            if (filter.Boxnm != null && filter.Boxnm.Count > 0)
            {
                string boxValues = string.Join("'',''", filter.Boxnm.Where(b => HasText(b)));
                if (HasText(boxValues))
                {
                    strWhere += " And BoxName In (''" + boxValues + "'')";
                }
            }

            if (filter.PartyLocation != null && filter.PartyLocation.Count > 0)
            {
                string locValues = string.Join("'',''", filter.PartyLocation.Where(l => HasText(l)));
                if (HasText(locValues))
                {
                    strWhere += " And CountryName In (''" + locValues + "'')";
                }
            }

            if (filter.ItemGroup != null && filter.ItemGroup.Count > 0)
            {
                string igValues = string.Join("'',''", filter.ItemGroup.Where(i => HasText(i)));
                if (HasText(igValues))
                {
                    strWhere += " And ItemGroupId In (''" + igValues + "'')";
                }
            }

            return strWhere;
        }

        public async Task<List<ParcelSaleAnalysisResultDto>?> GetParcelSaleAnalysisDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string saleAna = HasText(filter.SaleAna) ? filter.SaleAna! : "Shape";
            string? spType = saleAna.ToLowerInvariant() switch
            {
                "shape" => "ParcelSaleShapeAnalysis",
                "size" => "ParcelSaleSizeAnalysis",
                "color" => "ParcelSaleColorAnalysis",
                "clarity" => "ParcelSaleClarityAnalysis",
                "sieve" => "ParcelSaleSieveAnalysis",
                "location" => "ParcelSaleLocationAnalysis",
                "partylocation" => "ParcelSalePartyLocationAnalysis",
                "stockid" => "ParcelSaleStockIDAnalysis",
                "month" => "ParcelSaleMonthAnalysis",
                _ => "ParcelSaleShapeAnalysis"
            };

            string strWhere = BuildSaleWhereClause(filter);
            string sql = @" EXEC [GetParcelSaleAnalysisData] @Type='" + spType + "', @Where = '" + strWhere + "' ";

            var data = await _sqlDataAccess.SelectRawSql<ParcelSaleAnalysisResultDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<List<ParcelSalePivotDto>> GetParcelSalePivotDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string strWhere = BuildSaleWhereClause(filter);
            string sql = @" EXEC [GetParcelSaleAnalysisData] @Type='ParcelSalePivotDatas', @Where = '" + strWhere + "' ";

            var data = await _sqlDataAccess.SelectRawSql<ParcelSalePivotDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<IEnumerable<BasicDataDto>> GetSaleSearchDropdownAsync(string companyId)
        {
            string sql = @"EXEC AllMasterDataForAdminStockSearch";
            var result = await _sqlDataAccess.SelectRawSql<BasicDataDto>(sql, companyId);
            return result;
        }
    }
}
