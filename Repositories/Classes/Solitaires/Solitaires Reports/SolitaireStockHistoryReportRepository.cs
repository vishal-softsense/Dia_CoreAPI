using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireStockHistoryReportRepository : ISolitaireStockHistoryReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireStockHistoryReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireStockHistoryReportSpRowDto>> GetStockHistoryReportAsync(
            SolitaireStockHistoryReportDto filter,
            string companyId)
        {
            var where = BuildStockRegisterWhere(filter);
            var actionType = BuildStockRegisterActionType(filter);

            var dbParams = new Dictionary<string, object>
            {
                { "@Where", where.Replace("''", "'") },
                { "@ActionType", actionType }
            };

            return await _db.SelectDataFromSP<SolitaireStockHistoryReportSpRowDto>(
                "GetStockRegister",
                dbParams,
                companyId);
        }

        private static string BuildStockRegisterWhere(SolitaireStockHistoryReportDto filter)
        {
            string strWhere = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = string.Empty;
                    strTmpNewLine = filter.LotNo.Replace(" ", "");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    strWhere += " AND Lotnosearch  IN ('" + strTmpNewLine + "')";
                }

                string Shape = "";
                if (filter.Shape != null)
                {
                    filter.Shape.ForEach(el => Shape += "''" + el.ToString() + "'',");
                    if (Shape.Trim().Length > 1)
                        Shape = Shape.Remove(Shape.Length - 1);

                    if (Shape.Length > 0)
                        strWhere += " And  Shapeid  In (" + Shape + ")";
                }

                string Color = "";
                if (filter.Color != null)
                {
                    filter.Color.ForEach(el => Color += "''" + el.ToString() + "'',");
                    if (Color.Trim().Length > 1)
                        Color = Color.Remove(Color.Length - 1);

                    if (Color.Length > 0)
                        strWhere += " And ColorId In (" + Color + ")";
                }

                string Clarity = "";
                if (filter.Clarity != null)
                {
                    filter.Clarity.ForEach(el => Clarity += "''" + el.ToString() + "'',");
                    if (Clarity.Trim().Length > 1)
                        Clarity = Clarity.Remove(Clarity.Length - 1);

                    if (Clarity.Length > 0)
                        strWhere += " And ClarityId In (" + Clarity + ")";
                }

                string Size = "";
                if (filter.Size != null)
                {
                    filter.Size.ForEach(el => Size += "''" + el.ToString() + "'',");
                    if (Size.Trim().Length > 1)
                        Size = Size.Remove(Size.Length - 1);

                    if (Size.Length > 0)
                        strWhere += " And SizeId In (" + Size + ")";
                }

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND Trndate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                List<string> stkIn = new List<string> { "OpStk" };
                List<string> stkOut = new List<string>();

                var selectedTypes = filter.SelectedTypes ?? new List<string>();
                if (!selectedTypes.Any())
                    selectedTypes = new List<string> { "ALL" };

                foreach (var type in selectedTypes)
                {
                    switch (type)
                    {
                        case "ALL":
                            stkIn.AddRange(new[]
                            {
                                "OpStk","PUR", "SaleRtn",
                                "MemoIn", "MemoRtn","StkTrn","PartyHold"
                            });

                            stkOut.AddRange(new[]
                            {
                                "SALE", "PurRtn",
                                "MemoOut", "MemoInRtn","StkTrn","PartyHold"
                            });

                            goto Done;

                        case "PUR": stkIn.Add("PUR"); break;
                        case "PURRTN": stkOut.Add("PurRtn"); break;
                        case "SALE": stkOut.Add("SALE"); break;
                        case "SALERTN": stkIn.Add("SaleRtn"); break;
                        case "MEMOIN": stkIn.Add("MemoIn"); break;
                        case "MEMOINRTN": stkOut.Add("MemoInRtn"); break;
                        case "MEMOOUT": stkOut.Add("MemoOut"); break;
                        case "MEMOOUTRTN": stkIn.Add("MemoRtn"); break;
                        case "STKTRN": stkIn.Add("StkTrn"); break;
                        case "PARTYHOLD": stkIn.Add("PartyHold"); break;
                        case "PARTYHOLDRTN": stkIn.Add("PartyHold"); break;
                    }
                }

            Done:
                stkIn = stkIn.Distinct().ToList();
                stkOut = stkOut.Distinct().ToList();

                var allTrnTypes = stkIn
                    .Union(stkOut)
                    .Distinct()
                    .ToList();

                if (!allTrnTypes.Any())
                    allTrnTypes.Add("__NA__");

                string trnTypeStr = string.Join(
                    ", ",
                    allTrnTypes.Select(x => $"'{x}'"));

                strWhere += $" AND TrnType IN ({trnTypeStr})";
            }
            catch
            {
            }

            return strWhere;
        }

        private static string BuildStockRegisterActionType(SolitaireStockHistoryReportDto filter)
        {
            string actionType = "";
            try
            {
                if (filter.PurAvg == true)
                    actionType += "PurAvg";
                else if (filter.SalpurAvg == true)
                    actionType += "Salepuravg";
                else if (filter.MarketAvg == true)
                    actionType += "MarketPrice";
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(actionType))
                actionType = "Salepuravg";

            return actionType;
        }
    }
}
