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
    /// Exact port of MVC ChartsReportController.PurAnaData + ReportProcessor Pur*AnaReport.
    /// SPs: AllMasterDataForPurchaseReport, GetPurchaseAnalysisData (no new SPs).
    /// </summary>
    public class PurchaseAnalysisReportRepository : IPurchaseAnalysisReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public PurchaseAnalysisReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        // Matches MVC ReportProcessor.PurchaseReportDropDown()
        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "AllMasterDataForPurchaseReport",
                null,
                companyId);
            return result.ToList();
        }

        // Matches MVC ChartsReportController.PurAnaData(PurchaseRptFilter filter) line-for-line
        public async Task<List<PurchaseAnalysisReportDto>> GetPurchaseAnalysisReportAsync(PurchaseAnalysisRequestDto filter, string companyId)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            string strWhere = "";

            // MVC: if (filter.FromDate != null && filter.ToDate != null)
            if (HasText(filter.FromDate) && HasText(filter.ToDate))
                strWhere += " AND TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

            // MVC: if (filter.Currency != null)
            if (HasText(filter.Currency))
                strWhere += " AND CurCode IN (''" + filter.Currency + "'')";

            if (HasText(filter.InvNo))
                strWhere += " AND InvNo IN (''" + filter.InvNo + "'')";

            if (HasText(filter.BillNo))
                strWhere += " AND BillNo IN (''" + filter.BillNo + "'')";

            if (HasText(filter.Type))
                strWhere += " AND invType IN (''" + filter.Type + "'')";

            if (HasText(filter.SubType))
                strWhere += " AND SubType IN (''" + filter.SubType + "'')";

            if (HasText(filter.Category))
                strWhere += " AND Category IN (''" + filter.Category + "'')";

            if (HasText(filter.Party))
                strWhere += " AND Party IN (''" + filter.Party + "'')";

            if (HasText(filter.Broker))
                strWhere += " AND Broker IN (''" + filter.Broker + "'')";

            if (HasText(filter.ExpParty))
                strWhere += " AND ExpParty IN (''" + filter.ExpParty + "'')";

            if (HasText(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo.Replace(" ", "");
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                strWhere += " AND ViewItem.LotNo IN (''" + strTmpNewLine + "'')";
            }

            // Multi-selects — exact MVC column names from PurAnaData
            AppendListFilter(ref strWhere, filter.Shape, "ViewItem.Shp");
            AppendListFilter(ref strWhere, filter.Color, "ViewItem.Col");
            AppendListFilter(ref strWhere, filter.Clarity, "ViewItem.Cla");
            AppendListFilter(ref strWhere, filter.Size, "ViewItem.SizeId");
            AppendListFilter(ref strWhere, filter.Proportion, "ViewItem.Prop");
            AppendListFilter(ref strWhere, filter.Polish, "ViewItem.Pol");
            AppendListFilter(ref strWhere, filter.Symmetry, "ViewItem.Sym");
            AppendListFilter(ref strWhere, filter.LAB, "ViewItem.Cert");
            AppendListFilter(ref strWhere, filter.Fluorescence, "ViewItem.FL");
            AppendListFilter(ref strWhere, filter.Milky, "MilkyId");
            AppendListFilter(ref strWhere, filter.Luster, "LusterId", spaceAfterParen: true);
            AppendListFilter(ref strWhere, filter.Natts, "NattsId");
            AppendListFilter(ref strWhere, filter.Mines, "Minesid");
            AppendListFilter(ref strWhere, filter.ProductType, "ProductType");
            AppendListFilter(ref strWhere, filter.Sieve, "Sieve");

            // GROUP BY — exact MVC text (leading space kept so date-only WHERE stays valid)
            if (filter.PurAna == "Lab")
                strWhere += " GROUP BY ViewItem.LAB,ViewItem.CertPriority ";
            if (filter.PurAna == "Shape")
                strWhere += " GROUP BY ViewItem.Shp,ViewItem.ShapePriority ";
            if (filter.PurAna == "Color")
                strWhere += " GROUP BY ViewItem.Col,ViewItem.ColorPriority ";
            if (filter.PurAna == "Clarity")
                strWhere += " GROUP BY ViewItem.Cla,ViewItem.ClarityPriority ";
            if (filter.PurAna == "Size")
                strWhere += " GROUP BY ViewItem.SizeID,ViewItem.SizePriority ";
            if (filter.PurAna == "Prop")
                strWhere += " GROUP BY ViewITem.Prop,ViewItem.PropPriority ";
            if (filter.PurAna == "Polish")
                strWhere += " GROUP BY ViewITem.Pol,ViewItem.PolishPriority ";
            if (filter.PurAna == "Symmetry")
                strWhere += " GROUP BY ViewITem.Sym,ViewItem.SymPriority ";
            if (filter.PurAna == "CPS")
                strWhere += " GROUP BY ViewITem.CPSID ";
            if (filter.PurAna == "Fluorescence")
                strWhere += " GROUP BY ViewITem.FL,ViewItem.FLPriority ";
            if (filter.PurAna == "Series")
                strWhere += " GROUP BY ViewItem.Kapan ";
            if (filter.PurAna == "Location")
                strWhere += " GROUP BY ViewItem.Country ";
            if (filter.PurAna == "YearMonth")
                strWhere += " GROUP BY Year(tspt.SPDate), Month(tspt.SPDate), DATENAME(month, tspt.SPDate) ";
            else if (filter.PurAna == "Aging")
                strWhere += " GROUP BY StoneAge ";
            if (filter.PurAna == "PriceRevision")
                strWhere += " GROUP BY PriceRevision ";
            if (filter.PurAna == "PartyLocation")
                strWhere += " GROUP BY tCountry.CountryName  ";
            if (filter.PurAna == "StockID")
                strWhere += " GROUP BY ViewItem.LotNo  ";

            // Order By — exact MVC text
            if (filter.PurAna == "Lab")
                strWhere += "Order By ViewItem.CertPriority ";
            if (filter.PurAna == "Shape")
                strWhere += "Order By ViewItem.ShapePriority ";
            if (filter.PurAna == "Color")
                strWhere += "Order By ViewItem.ColorPriority ";
            if (filter.PurAna == "Clarity")
                strWhere += "Order By ViewItem.ClarityPriority ";
            if (filter.PurAna == "Size")
                strWhere += "  Order By ViewItem.SizePriority";
            if (filter.PurAna == "Prop")
                strWhere += "  Order By ViewItem.PropPriority";
            if (filter.PurAna == "Polish")
                strWhere += "  Order By ViewItem.PolishPriority";
            if (filter.PurAna == "Symmetry")
                strWhere += "  Order By ViewItem.SymPriority";
            if (filter.PurAna == "CPS")
                strWhere += "  Order By ViewItem.CPSID";
            if (filter.PurAna == "Fluorescence")
                strWhere += "  Order By ViewItem.FLPriority";
            if (filter.PurAna == "Series")
                strWhere += "  Order By ViewItem.Kapan";
            if (filter.PurAna == "Location")
                strWhere += "  Order By ViewItem.Country";
            if (filter.PurAna == "YearMonth")
                strWhere += "  Order By Year(tspt.SPDate), Month(tspt.SPDate)";
            if (filter.PurAna == "Aging")
                strWhere += "  Order By StoneAge";
            if (filter.PurAna == "PriceRevision")
                strWhere += "  Order By PriceRevision";
            if (filter.PurAna == "PartyLocation")
                strWhere += "  Order By tCountry.CountryName ";
            if (filter.PurAna == "StockID")
                strWhere += "  Order By ViewItem.LotNo ";

            // ReportProcessor.Pur*AnaReport @Type
            string typeName = filter.PurAna switch
            {
                "Lab" => "PurLabAnalysis",
                "Shape" => "PurShapeAnalysis",
                "Color" => "PurColorAnalysis",
                "Clarity" => "PurClarityAnalysis",
                "Size" => "PurSizeAnalysis",
                "Prop" => "PurPropAnalysis",
                "Polish" => "PurPolishAnalysis",
                "Symmetry" => "PurSymAnalysis",
                "CPS" => "PurCPSAnalysis",
                "Fluorescence" => "PurFLAnalysis",
                "Series" => "PurSeriesAnalysis",
                "Location" => "PurLocationAnalysis",
                "YearMonth" => "PurPurYearMonthAnalysis",
                "Aging" => "PurAgingAnalysis",
                "PriceRevision" => "PurPriceRevisionAnalysis",
                "PartyLocation" => "PurPartyLocationAnalysis",
                "StockID" => "PurStockIDAnalysis",
                _ => throw new ArgumentException("Invalid PurAna grouping: " + filter.PurAna)
            };

            string sql = @" EXEC [GetPurchaseAnalysisData] @Type='" + typeName + "', @Where = '" + strWhere + "' ";

            var data = await _sqlDataAccess.SelectRawSql<PurchaseAnalysisReportDto>(sql, companyId);
            return data.ToList();
        }

        /// <summary>
        /// MVC skips null. Also skip empty and Swagger example placeholder "string".
        /// </summary>
        private static bool HasText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static void AppendListFilter(ref string strWhere, List<string> list, string columnName, bool spaceAfterParen = false)
        {
            // MVC: if (filter.X != null) then build; empty select2 = null so skipped
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

            // MVC Luster uses " In ( " with space after (
            string open = spaceAfterParen ? " ( " : " (";
            strWhere += " And " + columnName + " In" + open + joined + ")";
        }
    }
}
