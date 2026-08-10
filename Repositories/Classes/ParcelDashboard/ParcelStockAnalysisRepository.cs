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
    public class ParcelStockAnalysisRepository : IParcelStockAnalysisRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelStockAnalysisRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        private static bool HasText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (string.Equals(value.Trim(), "string", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        public async Task<List<ParcelChartDataDto>?> GetParcelChartDataAsync(ParcelStockAnalysisRequestDto filter, string companyId)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            string Where = "";
            string Date = HasText(filter.Date) ? filter.Date! : DateTime.Now.ToString("yyyy-MM-dd");

            string Show = HasText(filter.Show) ? filter.Show! : string.Empty;
            if (!string.Equals(Show, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Show))
            {
                if (string.Equals(Show, "Sold", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                }
                else if (string.Equals(Show, "Unsold", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
                }
            }

            string HoldUnHold = HasText(filter.HoldUnHold) ? filter.HoldUnHold! : string.Empty;
            if (!string.Equals(HoldUnHold, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(HoldUnHold))
            {
                if (string.Equals(HoldUnHold, "Hold", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And NotShow = 1 ";
                }
                else if (string.Equals(HoldUnHold, "Unhold", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And NotShow = 0 ";
                }
            }

            string StoneTypes = HasText(filter.StoneTypes) ? filter.StoneTypes! : string.Empty;
            if (!string.Equals(StoneTypes, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(StoneTypes))
            {
                if (string.Equals(StoneTypes, "Single", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And  singlepcs = 1 ";
                }
                else if (string.Equals(StoneTypes, "Parcel", StringComparison.OrdinalIgnoreCase))
                {
                    Where += " And  singlepcs = 0 ";
                }
            }

            string Status = HasText(filter.Status) ? filter.Status! : string.Empty;
            if (!string.Equals(Status, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Status))
            {
                if (Status == "OnHand")
                {
                    Where += " And onhand>0.01 And CurStatus in (''S'') ";
                }
                else if (Status == "OnMemo")
                {
                    Where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                }
                else if (Status == "OnHand&Int.Memo")
                {
                    Where += " And CurStatus in (''S'',''I'') ";
                }
                else if (Status == "OnHand + Memo In")
                {
                    Where += " And CurStatus in (''S'',''MI'') ";
                }
                else if (Status == "OnHand + Party Hold")
                {
                    Where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                }
                else if (Status == "Memo In")
                {
                    Where += " And CurStatus in (''MI'') ";
                }
                else if (Status == "Party Hold")
                {
                    Where += " And CurStatus in (''PH'') ";
                }
                else if (Status == "All - Party Hold")
                {
                    Where += " And CurStatus not in (''PH'') ";
                }
            }

            float BackPer = filter.BackPer;
            float BackPlusMinus = filter.BackPlusMinus;
            float RatePer = filter.RatePer;
            float RatePlusMinus = filter.RatePlusMinus;

            string AskingQry = "";

            if (BackPer != 0)
            {
                AskingQry = " Rapa+(Rapa*(" + BackPer + ")/100) AS AskingRate,(Rapa+(Rapa*(" + BackPer + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount," + BackPer + " As AskingBack ";
            }
            else if (BackPlusMinus != 0)
            {
                AskingQry = " Rapa+(Rapa*(ViewStockReport.back+" + BackPlusMinus + ")/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+" + BackPlusMinus + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+" + BackPlusMinus + " As AskingBack ";
            }
            else if (RatePer != 0)
            {
                AskingQry = " (ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }
            else if (RatePlusMinus != 0)
            {
                AskingQry = " Rate+" + RatePlusMinus + " AS AskingRate, ((rate+" + RatePlusMinus + ") * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+" + RatePlusMinus + " )*100) / Rapa) - 100) END As AskingBack ";
            }
            else
            {
                AskingQry = " SUM(Rate) AS AskingRate,SUM(Back) AS AskingBack,ROUND(CONVERT(DECIMAL(18, 2), SUM((TotCarats + MemoIn) * ISNULL(Rate, 0))), 2,1) AS AskingAmount";
            }

            AppendListFilter(ref Where, filter.ShapeGroup, "ShapeGroupId");
            AppendListFilter(ref Where, filter.Shape, "Shp");
            AppendListFilter(ref Where, filter.ColorGroup, "ColorGroupId");
            AppendListFilter(ref Where, filter.Color, "Col");
            AppendListFilter(ref Where, filter.Size, "Size");
            AppendListFilter(ref Where, filter.Clarity, "Cla");
            AppendListFilter(ref Where, filter.Sieve, "Sieve");
            AppendListFilter(ref Where, filter.Location, "City");
            AppendListFilter(ref Where, filter.ItemGroup, "ItemgroupId");
            AppendListFilter(ref Where, filter.BoxNm, "BoxName");

            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
            {
                Where += " And Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) between " + filter.CtsFrm + " And " + filter.CtsTo + "";
            }

            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
            {
                Where += " And (Rate between " + filter.PriceFrm + " And " + filter.PriceTo + ")";
            }

            if (filter.LotFrm != 0 && filter.LotTo != 0)
            {
                Where += " And (ItemSrno between " + filter.LotFrm + " And " + filter.LotTo + ")";
            }

            if (HasText(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo!;
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                Where += " AND (LotNo IN (''" + strTmpNewLine + "''))";
            }

            string stockAna = HasText(filter.StockAna) ? filter.StockAna! : string.Empty;
            if (string.Equals(stockAna, "Shape", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY Shp ";
            }
            else if (string.Equals(stockAna, "Color", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY Col ";
            }
            else if (string.Equals(stockAna, "Size", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY Size ";
            }
            else if (string.Equals(stockAna, "Clarity", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY Cla ";
            }
            else if (string.Equals(stockAna, "Sieve", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY Sieve ";
            }
            else if (string.Equals(stockAna, "StockID", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY LotNo ";
            }
            else if (string.Equals(stockAna, "Location", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY ISNULL(Country,''''); ";
            }
            else if (string.Equals(stockAna, "ItemGroup", StringComparison.OrdinalIgnoreCase))
            {
                Where += " GROUP BY ItemGroupId ";
            }

            string? spType = stockAna.ToLowerInvariant() switch
            {
                "shape" => "ShapeAnalysis",
                "color" => "ColorAnalysis",
                "size" => "SizeAnalysis",
                "clarity" => "ClarityAnalysis",
                "sieve" => "SieveAnalysis",
                "stockid" => "StockIDAnalysis",
                "location" => "LocationAnalysis",
                "itemgroup" => "ItemGroupAnalysis",
                _ => null
            };

            if (spType == null)
            {
                return null;
            }

            string sql = @"  EXEC [GetSolitaireStockAnalysisData] @Type='" + spType + "', @Date = '''" + Date + "''',@AskingQry = '" + AskingQry + "',@Where = '" + Where + "' ";

            var data = await _sqlDataAccess.SelectRawSql<ParcelChartDataDto>(sql, companyId);
            return data.ToList();
        }

        public async Task<List<ParcelStockPivotDto>> GetParcelPivotDataAsync(ParcelStockAnalysisRequestDto filter, string companyId)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            string Where = "";
            string Date = HasText(filter.Date) ? filter.Date! : DateTime.Now.ToString("yyyy-MM-dd");

            string Show = HasText(filter.Show) ? filter.Show! : string.Empty;
            if (!string.Equals(Show, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Show))
            {
                if (string.Equals(Show, "Sold", StringComparison.OrdinalIgnoreCase))
                    Where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                else if (string.Equals(Show, "Unsold", StringComparison.OrdinalIgnoreCase))
                    Where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
            }

            string HoldUnHold = HasText(filter.HoldUnHold) ? filter.HoldUnHold! : string.Empty;
            if (!string.Equals(HoldUnHold, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(HoldUnHold))
            {
                if (string.Equals(HoldUnHold, "Hold", StringComparison.OrdinalIgnoreCase))
                    Where += " And NotShow = 1 ";
                else if (string.Equals(HoldUnHold, "Unhold", StringComparison.OrdinalIgnoreCase))
                    Where += " And NotShow = 0 ";
            }

            string StoneTypes = HasText(filter.StoneTypes) ? filter.StoneTypes! : string.Empty;
            if (!string.Equals(StoneTypes, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(StoneTypes))
            {
                if (string.Equals(StoneTypes, "Single", StringComparison.OrdinalIgnoreCase))
                    Where += " And  singlepcs = 1 ";
                else if (string.Equals(StoneTypes, "Parcel", StringComparison.OrdinalIgnoreCase))
                    Where += " And  singlepcs = 0 ";
            }

            string Status = HasText(filter.Status) ? filter.Status! : string.Empty;
            if (!string.Equals(Status, "All", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Status))
            {
                if (Status == "OnHand")
                    Where += " And onhand>0.01 And CurStatus in (''S'') ";
                else if (Status == "OnMemo")
                    Where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                else if (Status == "OnHand&Int.Memo")
                    Where += " And CurStatus in (''S'',''I'') ";
                else if (Status == "OnHand + Memo In")
                    Where += " And CurStatus in (''S'',''MI'') ";
                else if (Status == "OnHand + Party Hold")
                    Where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                else if (Status == "Memo In")
                    Where += " And CurStatus in (''MI'') ";
                else if (Status == "Party Hold")
                    Where += " And CurStatus in (''PH'') ";
                else if (Status == "All - Party Hold")
                    Where += " And CurStatus not in (''PH'') ";
            }

            float BackPer = filter.BackPer;
            float BackPlusMinus = filter.BackPlusMinus;
            float RatePer = filter.RatePer;
            float RatePlusMinus = filter.RatePlusMinus;

            string AskingQry;
            if (BackPer != 0)
            {
                AskingQry = " Rapa+(Rapa*(" + BackPer + ")/100) AS AskingRate,(Rapa+(Rapa*(" + BackPer + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount," + BackPer + " As AskingBack ";
            }
            else if (BackPlusMinus != 0)
            {
                AskingQry = " Rapa+(Rapa*(ViewStockReport.back+" + BackPlusMinus + ")/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+" + BackPlusMinus + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+" + BackPlusMinus + " As AskingBack ";
            }
            else if (RatePer != 0)
            {
                AskingQry = " (ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*(" + RatePer + ")/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }
            else if (RatePlusMinus != 0)
            {
                AskingQry = " Rate+" + RatePlusMinus + " AS AskingRate, ((rate+" + RatePlusMinus + ") * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+" + RatePlusMinus + " )*100) / Rapa) - 100) END As AskingBack ";
            }
            else
            {
                AskingQry = " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 3), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
            }

            AppendListFilter(ref Where, filter.ShapeGroup, "ShapeGroupId");
            AppendListFilter(ref Where, filter.Shape, "Shp");
            AppendListFilter(ref Where, filter.ColorGroup, "ColorGroupId");
            AppendListFilter(ref Where, filter.Color, "Col");
            AppendListFilter(ref Where, filter.Size, "Size");
            AppendListFilter(ref Where, filter.Clarity, "Cla");
            AppendListFilter(ref Where, filter.Proportion, "Prop");
            AppendListFilter(ref Where, filter.Polish, "Pol");
            AppendListFilter(ref Where, filter.Symmetry, "Sym");
            AppendListFilter(ref Where, filter.LAB, "Cert");
            AppendListFilter(ref Where, filter.HA, "HA");
            AppendListFilter(ref Where, filter.Location, "City");
            AppendListFilter(ref Where, filter.Fluorescence, "FL");
            AppendListFilter(ref Where, filter.Milky, "[Ml.]");
            AppendListFilter(ref Where, filter.Luster, "[LS.]");
            AppendListFilter(ref Where, filter.Natts, "[Nts.]");
            AppendListFilter(ref Where, filter.Shade, "Shade");
            AppendListFilter(ref Where, filter.FlColor, "FLColor");
            AppendListFilter(ref Where, filter.ItemGroup, "ItemgroupId");
            AppendListFilter(ref Where, filter.BoxNm, "BoxName");
            AppendListFilter(ref Where, filter.Sieve, "Sieve");

            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
                Where += " And Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) between " + filter.CtsFrm + " And " + filter.CtsTo + "";

            if (filter.BackFrm != 0 && filter.BackTo != 0)
                Where += " And (Back between " + filter.BackFrm + " And " + filter.BackTo + ")";

            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
                Where += " And (Rate between " + filter.PriceFrm + " And " + filter.PriceTo + ")";

            if (filter.LotFrm != 0 && filter.LotTo != 0)
                Where += " And (ItemSrno between " + filter.LotFrm + " And " + filter.LotTo + ")";

            if (filter.TDepthFrm != 0 && filter.TDepthTo != 0)
                Where += " And (TDep between " + filter.TDepthFrm + " And " + filter.TDepthTo + ")";

            if (filter.TablePerFrm != 0 && filter.TablePerTo != 0)
                Where += " And ([Tab%] between " + filter.TablePerFrm + " And " + filter.TablePerTo + ")";

            if (filter.CAFrm != 0 && filter.CATo != 0)
                Where += " And (CA between " + filter.CAFrm + " And " + filter.CATo + ")";

            if (filter.CHFrm != 0 && filter.CHTo != 0)
                Where += " And (CH between " + filter.CHFrm + " And " + filter.CHTo + ")";

            if (filter.PHFrm != 0 && filter.PHTo != 0)
                Where += " And (PH between " + filter.PHFrm + " And " + filter.PHTo + ")";

            if (filter.PAFrm != 0 && filter.PATo != 0)
                Where += " And (PA between " + filter.PAFrm + " And " + filter.PATo + ")";

            if (filter.PurchaseChkBox && HasText(filter.FrmPurchaseDate) && HasText(filter.ToPurchaseDate))
                Where += " And (convert(date,PDate,111) Between ''" + filter.FrmPurchaseDate + "'' And ''" + filter.ToPurchaseDate + "'' ) ";

            if (filter.ReviseCheckBox && HasText(filter.FrmReviseDate) && HasText(filter.ToReviseDate))
                Where += " And (convert(date,RDate,111) Between ''" + filter.FrmReviseDate + "'' And ''" + filter.ToReviseDate + "'' ) ";

            if (filter.NewArrivalChkBox && HasText(filter.FrmNewArrivalDate) && HasText(filter.ToNewArrivalDate))
                Where += " And (convert(date,NewDate,111) Between ''" + filter.FrmNewArrivalDate + "'' And ''" + filter.ToNewArrivalDate + "'' ) ";

            if (filter.WebRevisedChkBox && HasText(filter.FrmWebRevisedDate) && HasText(filter.ToWebRevisedDate))
                Where += " And (convert(date,WebRDate,111) Between ''" + filter.FrmWebRevisedDate + "'' And ''" + filter.ToWebRevisedDate + "'' ) ";

            if (filter.UnholdDateChkBox && HasText(filter.FrmUnholdDate) && HasText(filter.ToUnholdDate))
                Where += " And (convert(date,UnholdDate,111) Between ''" + filter.FrmUnholdDate + "'' And ''" + filter.ToUnholdDate + "'' ) ";

            if (filter.LabDateChkBox && HasText(filter.FrmLabDate) && HasText(filter.ToLabDate))
                Where += " And (convert(date,GRecDate,111) Between ''" + filter.FrmLabDate + "'' And ''" + filter.ToLabDate + "'' ) ";

            if (HasText(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo!;
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                Where += " AND (LotNo IN (''" + strTmpNewLine + "''))";
            }

            string sql = @" EXEC [ParcelGetStockReportData] @Date = '''" + Date + "''',@AskingQry = '" + AskingQry + "',@Where = '" + Where + "' ";

            var rawRows = (await _sqlDataAccess.SelectRawSql<ParcelStockPivotRawDto>(sql, companyId)).ToList();
            return rawRows.Select(MapRawToPivotDto).ToList();
        }

        private static ParcelStockPivotDto MapRawToPivotDto(ParcelStockPivotRawDto r)
        {
            string? shape = FirstNonEmptyOrNull(r.Shp, r.Shape);
            string? color = FirstNonEmptyOrNull(r.Col, r.Color);
            string? clarity = FirstNonEmptyOrNull(r.Cla, r.Clarity);
            string? location = FirstNonEmptyOrNull(r.City, r.Country);
            string? size = CleanOrNull(r.Size);
            string? sieve = CleanOrNull(r.Sieve);

            return new ParcelStockPivotDto
            {
                LotNo = r.LotNo,
                Shape = shape,
                Color = color,
                Clarity = clarity,
                Size = size,
                Sieve = sieve,
                Location = location,
                City = r.City,
                Country = r.Country,
                BoxName = r.BoxName,
                ItemGroupId = r.ItemGroupId,
                CurStatus = r.CurStatus,
                Prop = r.Prop,
                Pol = r.Pol,
                Sym = r.Sym,
                FL = r.FL,
                LAB = r.LAB,
                CNo = r.CNo,
                HA = r.HA,
                Nts = r.Nts,
                Brown = r.Brown,
                Milky = r.Milky,
                luster = r.luster,
                Shade = r.Shade,
                Packet = r.Packet,
                Kapan = r.Kapan,
                FLColor = r.FLColor,
                Pcs = r.Pcs,
                Carats = ParseFloat(r.carats),
                Amount = ParseFloat(r.Amount),
                Rate = ParseFloat(r.Rate),
                Back = ParseFloat(r.Back),
                Rapa = r.Rapa,
                MRPAmount = r.MRPAmount,
                PurchaseBack = r.PurchaseBack,
                PurchaseRate = r.PurchaseRate,
                PurChaseAmount = r.PurChaseAmount,
                AskingBack = r.AskingBack,
                AskingRate = r.AskingRate,
                AskingAmount = r.AskingAmount
            };
        }

        private static string? FirstNonEmptyOrNull(params string?[] values)
        {
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v) && !string.Equals(v.Trim(), "null", StringComparison.OrdinalIgnoreCase))
                    return v.Trim();
            }
            return null;
        }

        private static string? CleanOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase))
                return null;
            return value.Trim();
        }

        private static float ParseFloat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0f;
            var cleaned = value.Trim().Replace(",", "").Replace("$", "");
            if (float.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var n))
                return n;
            return 0f;
        }

        private static void AppendListFilter(ref string strWhere, List<string>? list, string columnName, bool spaceAfterParen = true)
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

        public async Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            return await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("AllMasterDataForAdminStockSearch", new Dictionary<string, object>(), companyId);
        }
    }
}
