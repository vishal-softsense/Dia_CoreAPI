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
    public class ParcelPurchaseAnalysisRepository : IParcelPurchaseAnalysisRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelPurchaseAnalysisRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        private static bool HasText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private string BuildPurchaseWhereClause(ParcelPurchaseAnalysisRequestDto filter)
        {
            string strWhere = "";

            if (HasText(filter.FromDate) && HasText(filter.ToDate))
            {
                strWhere += " AND TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }

            if (HasText(filter.Currency))
            {
                strWhere += " AND tpt.CurCode IN (''" + filter.Currency + "'')";
            }

            if (HasText(filter.InvNo))
            {
                strWhere += " AND InvNo IN (''" + filter.InvNo + "'')";
            }

            if (HasText(filter.BillNo))
            {
                strWhere += " AND BillNo IN (''" + filter.BillNo + "'')";
            }

            if (HasText(filter.Type))
            {
                strWhere += " AND invType IN (''" + filter.Type + "'')";
            }

            if (HasText(filter.SubType))
            {
                strWhere += " AND SubType IN (''" + filter.SubType + "'')";
            }

            if (HasText(filter.Category))
            {
                strWhere += " AND Category IN (''" + filter.Category + "'')";
            }

            if (HasText(filter.Party))
            {
                strWhere += " AND tSupp.AccGrpName IN (''" + filter.Party + "'')";
            }

            if (HasText(filter.Broker))
            {
                strWhere += " AND tSupp.AccGrpName IN (''" + filter.Broker + "'')";
            }

            if (HasText(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo!
                    .Replace(" ", ",")
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace(",", "'',''");
                strWhere += " AND ViewItem.LotNo IN (''" + strTmpNewLine + "'')";
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

            if (filter.Boxnm != null && filter.Boxnm.Count > 0)
            {
                string boxValues = string.Join("'',''", filter.Boxnm.Where(b => HasText(b)));
                if (HasText(boxValues))
                {
                    strWhere += " And BoxName In (''" + boxValues + "'')";
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

            if (filter.PartyLocation != null && filter.PartyLocation.Count > 0)
            {
                string locValues = string.Join("'',''", filter.PartyLocation.Where(l => HasText(l)));
                if (HasText(locValues))
                {
                    strWhere += " And CountryName In (''" + locValues + "'')";
                }
            }

            if (filter.Shape != null && filter.Shape.Count > 0)
            {
                string shapeValues = string.Join("'',''", filter.Shape.Where(s => HasText(s)));
                if (HasText(shapeValues))
                {
                    strWhere += " And ViewItem.Shp In (''" + shapeValues + "'')";
                }
            }

            if (filter.Color != null && filter.Color.Count > 0)
            {
                string colorValues = string.Join("'',''", filter.Color.Where(c => HasText(c)));
                if (HasText(colorValues))
                {
                    strWhere += " And ViewItem.Col In (''" + colorValues + "'')";
                }
            }

            if (filter.Clarity != null && filter.Clarity.Count > 0)
            {
                string clarityValues = string.Join("'',''", filter.Clarity.Where(c => HasText(c)));
                if (HasText(clarityValues))
                {
                    strWhere += " And ViewItem.Cla In (''" + clarityValues + "'')";
                }
            }

            if (filter.Size != null && filter.Size.Count > 0)
            {
                string sizeValues = string.Join("'',''", filter.Size.Where(s => HasText(s)));
                if (HasText(sizeValues))
                {
                    strWhere += " And ViewItem.SizeId In (''" + sizeValues + "'')";
                }
            }

            if (filter.Sieve != null && filter.Sieve.Count > 0)
            {
                string sieveValues = string.Join("'',''", filter.Sieve.Where(s => HasText(s)));
                if (HasText(sieveValues))
                {
                    strWhere += " And ViewItem.Sieve In (''" + sieveValues + "'')";
                }
            }

            return strWhere;
        }

        public async Task<List<ParcelPurchaseAnalysisResultDto>?> GetParcelPurchaseAnalysisDataAsync(ParcelPurchaseAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string purAna = HasText(filter.PurAna) ? filter.PurAna! : "Shape";
            string? spType = purAna.ToLowerInvariant() switch
            {
                "shape" => "ParcelPurShapeAnalysis",
                "size" => "ParcelPurSizeAnalysis",
                "color" => "ParcelPurColorAnalysis",
                "clarity" => "ParcelPurClarityAnalysis",
                "sieve" => "ParcelPurSieveAnalysis",
                "location" => "ParcelPurLocationAnalysis",
                "partylocation" => "ParcelPurPartyLocationAnalysis",
                "stockid" => "ParcelPurStockIDAnalysis",
                "month" => "ParcelPurMonthAnalysis",
                _ => "ParcelPurShapeAnalysis"
            };

            string strWhere = BuildPurchaseWhereClause(filter);
            string sql = @" EXEC [GetParcelPurchaseAnalysisData] @Type='" + spType + "', @Where = '" + strWhere + "' ";

            var data = await _sqlDataAccess.SelectRawSql<ParcelPurchaseAnalysisResultDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<IEnumerable<BasicDataDto>> GetPurchaseSearchDropdownAsync(string companyId)
        {
            var list1 = await _sqlDataAccess.SelectRawSql<BasicDataDto>("EXEC AllMasterDataForPurchaseReport", companyId);
            var list2 = await _sqlDataAccess.SelectRawSql<BasicDataDto>("EXEC AllMasterDataForAdminStockSearch", companyId);
            

            return list1.Concat(list2);
        }
    }
}
