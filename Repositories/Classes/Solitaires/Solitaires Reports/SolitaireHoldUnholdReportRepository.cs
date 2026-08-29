using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireHoldUnholdReportRepository : ISolitaireHoldUnholdReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireHoldUnholdReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireHoldUnholdReportRowDto>> SearchAsync(
            SolitaireHoldUnholdReportFilterDto filter,
            string companyId)
        {
            var (dateVal, where, askingQry) = BuildQueryParams(filter ?? new SolitaireHoldUnholdReportFilterDto());

            var dbParams = new Dictionary<string, object>
            {
                { "@Date", $"'{dateVal}'" },
                { "@AskingQry", askingQry },
                { "@Where", where.Replace("''", "'") },
                { "@ActionType", "StockReport" }
            };

            var rows = (await _db.SelectDataFromSP<SolitaireHoldUnholdReportRowDto>(
                "GetStockReportData",
                dbParams,
                companyId)).ToList();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.view))
                    row.view = row.PagePath;
            }

            return rows;
        }

        public async Task<bool> HoldRestoreAsync(SolitaireHoldUnholdActionDto action, string companyId)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.LotNo) || string.IsNullOrWhiteSpace(action.Type))
                return false;

            var type = action.Type.Trim().ToLowerInvariant();
            if (type != "hold" && type != "unhold")
                return false;

            // MVC: holdunholdrequest(" AND LotNo in (" + spte.LotNo + ")", actiontype)
            var where = " AND LotNo in (" + action.LotNo + ")";

            var parameters = new Dictionary<string, object>
            {
                { "@Where", where },
                { "@actiontype", type }
            };

            await _db.ExecuteSP("TestingGetItemsByNotShow", parameters, companyId);
            return true;
        }

        /// <summary>
        /// Where / AskingQry built to match MVC ReportController.HoldRestorelist (Show/HoldUnHold/StoneTypes/Status/LotNo + AskingQry).
        /// </summary>
        private static (string dateVal, string where, string askingQry) BuildQueryParams(SolitaireHoldUnholdReportFilterDto filter)
        {
            string dateVal = string.IsNullOrEmpty(filter.Date)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : filter.Date;

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
            string askingQry;

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
                // MVC default AskingQry
                askingQry = " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 3), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
            }

            if (!string.IsNullOrEmpty(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo.Replace(" ", "");
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                where += " AND (LotNo IN (''" + strTmpNewLine + "''))";
            }

            return (dateVal, where, askingQry);
        }
    }
}
