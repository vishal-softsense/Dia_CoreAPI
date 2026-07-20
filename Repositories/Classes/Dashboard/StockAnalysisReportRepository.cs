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
    /// Port of MVC ChartsReportController.ChartData, StockAnalysisReport, and StockSearchController.GetPivotData.
    /// SPs: AllMasterDataForAdminStockSearch, GetSolitaireStockAnalysisData, SP_StockAnalysisReport, GetStockReportData.
    /// </summary>
    public class StockAnalysisReportRepository : IStockAnalysisReportRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        private static readonly HashSet<string> PivotByFilterGroupings = new(StringComparer.OrdinalIgnoreCase)
        {
            "Shape", "Size", "Color", "Clarity", "Prop", "Symmetry", "FL", "Polish"
        };

        public StockAnalysisReportRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<List<BasicData>> GetDropdownDataAsync(string companyId)
        {
            var result = await _sqlDataAccess.SelectDataFromSP<BasicData>(
                "AllMasterDataForAdminStockSearch",
                null,
                companyId);
            return result.ToList();
        }

        public async Task<List<StockAnalysisReportDto>> GetReportAsync(StockAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (string.IsNullOrWhiteSpace(filter.StockAna))
                throw new ArgumentException("StockAna grouping is required.");

            var (dateVal, where, askingQry) = BuildBaseWhere(filter, forChartReport: true);
            AppendGroupBy(ref where, filter.StockAna);
            AppendChartOrderBy(ref where, filter.FilterSize);

            string typeName = GetChartTypeName(filter.StockAna);
            string sql = $" EXEC [GetSolitaireStockAnalysisData] @Type='{typeName}', @Date = '''{dateVal}''',@AskingQry = '{askingQry}',@Where = '{where}' ";

            var data = await _sqlDataAccess.SelectRawSql<StockAnalysisReportDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<List<StockAnalysisPivotByFilterDto>> GetPivotByFilterAsync(StockAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            string groupBy = !string.IsNullOrWhiteSpace(filter.GroupBy) ? filter.GroupBy : filter.StockAna;
            if (string.IsNullOrWhiteSpace(groupBy) || !PivotByFilterGroupings.Contains(groupBy))
                throw new ArgumentException("Invalid Group By selection! Please select Shape, Size, Color, Clarity, Proportion, Symmetry, FL, Polish.");

            string where = BuildPivotByFilterWhere(filter);
            string rowDimension = MapPivotRowDimension(groupBy);

            string sql = $" EXEC [SP_StockAnalysisReport] @Where = '{where}' , @RowDimension = '{rowDimension}' ";
            var data = await _sqlDataAccess.SelectRawSql<StockAnalysisPivotByFilterDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<List<StockAnalysisPivotFixedDto>> GetPivotFixedAsync(StockAnalysisRequestDto filter, string companyId)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var (dateVal, where, askingQry) = BuildBaseWhere(filter, forChartReport: false);
            AppendPivotFixedOrderBy(ref where, filter.FilterSize);

            string sql = $" EXEC [GetStockReportData] @Date = '''{dateVal}''',@AskingQry = '{askingQry}',@Where = '{where}',@ActionType = 'StockReportPivot' ";
            var data = await _sqlDataAccess.SelectRawSql<StockAnalysisPivotFixedDto>(sql, companyId);
            return data.ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // WHERE builders
        // ─────────────────────────────────────────────────────────────────────────

        private static (string dateVal, string where, string askingQry) BuildBaseWhere(StockAnalysisRequestDto filter, bool forChartReport)
        {
            string dateVal = HasText(filter.Date) ? filter.Date : DateTime.Now.ToString("yyyy-MM-dd");
            string where = "";

            if (HasText(filter.Show) && filter.Show != "All")
            {
                if (filter.Show == "Sold")
                    where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                else if (filter.Show == "Unsold")
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
            }

            if (HasText(filter.HoldUnHold) && filter.HoldUnHold != "All")
            {
                if (filter.HoldUnHold == "Hold")
                    where += " And NotShow = 1 ";
                else if (filter.HoldUnHold == "Unhold")
                    where += " And NotShow = 0 ";
            }

            if (HasText(filter.StoneTypes) && filter.StoneTypes != "All")
            {
                if (filter.StoneTypes == "Single")
                    where += " And  singlepcs = 1 ";
                else if (filter.StoneTypes == "Parcel")
                    where += " And  singlepcs = 0 ";
            }

            if (HasText(filter.Status) && filter.Status != "All")
            {
                switch (filter.Status)
                {
                    case "OnHand":
                        where += " And onhand>0.01 And CurStatus in (''S'') ";
                        break;
                    case "OnMemo":
                        where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                        break;
                    case "OnHand&Int.Memo":
                        where += " And CurStatus in (''S'',''I'') ";
                        break;
                    case "OnHand + Memo In":
                        where += " And CurStatus in (''S'',''MI'') ";
                        break;
                    case "OnHand + Party Hold":
                        where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                        break;
                    case "Memo In":
                        where += " And CurStatus in (''MI'') ";
                        break;
                    case "Party Hold":
                        where += " And CurStatus in (''PH'') ";
                        break;
                    case "All - Party Hold":
                        where += " And CurStatus not in (''PH'') ";
                        break;
                }
            }

            string askingQry = BuildAskingQry(filter, forChartReport);

            AppendListFilter(ref where, filter.Shape, "Shp");
            AppendListFilter(ref where, filter.Color, "Col");
            AppendListFilter(ref where, filter.Size, "Size");
            AppendListFilter(ref where, filter.Clarity, "Cla");
            AppendListFilter(ref where, filter.Proportion, "Prop");
            AppendListFilter(ref where, filter.Polish, "Pol");
            AppendListFilter(ref where, filter.Symmetry, "Sym");
            AppendListFilter(ref where, filter.LAB, "Cert");
            AppendListFilter(ref where, filter.Fluorescence, "FL");
            AppendListFilter(ref where, filter.ShapeGroup, "ShapeGroupId");
            AppendListFilter(ref where, filter.ColorGroup, "ColorGroupId");
            AppendListFilter(ref where, filter.Rough, "Rough");
            AppendListFilter(ref where, filter.HA, "HA");
            AppendListFilter(ref where, filter.Location, "City");
            AppendListFilter(ref where, filter.Milky, "[Ml.]");
            AppendListFilter(ref where, filter.Luster, "[LS.]");
            AppendListFilter(ref where, filter.Natts, "[Nts.]");
            AppendListFilter(ref where, filter.Shade, "Shade");
            AppendListFilter(ref where, filter.FlColor, "FLColor");
            AppendListFilter(ref where, filter.ItemGroup, "ItemgroupId");
            AppendListFilter(ref where, filter.Pair, "Pair");
            AppendListFilter(ref where, filter.BoxNm, "BoxName");

            AppendLotFilter(ref where, filter);

            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
                where += " And Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) between " + filter.CtsFrm + " And " + filter.CtsTo + "";
            if (filter.BackFrm != 0 && filter.BackTo != 0)
                where += " And (Back between " + filter.BackFrm + " And " + filter.BackTo + ")";
            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
                where += " And (Rate between " + filter.PriceFrm + " And " + filter.PriceTo + ")";
            if (filter.LotFrm != 0 && filter.LotTo != 0)
                where += " And (ItemSrno between " + filter.LotFrm + " And " + filter.LotTo + ")";
            if (filter.TDepthFrm != 0 && filter.TDepthTo != 0)
                where += " And (TDep between " + filter.TDepthFrm + " And " + filter.TDepthTo + ")";
            if (filter.TablePerFrm != 0 && filter.TablePerTo != 0)
                where += " And ([Tab%] between " + filter.TablePerFrm + " And " + filter.TablePerTo + ")";
            if (filter.CAFrm != 0 && filter.CATo != 0)
                where += " And (CA between " + filter.CAFrm + " And " + filter.CATo + ")";
            if (filter.CHFrm != 0 && filter.CHTo != 0)
                where += " And (CH between " + filter.CHFrm + " And " + filter.CHTo + ")";
            if (filter.PHFrm != 0 && filter.PHTo != 0)
                where += " And (PH between " + filter.PHFrm + " And " + filter.PHTo + ")";
            if (filter.PAFrm != 0 && filter.PATo != 0)
                where += " And (PA between " + filter.PAFrm + " And " + filter.PATo + ")";

            if (filter.PurchaseChkBox && HasText(filter.FrmPurchaseDate) && HasText(filter.ToPurchaseDate))
                where += " And (convert(date,PDate,111) Between ''" + filter.FrmPurchaseDate + "'' And ''" + filter.ToPurchaseDate + "'' ) ";
            if (filter.ReviseCheckBox && HasText(filter.FrmReviseDate) && HasText(filter.ToReviseDate))
                where += " And (convert(date,RDate,111) Between ''" + filter.FrmReviseDate + "'' And ''" + filter.ToReviseDate + "'' ) ";
            if (filter.NewArrivalChkBox && HasText(filter.FrmNewArrivalDate) && HasText(filter.ToNewArrivalDate))
                where += " And (convert(date,NewDate,111) Between ''" + filter.FrmNewArrivalDate + "'' And ''" + filter.ToNewArrivalDate + "'' ) ";
            if (filter.WebRevisedChkBox && HasText(filter.FrmWebRevisedDate) && HasText(filter.ToWebRevisedDate))
                where += " And (convert(date,WebRDate,111) Between ''" + filter.FrmWebRevisedDate + "'' And ''" + filter.ToWebRevisedDate + "'' ) ";
            if (filter.UnholdDateChkBox && HasText(filter.FrmUnholdDate) && HasText(filter.ToUnholdDate))
                where += " And (convert(date,UnholdDate,111) Between ''" + filter.FrmUnholdDate + "'' And ''" + filter.ToUnholdDate + "'' ) ";
            if (filter.LabDateChkBox && HasText(filter.FrmLabDate) && HasText(filter.ToLabDate))
                where += " And (convert(date,GRecDate,111) Between ''" + filter.FrmLabDate + "'' And ''" + filter.ToLabDate + "'' ) ";

            return (dateVal, where, askingQry);
        }

        private static string BuildPivotByFilterWhere(StockAnalysisRequestDto filter)
        {
            string where = "";
            AppendListFilter(ref where, filter.Shape, "Shp");
            AppendListFilter(ref where, filter.Color, "Col");
            AppendListFilter(ref where, filter.Size, "Size");
            AppendListFilter(ref where, filter.Clarity, "Cla");
            AppendListFilter(ref where, filter.Proportion, "Prop");
            AppendListFilter(ref where, filter.Polish, "Pol");
            AppendListFilter(ref where, filter.Symmetry, "Sym");
            AppendListFilter(ref where, filter.LAB, "Cert");
            AppendListFilter(ref where, filter.Fluorescence, "FL");
            AppendListFilter(ref where, filter.ShapeGroup, "ShapeGroupId");
            AppendListFilter(ref where, filter.ColorGroup, "ColorGroupId");
            AppendLotFilter(ref where, filter);
            return where;
        }

        private static string BuildAskingQry(StockAnalysisRequestDto filter, bool forChartReport)
        {
            if (filter.BackPer != 0)
                return " Rapa+(Rapa*(" + filter.BackPer + ")/100) AS AskingRate,(Rapa+(Rapa*(" + filter.BackPer + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount," + filter.BackPer + " As AskingBack ";
            if (filter.BackPlusMinus != 0)
                return " Rapa+(Rapa*(ViewStockReport.back+" + filter.BackPlusMinus + ")/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+" + filter.BackPlusMinus + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+" + filter.BackPlusMinus + " As AskingBack ";
            if (filter.RatePer != 0)
                return " (ROUND(((Rate+(Rate*(" + filter.RatePer + ")/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*(" + filter.RatePer + ")/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*(" + filter.RatePer + ")/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            if (filter.RatePlusMinus != 0)
                return " Rate+" + filter.RatePlusMinus + " AS AskingRate, ((rate+" + filter.RatePlusMinus + ") * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+" + filter.RatePlusMinus + " )*100) / Rapa) - 100) END As AskingBack ";

            if (forChartReport)
                return " SUM(Rate) AS AskingRate,SUM(Back) AS AskingBack,ROUND(CONVERT(DECIMAL(28, 10), SUM((TotCarats + MemoIn) * ISNULL(Rate, 0))), 2) AS AskingAmount";

            return " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 3), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
        }

        private static void AppendGroupBy(ref string where, string stockAna)
        {
            switch (stockAna)
            {
                case "Shape": where += "GROUP BY Shp "; break;
                case "Color": where += "GROUP BY Col "; break;
                case "Size": where += "GROUP BY Size "; break;
                case "Clarity": where += "GROUP BY Cla "; break;
                case "Lab": where += "GROUP BY Lab "; break;
                case "Prop": where += "GROUP BY Prop "; break;
                case "Polish": where += "GROUP BY Pol "; break;
                case "Symmetry": where += "GROUP BY Sym "; break;
                case "FL": where += "GROUP BY FL "; break;
                case "Series": where += "GROUP BY Kapan "; break;
                case "Location": where += "GROUP BY Country "; break;
                case "YearMonth": where += "GROUP BY Convert(varchar(10),Year(PDate)) + ''_'' + Convert(varchar(2),month(Pdate)) "; break;
                case "Aging": where += "GROUP BY StoneAge "; break;
                case "CPS": where += "GROUP BY CPSID "; break;
                case "PriceRevision": where += "GROUP BY PriceRevision "; break;
                case "StockID": where += "GROUP BY LotNo "; break;
                default:
                    throw new ArgumentException("Invalid StockAna grouping: " + stockAna);
            }
        }

        private static void AppendChartOrderBy(ref string where, string filterSize)
        {
            if (!HasText(filterSize)) return;

            switch (filterSize)
            {
                case "Sort On LAB-Shape-Size-Color-Clarity-LotNo":
                    where += "Order By CertPriority,ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On LAB-Shape-Color-Size-Clarity-LotNo":
                    where += "Order By CertPriority,ShapePriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On LAB-Shape-Size-Clarity-Color-LotNo":
                    where += "Order By CertPriority,ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-LAB-Size-Color-Clarity-LotNo":
                    where += "Order By ShapePriority,CertPriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-LAB-Color-Size-Clarity-LotNo":
                    where += "Order By ShapePriority,CertPriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-LAB-Size-Clarity-Color-LotNo":
                    where += "Order By ShapePriority,CertPriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-Size-Clarity-Color-LotNo":
                    where += "Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-Size-Color-Clarity-LotNo":
                    where += "Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,SUM(TotCarats+MemoIn) Desc,LotNo"; break;
                case "Sort On Shape-Size-Color-Clarity-Cut-Polish-Sym-Fl":
                    where += "Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,SUM(TotCarats+MemoIn) Asc,LotNo "; break;
                case "Sort On Kapan-LotNo": where += "Order By Kapan,LotNo"; break;
                case "Sort On ItemCode": where += "Order By ItemCode"; break;
                case "Sort On Item Serial No": where += "Order By ItemSrNo"; break;
                case "Sort On Stone Status & Lotno": where += "Order By Lotno"; break;
                case "Sort On Size Wise": where += "Order By SizePriority,SUM(TotCarats+MemoIn) Asc,LotNo"; break;
                case "Sort On Size-Color-Clarity-Lotno": where += "Order By SizePriority desc,ClarityPriority,ColorPriority"; break;
                case "Sort On ItemGroup Wise": where += "Order By ItemGroupId desc"; break;
                case "Sort On Shape-Size-Clarity-Color": where += "Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority"; break;
            }
        }

        private static void AppendPivotFixedOrderBy(ref string where, string filterSize)
        {
            if (!HasText(filterSize)) return;

            switch (filterSize)
            {
                case "Sort On LAB-Shape-Size-Color-Clarity-LotNo":
                    where += "Order By CertPriority,ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On LAB-Shape-Color-Size-Clarity-LotNo":
                    where += "Order By CertPriority,ShapePriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On LAB-Shape-Size-Clarity-Color-LotNo":
                    where += "Order By CertPriority,ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-LAB-Size-Color-Clarity-LotNo":
                    where += "Order By ShapePriority,CertPriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-LAB-Color-Size-Clarity-LotNo":
                    where += "Order By ShapePriority,CertPriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-LAB-Size-Clarity-Color-LotNo":
                    where += "Order By ShapePriority,CertPriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-Size-Clarity-Color-LotNo":
                    where += "Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-Size-Color-Clarity-LotNo":
                    where += "Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Desc,LotNo"; break;
                case "Sort On Shape-Size-Color-Clarity-Cut-Polish-Sym-Fl":
                    where += "Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Asc,LotNo "; break;
                case "Sort On Kapan-LotNo": where += "Order By Kapan,LotNo"; break;
                case "Sort On ItemCode": where += "Order By ItemCode"; break;
                case "Sort On Item Serial No": where += "Order By ItemSrNo"; break;
                case "Sort On Stone Status & Lotno": where += "Order By Lotno"; break;
                case "Sort On Size Wise": where += "Order By SizePriority,TotCarats+MemoIn Asc,LotNo"; break;
                case "Sort On Size-Color-Clarity-Lotno": where += "Order By SizePriority desc,ClarityPriority,ColorPriority"; break;
                case "Sort On ItemGroup Wise": where += "Order By ItemGroupId desc"; break;
                case "Sort On Shape-Size-Clarity-Color": where += "Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority"; break;
            }
        }

        private static string GetChartTypeName(string stockAna) => stockAna switch
        {
            "Shape" => "ShapeAnalysis",
            "Color" => "ColorAnalysis",
            "Size" => "SizeAnalysis",
            "Clarity" => "ClarityAnalysis",
            "Lab" => "LABAnalysis",
            "Prop" => "PropAnalysis",
            "Polish" => "PolishAnalysis",
            "Symmetry" => "SymmetryAnalysis",
            "FL" => "FlouAnalysis",
            "Series" => "SeriesAnalysis",
            "Location" => "LocationAnalysis",
            "YearMonth" => "PurchaseMonthYearAnalysis",
            "Aging" => "AgeingAnalysis",
            "CPS" => "CPSAnalysis",
            "PriceRevision" => "PriceRevision",
            "StockID" => "StockIDAnalysis",
            _ => throw new ArgumentException("Invalid StockAna grouping: " + stockAna)
        };

        private static string MapPivotRowDimension(string groupBy) => groupBy switch
        {
            "Shape" => "Shp",
            "Size" => "Size",
            "Color" => "Col",
            "Clarity" => "Cla",
            "Prop" => "Prop",
            "Symmetry" => "Sym",
            "Polish" => "Pol",
            "FL" => "FL",
            _ => throw new ArgumentException("Invalid GroupBy: " + groupBy)
        };

        private static void AppendLotFilter(ref string where, StockAnalysisRequestDto filter)
        {
            if (!HasText(filter.LotNo) || !HasText(filter.Filters)) return;

            string strTmpNewLine = filter.LotNo.Replace("\r\n", ",").Replace("\n", ",");
            strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

            switch (filter.Filters)
            {
                case "LotNo":
                    where += " AND (LotNo IN (''" + strTmpNewLine + "''))"; break;
                case "LotNo Not":
                    where += " AND (LotNo NOT IN (''" + strTmpNewLine + "''))"; break;
                case "LotNo Like":
                    where += " AND (LotNo Like (''" + strTmpNewLine + "%''))"; break;
                case "KapanNo":
                    where += " AND (Kapan IN (''" + strTmpNewLine + "''))"; break;
                case "KapanNo Not":
                    where += " AND (Kapan NOT IN (''" + strTmpNewLine + "''))"; break;
                case "PacketNo":
                    where += " AND (Packet IN (''" + strTmpNewLine + "''))"; break;
                case "PacketNo Not":
                    where += " AND (Packet NOT IN (''" + strTmpNewLine + "''))"; break;
            }
        }

        private static bool HasText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static void AppendListFilter(ref string where, List<string> list, string columnName)
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

            where += " And " + columnName + " In ( " + joined + ")";
        }
    }
}
