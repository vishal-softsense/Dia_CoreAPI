using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;

namespace Dia_CoreAPI.Repositories.Classes.Dashboard
{
    /// <summary>
    /// Exact port of MVC ChartsReportController.SaleAnaData + ChartsReportController.GetPivotData
    /// (sale-data and sale-pivotdata endpoints).
    /// SP used: GetSaleAnalysisData — no new SPs needed.
    /// </summary>
    public class SaleAnalysisReportRepository : ISaleAnalysisReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SaleAnalysisReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        // Matches MVC ReportProcessor.PurchaseReportDropDown() — same SP as Purchase/Parcel reports
        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "AllMasterDataForPurchaseReport",
                null,
                companyId);
            return result.ToList();
        }

        // Matches MVC ChartsReportController.SaleAnaData (Group-by/sale-data)
        public async Task<List<SaleAnalysisReportDto>> GetSaleAnalysisReportAsync(SaleAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string strWhere = BuildWhereClause(filter, forPivot: false);

            // Add GROUP BY + ORDER BY exactly as MVC does
            AppendGroupAndOrder(ref strWhere, filter.SaleAna);

            string typeName = GetTypeName(filter.SaleAna);
            string sql = $" EXEC [GetSaleAnalysisData] @Type='{typeName}', @Where = '{strWhere}' ";

            var data = await _sqlDataAccess.SelectRawSql<SaleAnalysisReportDto>(sql, companyId);
            return data.ToList();
        }

        // Matches MVC ChartsReportController.GetPivotData (Group-by/sale-pivotdata)
        public async Task<List<SaleAnalysisReportDto>> GetSaleAnalysisPivotDataAsync(SaleAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string strWhere = BuildWhereClause(filter, forPivot: true);

            string sql = $" EXEC [GetSaleAnalysisData] @Type='SalePivotDatas', @Where = '{strWhere}' ";

            var data = await _sqlDataAccess.SelectRawSql<SaleAnalysisReportDto>(sql, companyId);
            return data.ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // WHERE clause builder — mirrors MVC SaleAnaData line-for-line
        // ─────────────────────────────────────────────────────────────────────────
        private static string BuildWhereClause(SaleAnalysisRequestDto filter, bool forPivot)
        {
            string strWhere = "";

            if (HasText(filter.FromDate) && HasText(filter.ToDate))
                strWhere += " AND tsp.TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

            if (HasText(filter.Currency))
                strWhere += " AND tsp.CurCode IN (''" + filter.Currency + "'')";

            if (HasText(filter.InvNo))
                strWhere += " AND tsp.InvNo IN (''" + filter.InvNo + "'')";

            if (HasText(filter.BillNo))
                strWhere += " AND tsp.BillNo IN (''" + filter.BillNo + "'')";

            if (HasText(filter.Type))
                strWhere += " AND tsp.invType IN (''" + filter.Type + "'')";

            if (HasText(filter.SubType))
                strWhere += " AND tsp.SubType IN (''" + filter.SubType + "'')";

            if (HasText(filter.Category))
                strWhere += " AND tsp.Category IN (''" + filter.Category + "'')";

            if (filter.PurRtn)
                strWhere += " and tsp.TrnType = ''PurRtn''";

            if (HasText(filter.Party))
                strWhere += " AND tcust.AccGrpName IN (''" + filter.Party + "'')";

            if (HasText(filter.Broker))
            {
                if (forPivot)
                    strWhere += " AND tbroker.AccGrpName IN (''" + filter.Broker + "'')";
                else
                    strWhere += " AND tcust.AccGrpName IN (''" + filter.Broker + "'')";
            }

            if (HasText(filter.ExpParty))
                strWhere += forPivot
                    ? " AND ExportPartyName IN (''" + filter.ExpParty + "'')"
                    : " AND tsp.ExportPartyName IN (''" + filter.ExpParty + "'')";

            if (HasText(filter.LotNo))
            {
                string tmp = filter.LotNo.Replace(" ", "");
                tmp = tmp.Replace("\r\n", ",");
                tmp = tmp.Replace("\n", ",");
                tmp = tmp.Replace(",", "'',''");
                strWhere += forPivot
                    ? " AND ViewItem.LotNo IN (''" + tmp + "'')"
                    : " AND LotNo IN (''" + tmp + "'')";
            }

            // MVC pivot uses ViewItem.* prefixed columns; report uses bare column names
            string pfx = forPivot ? "ViewItem." : "";

            AppendListFilter(ref strWhere, filter.Shape, pfx + "Shp");
            AppendListFilter(ref strWhere, filter.Color, pfx + "Col");
            AppendListFilter(ref strWhere, filter.Clarity, pfx + "Cla");
            AppendListFilter(ref strWhere, filter.Size, forPivot ? "ViewItem.Size" : "Size");
            AppendListFilter(ref strWhere, filter.Proportion, pfx + "Prop");
            AppendListFilter(ref strWhere, filter.Polish, pfx + "Pol");
            AppendListFilter(ref strWhere, filter.Symmetry, pfx + "Sym");
            AppendListFilter(ref strWhere, filter.LAB, pfx + "Cert");
            AppendListFilter(ref strWhere, filter.Fluorescence, pfx + "FL");
            AppendListFilter(ref strWhere, filter.Milky, forPivot ? "ViewItem.Milky" : "Milky");
            AppendListFilter(ref strWhere, filter.Luster, forPivot ? "ViewItem.Luster" : "Luster", spaceAfterParen: true);
            AppendListFilter(ref strWhere, filter.Natts, forPivot ? "ViewItem.Natts" : "Natts");
            AppendListFilter(ref strWhere, filter.Mines, forPivot ? "ViewItem.Mines" : "Mines");
            AppendListFilter(ref strWhere, filter.ProductType, "ProductType");
            AppendListFilter(ref strWhere, filter.Sieve, forPivot ? "ViewItem.Sieve" : "Sieve");

            return strWhere;
        }

        // GROUP BY + ORDER BY — exact MVC ChartsReportController.SaleAnaData logic
        private static void AppendGroupAndOrder(ref string strWhere, string saleAna)
        {
            switch (saleAna)
            {
                case "Lab":
                    strWhere += "GROUP BY ViewItem.LAB,ViewItem.CertPriority ";
                    strWhere += "  Order By ViewItem.CertPriority";
                    break;
                case "Shape":
                    strWhere += "GROUP BY ViewItem.Shp,ViewItem.ShapePriority ";
                    strWhere += "  Order By ViewItem.ShapePriority";
                    break;
                case "Size":
                    strWhere += "GROUP BY ViewItem.SizeID,ViewItem.SizePriority ";
                    strWhere += "  Order By ViewItem.SizePriority";
                    break;
                case "Color":
                    strWhere += "GROUP BY ViewITem.Col,ViewItem.ColorPriority ";
                    strWhere += "  Order By ViewItem.ColorPriority";
                    break;
                case "Clarity":
                    strWhere += "GROUP BY ViewITem.Cla,ViewItem.ClarityPriority ";
                    strWhere += "  Order By ViewItem.ClarityPriority";
                    break;
                case "Prop":
                    strWhere += "GROUP BY ViewITem.Prop,ViewItem.PropPriority ";
                    strWhere += "  Order By ViewItem.PropPriority";
                    break;
                case "Polish":
                    strWhere += "GROUP BY ViewITem.Pol,ViewItem.PolishPriority ";
                    strWhere += "  Order By ViewItem.PolishPriority";
                    break;
                case "Symmertry":
                    strWhere += "GROUP BY ViewITem.Sym,ViewItem.SymPriority ";
                    strWhere += "  Order By ViewItem.SymPriority";
                    break;
                case "CPS":
                    strWhere += "GROUP BY ViewITem.CPSID ";
                    strWhere += "  Order By ViewItem.CPSID";
                    break;
                case "FL":
                    strWhere += "GROUP BY ViewITem.FL,ViewItem.FLPriority ";
                    strWhere += "  Order By ViewItem.FLPriority";
                    break;
                case "Series":
                    strWhere += "GROUP BY ViewItem.Kapan ";
                    strWhere += "  Order By ViewItem.Kapan";
                    break;
                case "Location":
                    strWhere += "GROUP BY ViewItem.Country ";
                    strWhere += "  Order By ViewItem.Country";
                    break;
                case "YearMonth":
                    strWhere += "GROUP BY Year(tspt.SPDate), Month(tspt.SPDate), DATENAME(month, tspt.SPDate) ";
                    strWhere += "  Order By Year(tspt.SPDate), Month(tspt.SPDate)";
                    break;
                case "Aging":
                    strWhere += "GROUP BY StoneAge ";
                    strWhere += "  Order By StoneAge";
                    break;
                case "PriceRevision":
                    strWhere += "GROUP BY PriceRevision ";
                    strWhere += "  Order By PriceRevision";
                    break;
                case "PartyLocation":
                    strWhere += "GROUP BY tCountry.CountryName ";
                    strWhere += "  Order By tCountry.CountryName ";
                    break;
                case "StockID":
                    strWhere += "GROUP BY ViewItem.LotNo ";
                    strWhere += "  Order By ViewItem.LotNo ";
                    break;
                default:
                    throw new ArgumentException("Invalid SaleAna grouping: " + saleAna);
            }
        }

        private static string GetTypeName(string saleAna)
        {
            return saleAna switch
            {
                "Lab"          => "SaleLabAnalysis",
                "Shape"        => "SaleShapeAnalysis",
                "Size"         => "SaleSizeAnalysis",
                "Color"        => "SaleColorAnalysis",
                "Clarity"      => "SaleClarityAnalysis",
                "Prop"         => "SalePropAnalysis",
                "Polish"       => "SalePolishAnalysis",
                "Symmertry"    => "SaleSymAnalysis",
                "CPS"          => "SaleCPSAnalysis",
                "FL"           => "SaleFLAnalysis",
                "Series"       => "SaleSeriesAnalysis",
                "Location"     => "SaleLocationAnalysis",
                "YearMonth"    => "SalePurYearMonthAnalysis",
                "Aging"        => "SaleAgingAnalysis",
                "PriceRevision"=> "SalePriceRevisionAnalysis",
                "PartyLocation"=> "SalePartyLocationAnalysis",
                "StockID"      => "SaleStockIDAnalysis",
                _ => throw new ArgumentException("Invalid SaleAna grouping: " + saleAna)
            };
        }

        private static bool HasText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static void AppendListFilter(ref string strWhere, List<string> list, string columnName, bool spaceAfterParen = false)
        {
            if (list == null || list.Count == 0) return;

            string joined = "";
            foreach (var el in list)
            {
                if (!HasText(el)) continue;
                joined += "''" + el + "'',";
            }
            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);
            if (joined.Length == 0) return;

            string open = spaceAfterParen ? " ( " : " (";
            strWhere += " And " + columnName + " In" + open + joined + ")";
        }
    }
}
