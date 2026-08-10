using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireAdminStockReportRepository : ISolitaireAdminStockReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireAdminStockReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireAdminStockReportResponseDTO>> GetAdminStockSearchReportAsync(SolitaireAdminStockReportDto filter, string companyId)
        {
            var (dateVal, where, askingQry) = BuildQueryParams(filter);

            var dbParams = new Dictionary<string, object>
            {
                { "@Date", $"'{dateVal}'" },
                { "@AskingQry", askingQry },
                { "@Where", where.Replace("''", "'") }
            };

            return await _db.SelectDataFromSP<SolitaireAdminStockReportResponseDTO>("GetAdminReportData", dbParams, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAdminStockSearchDropdownAsync(string companyId)
        {
            return await _db.SelectDataFromSP<BasicDataDto>("AllMasterDataForAdminStockSearch", new Dictionary<string, object>(), companyId);
        }

        public async Task<bool> UpdateLocationAsync(SolitaireAdminLocationUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@City", dto.City ?? "" },
                    { "@Country", dto.Country ?? "" },
                    { "@State", dto.State ?? "" },
                    { "@Lot", lot },
                    { "@Type", "location" }
                };

                await _db.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }

        public async Task<bool> UpdateBoxAndGroupAsync(SolitaireAdminBoxGroupUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@boxname", dto.BoxName ?? "" },
                    { "@groupname", dto.Group ?? "" },
                    { "@Lot", lot },
                    { "@Type", "box-group-update" }
                };

                await _db.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }

        public async Task<IEnumerable<BasicDataDto>> GetAdminSolitaireLotNoListAsync(string searchby, string companyId)
        {
            string strWhere = " AND ISNULL(SinglePcs,0) IN (0,1) ";
            if (!string.IsNullOrEmpty(searchby))
            {
                var cleaned = searchby.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(i => i.Trim())
                                   .Where(i => !string.IsNullOrEmpty(i))
                                   .ToList();

                if (items.Any())
                {
                    if (items.Count == 1)
                    {
                        string safeItem = items[0].Replace("'", "''");
                        strWhere += $" AND (Name LIKE ''%{safeItem}%'' OR TextName LIKE ''%{safeItem}%'') ";
                    }
                    else
                    {
                        string joined = string.Join("','", items.Select(i => i.Replace("'", "''")));
                        strWhere += " AND Name IN ('" + joined + "') ";
                    }
                }
            }

            var dbParams = new Dictionary<string, object>
            {
                { "@strWhere", strWhere }
            };

            return await _db.SelectDataFromSP<BasicDataDto>("GetDiamondLotDropdown", dbParams, companyId);
        }

        private (string dateVal, string where, string askingQry) BuildQueryParams(SolitaireAdminStockReportDto filter)
        {
            string dateVal = string.IsNullOrEmpty(filter.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : filter.Date;
            string where = "";

            string? show = filter.Show;
            if (!string.IsNullOrEmpty(show) && show != "All")
            {
                if (show == "Sold")
                {
                    where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                }
                else if (show == "Unsold")
                {
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
                }
            }

            string? holdUnHold = filter.HoldUnHold;
            if (!string.IsNullOrEmpty(holdUnHold) && holdUnHold != "All")
            {
                if (holdUnHold == "Hold")
                {
                    where += " And NotShow = 1 ";
                }
                else if (holdUnHold == "Unhold")
                {
                    where += " And NotShow = 0 ";
                }
            }

            string? stoneTypes = filter.StoneTypes;
            if (!string.IsNullOrEmpty(stoneTypes) && stoneTypes != "All")
            {
                if (stoneTypes == "Single")
                {
                    where += " And  singlepcs = 1 ";
                }
                else if (stoneTypes == "Parcel")
                {
                    where += " And  singlepcs = 0 ";
                }
            }

            string? status = filter.Status;
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "OnHand")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'') ";
                }
                else if (status == "OnMemo")
                {
                    where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                }
                else if (status == "OnHand&Int.Memo")
                {
                    where += " And CurStatus in (''S'',''I'') ";
                }
                else if (status == "OnHand + Memo In")
                {
                    where += " And CurStatus in (''S'',''MI'') ";
                }
                else if (status == "OnHand + Party Hold")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                }
                else if (status == "Memo In")
                {
                    where += " And CurStatus in (''MI'') ";
                }
                else if (status == "Party Hold")
                {
                    where += " And CurStatus in (''PH'') ";
                }
                else if (status == "All - Party Hold")
                {
                    where += " And CurStatus not in (''PH'') ";
                }
            }

            float backPer = filter.BackPer;
            float backPlusMinus = filter.BackPlusMinus;
            float ratePer = filter.RatePer;
            float ratePlusMinus = filter.RatePlusMinus;
            string askingQry = "";

            if (backPer != 0)
            {
                askingQry = " Rapa+(Rapa*(" + backPer + ")/100) AS AskingRate,(Rapa+(Rapa*(" + backPer + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount," + backPer + " As AskingBack ";
            }
            else if (backPlusMinus != 0)
            {
                askingQry = " Rapa+(Rapa*(ViewStockReport.back+" + backPlusMinus + ")/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+" + backPlusMinus + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+" + backPlusMinus + " As AskingBack ";
            }
            else if (ratePer != 0)
            {
                askingQry = " (ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }
            else if (ratePlusMinus != 0)
            {
                askingQry = " Rate+" + ratePlusMinus + " AS AskingRate, ((rate+" + ratePlusMinus + ") * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+" + ratePlusMinus + " )*100) / Rapa) - 100) END As AskingBack ";
            }
            else
            {
                askingQry = " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 3), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
            }

            AddInClause(ref where, "ShapeGroupId", filter.ShapeGroup);
            AddInClause(ref where, "Shp", filter.Shape);
            AddInClause(ref where, "ColorGroupId", filter.ColorGroup);
            AddInClause(ref where, "Col", filter.Color);
            AddInClause(ref where, "Size", filter.Size);
            AddInClause(ref where, "Cla", filter.Clarity);
            AddInClause(ref where, "Prop", filter.Proportion);
            AddInClause(ref where, "Pol", filter.Polish);
            AddInClause(ref where, "Sym", filter.Symmetry);
            AddInClause(ref where, "Cert", filter.LAB);
            AddInClause(ref where, "HA", filter.HA);
            AddInClause(ref where, "City", filter.Location);
            AddInClause(ref where, "FL", filter.Fluorescence);
            AddInClause(ref where, "[Ml.]", filter.Milky);
            AddInClause(ref where, "[LS.]", filter.Luster);
            AddInClause(ref where, "[Nts.]", filter.Natts);
            AddInClause(ref where, "Shade", filter.Shade);
            AddInClause(ref where, "FLColor", filter.FlColor);
            AddInClause(ref where, "ItemgroupId", filter.ItemGroup);
            AddInClause(ref where, "BoxName", filter.BoxNm);

            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
            {
                where += " And Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) between " + filter.CtsFrm + " And " + filter.CtsTo + "";
            }
            if (filter.BackFrm != 0 && filter.BackTo != 0)
            {
                where += " And (Back between " + filter.BackFrm + " And " + filter.BackTo + ")";
            }
            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
            {
                where += " And (Rate between " + filter.PriceFrm + " And " + filter.PriceTo + ")";
            }
            if (filter.LotFrm != 0 && filter.LotTo != 0)
            {
                where += " And (ItemSrno between " + filter.LotFrm + " And " + filter.LotTo + ")";
            }
            if (filter.TDepthFrm != 0 && filter.TDepthTo != 0)
            {
                where += " And (TDep between " + filter.TDepthFrm + " And " + filter.TDepthTo + ")";
            }
            if (filter.TablePerFrm != 0 && filter.TablePerTo != 0)
            {
                where += " And ([Tab%] between " + filter.TablePerFrm + " And " + filter.TablePerTo + ")";
            }
            if (filter.CAFrm != 0 && filter.CATo != 0)
            {
                where += " And (CA between " + filter.CAFrm + " And " + filter.CATo + ")";
            }
            if (filter.CHFrm != 0 && filter.CHTo != 0)
            {
                where += " And (CH between " + filter.CHFrm + " And " + filter.CHTo + ")";
            }
            if (filter.PHFrm != 0 && filter.PHTo != 0)
            {
                where += " And (PH between " + filter.PHFrm + " And " + filter.PHTo + ")";
            }
            if (filter.PAFrm != 0 && filter.PATo != 0)
            {
                where += " And (PA between " + filter.PAFrm + " And " + filter.PATo + ")";
            }

            if (filter.PurchaseChkBox)
            {
                where += " And (convert(date,PDate,111) Between ''" + filter.FrmPurchaseDate + "'' And ''" + filter.ToPurchaseDate + "'' ) ";
            }
            if (filter.ReviseCheckBox)
            {
                where += " And (convert(date,RDate,111) Between ''" + filter.FrmReviseDate + "'' And ''" + filter.ToReviseDate + "'' ) ";
            }
            if (filter.NewArrivalChkBox)
            {
                where += " And (convert(date,NewDate,111) Between ''" + filter.FrmNewArrivalDate + "'' And ''" + filter.ToNewArrivalDate + "'' ) ";
            }
            if (filter.WebRevisedChkBox)
            {
                where += " And (convert(date,WebRDate,111) Between ''" + filter.FrmWebRevisedDate + "'' And ''" + filter.ToWebRevisedDate + "'' ) ";
            }
            if (filter.UnholdDateChkBox)
            {
                where += " And (convert(date,UnholdDate,111) Between ''" + filter.FrmUnholdDate + "'' And ''" + filter.ToUnholdDate + "'' ) ";
            }
            if (filter.LabDateChkBox)
            {
                where += " And (convert(date,GRecDate,111) Between ''" + filter.FrmLabDate + "'' And ''" + filter.ToLabDate + "'' ) ";
            }

            if (!string.IsNullOrEmpty(filter.LotNo))
            {
                string lotNoClean = filter.LotNo.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = lotNoClean.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(i => i.Trim())
                                      .Where(i => !string.IsNullOrEmpty(i));
                
                if (items.Any())
                {
                    string strTmpNewLine = string.Join("'',''", items.Select(i => i.Replace("'", "''")));

                    if (filter.Filters == "LotNo")
                    {
                        where += " AND (LotNo IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "LotNo Not" || filter.Filters == "LotNoNot")
                    {
                        where += " AND (LotNo NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "LotNo Like" || filter.Filters == "LotNoLike")
                    {
                        where += " AND (LotNo Like (''" + strTmpNewLine + "%''))";
                    }
                    else if (filter.Filters == "CertNo")
                    {
                        where += " AND ([C.No.] IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "CertNo Not" || filter.Filters == "CertNoNot")
                    {
                        where += " AND ([C.No.] NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "SrNo")
                    {
                        where += " AND (ItemSrNo IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "SrNo Not" || filter.Filters == "SrNoNot")
                    {
                        where += " AND (ItemSrNo NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "KapanNo")
                    {
                        where += " AND (Kapan IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "KapanNo Not" || filter.Filters == "KapanNoNot")
                    {
                        where += " AND (Kapan NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "PacketNo")
                    {
                        where += " AND (Packet IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "PacketNo Not" || filter.Filters == "PacketNoNot")
                    {
                        where += " AND (Packet NOT IN (''" + strTmpNewLine + "''))";
                    }
                }
            }

            return (dateVal, where, askingQry);
        }

        private void AddInClause(ref string where, string columnName, List<string>? list)
        {
            if (list == null || list.Count == 0) return;
            string inClause = BuildInClauseString(list);
            if (!string.IsNullOrEmpty(inClause))
            {
                where += $" And {columnName} In ( {inClause})";
            }
        }

        private string BuildInClauseString(List<string> list)
        {
            var escaped = list.Select(el => $"''{el.Replace("'", "''")}''");
            return string.Join(",", escaped);
        }
    }
}
