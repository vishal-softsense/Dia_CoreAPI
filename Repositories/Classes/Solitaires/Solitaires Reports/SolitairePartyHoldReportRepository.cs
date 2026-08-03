using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitairePartyHoldReportRepository : ISolitairePartyHoldReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitairePartyHoldReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<SolitairePartyHoldReportRowDto>> GetReportAsync(
            SolitairePartyHoldReportFilterDto filter,
            string companyId)
        {
            var where = BuildPartyHoldWhere(filter);
            // MVC: GetMemoReport(strWhere, "") => EXEC [GetMemoReportData] @Where, @Type=''
            var sql = $"EXEC [GetMemoReportData] @Where = '{where}',@Type = ''";
            var rows = await _db.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        // Ported from MVC: ReportController.PartyHoldReportFilter
        private static string BuildPartyHoldWhere(SolitairePartyHoldReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (filter == null) return strWhere;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND tm.trndate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                bool chkRtnDate = filter.ChkRtnDate;
                if (chkRtnDate)
                {
                    if (!string.IsNullOrWhiteSpace(filter.FromRtnDate) && !string.IsNullOrWhiteSpace(filter.ToRtnDate))
                        strWhere += " AND   convert(date,tmtran.MemoReturnDate)  BETWEEN ''" + filter.FromRtnDate + "'' And ''" + filter.ToRtnDate + "''";
                }

                // Currency dropdown value = TabCurrencyMaster.Id (same as Memo/Consignment/Jew reports)
                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    strWhere += " AND tcur.Id IN (''" + filter.Currency + "'')";

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    strWhere += " AND tm.InvNo IN (''" + filter.InvNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "ALL")
                    strWhere += " AND tm.invtype IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND tparty.AccGrpname IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND tbroker.AccGrpname IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", "");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    strWhere += " AND titem.LOTNO IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref strWhere, filter.Shape, "tshp.ShapeId");
                AppendInClause(ref strWhere, filter.Color, "tcol.ColorId");
                AppendInClause(ref strWhere, filter.Clarity, "tcla.ClarityId");
                AppendInClause(ref strWhere, filter.Size, "tsize.SizeId");
                AppendInClause(ref strWhere, filter.Proportion, "PropId");
                AppendInClause(ref strWhere, filter.Polish, "PolishId");
                AppendInClause(ref strWhere, filter.Symmetry, "SymId");
                AppendInClause(ref strWhere, filter.LAB, "Cert");
                AppendInClause(ref strWhere, filter.Fluorescence, "FLId");
                AppendInClause(ref strWhere, filter.Milky, "MilkyId");
                AppendInClause(ref strWhere, filter.Luster, "LusterId", lusterHasExtraSpace: true);
                AppendInClause(ref strWhere, filter.Natts, "NattsId");
                AppendInClause(ref strWhere, filter.Mines, "Minesid");
                AppendInClause(ref strWhere, filter.ProductType, "ProductType");
                AppendInClause(ref strWhere, filter.Sieve, "Sieve");

                // MVC: only apply balance filter when NOT filtering by return date
                if (!chkRtnDate)
                {
                    if (filter.Clear)
                        strWhere += " And (tmtran.carats - tmtran.spcarats - tmtran.ReturnCarats) <= 0";
                    else
                        strWhere += " And (tmtran.carats - tmtran.spcarats - tmtran.ReturnCarats) > 0";
                }

                strWhere += " and tm.trntype = ''PartyHold''";
            }
            catch
            {
                // Swallow like MVC does — return partial where
            }

            return strWhere;
        }

        private static void AppendInClause(
            ref string strWhere,
            List<string>? values,
            string column,
            bool lusterHasExtraSpace = false)
        {
            if (values == null || values.Count == 0) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");

            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length <= 0) return;

            if (lusterHasExtraSpace)
                strWhere += " And " + column + " In ( " + joined + ")";
            else
                strWhere += " And " + column + " In (" + joined + ")";
        }

        private static SolitairePartyHoldReportRowDto MapReportRow(dynamic row)
        {
            return new SolitairePartyHoldReportRowDto
            {
                Code = GetLong(row, "Code", "code"),
                ConDATE = GetString(row, "ConDATE", "ConDate", "conDATE"),
                DateDiff = GetDouble(row, "DateDiff", "dateDiff"),
                MemoNo = GetString(row, "MemoNo", "memoNo"),
                BillNo = GetString(row, "BillNo", "billNo", "billno"),
                Party = GetString(row, "Party", "party"),
                Lotno = GetString(row, "Lotno", "LotNo", "lotno"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Discount = GetDouble(row, "Discount", "discount"),
                Price = GetDouble(row, "Price", "price"),
                Amount = GetDouble(row, "Amount", "amount"),
                Return = GetDouble(row, "Return", "return"),
                MemoReturnDate = GetString(row, "MemoReturnDate", "memoReturnDate", "ReturnDate", "returnDate"),
                Balance = GetDouble(row, "Balance", "balance"),
                BalanceAmt = GetDouble(row, "BalanceAmt", "balanceAmt"),
                Broker = GetString(row, "Broker", "broker"),
                Carats = GetDouble(row, "Carats", "carats"),
                IssUser = GetString(row, "IssUser", "issUser"),
                Seller = GetString(row, "Seller", "seller"),
                RtnUser = GetString(row, "RtnUser", "rtnUser"),
                Remarks = GetString(row, "Remarks", "remarks"),
                BalanceCarats3Dec = GetString(row, "BalanceCarats3Dec", "balanceCarats3Dec"),
                Estimate = GetDouble(row, "Estimate", "estimate"),
                MRP = GetDouble(row, "MRP", "mrp"),
                Symbol = GetString(row, "Symbol", "symbol"),
                ShapeId = GetString(row, "ShapeId", "shapeId"),
                ColorId = GetString(row, "ColorId", "colorId"),
                ClarityId = GetString(row, "ClarityId", "clarityId"),
                PropId = GetString(row, "PropId", "propId"),
                PolishId = GetString(row, "PolishId", "polishId"),
                SymId = GetString(row, "SymId", "symId"),
                FLId = GetString(row, "FLId", "flId"),
                CertId = GetString(row, "CertId", "certId", "Cert", "cert"),
                CertificateNo = GetString(row, "CertificateNo", "certificateNo"),
                Invtype = GetString(row, "Invtype", "InvType", "invtype"),
                MemoTime = GetString(row, "MemoTime", "memoTime"),
                MemoReturnTime = GetString(row, "MemoReturnTime", "memoReturnTime")
            };
        }

        private static object? GetRawValue(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        var value = dict[match];
                        if (value != null && value != DBNull.Value) return value;
                    }
                }
            }
            return null;
        }

        private static string? GetString(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            return v?.ToString();
        }

        private static long GetLong(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            long n;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static double GetDouble(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            double n;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0;
        }
    }
}
