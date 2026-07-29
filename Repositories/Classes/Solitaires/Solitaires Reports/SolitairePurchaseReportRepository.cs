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
    public class SolitairePurchaseReportRepository : ISolitairePurchaseReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitairePurchaseReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<SolitairePurchaseReportRowDto>> GetReportAsync(
            SolitairePurchaseReportFilterDto filter,
            string companyId)
        {
            var where = BuildPurchaseWhere(filter);
            var sql = $"EXEC [GetPurchaseReportData] @Where = '{where}'";
            var rows = await _db.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapPurchaseRow).ToList();
        }

        // Ported from MVC: ReportController.PurchaseReportFilter
        private static string BuildPurchaseWhere(SolitairePurchaseReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (filter == null) return strWhere;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND Date BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    strWhere += " AND CurCode IN (''" + filter.Currency + "'')";

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    strWhere += " AND InvNo IN (''" + filter.InvNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.BillNo))
                    strWhere += " AND BillNo IN (''" + filter.BillNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    strWhere += " AND invType IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.SubType))
                    strWhere += " AND SubType IN (''" + filter.SubType + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Category))
                    strWhere += " AND Category IN (''" + filter.Category + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND Party IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND Broker IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.ExpParty))
                    strWhere += " AND ExpParty IN (''" + filter.ExpParty + "'')";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", "");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    // MVC: AND Item IN (''<comma-separated values>'')
                    strWhere += " AND Item IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref strWhere, filter.Shape, "ShapeId");
                AppendInClause(ref strWhere, filter.Color, "ColorId");
                AppendInClause(ref strWhere, filter.Clarity, "ClarityId");
                AppendInClause(ref strWhere, filter.Size, "SizeId");
                AppendInClause(ref strWhere, filter.Proportion, "PropId");
                AppendInClause(ref strWhere, filter.Polish, "PolishId");
                AppendInClause(ref strWhere, filter.Symmetry, "SymId");

                AppendInClause(ref strWhere, filter.LAB, "Cert", isCert:true);
                AppendInClause(ref strWhere, filter.Fluorescence, "FLId");
                AppendInClause(ref strWhere, filter.Milky, "MilkyId");
                AppendInClause(ref strWhere, filter.Luster, "LusterId", lusterHasExtraSpace: true);
                AppendInClause(ref strWhere, filter.Natts, "NattsId");
                AppendInClause(ref strWhere, filter.Mines, "Minesid");
                AppendInClause(ref strWhere, filter.ProductType, "ProductType");
                AppendInClause(ref strWhere, filter.Sieve, "Sieve");

                bool saleRtn = filter.SaleRtn;
                if (saleRtn)
                    strWhere += " and trntype = ''SaleRtn''";
                else
                    strWhere += " and trntype = ''PUR''";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static void AppendInClause(
            ref string strWhere,
            List<string>? values,
            string column,
            bool isCert = false,
            bool lusterHasExtraSpace = false)
        {
            if (values == null || values.Count == 0) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");

            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length <= 0) return;

            // Keep MVC-ish formatting where it matters (e.g., Luster has a slightly different spacing).
            if (column == "Cert" || isCert)
                strWhere += " And Cert In (" + joined + ")";
            else if (column == "LusterId" && lusterHasExtraSpace)
                strWhere += " And LusterId In ( " + joined + ")";
            else
                strWhere += " And " + column + " In (" + joined + ")";
        }

        private static SolitairePurchaseReportRowDto MapPurchaseRow(dynamic row)
        {
            return new SolitairePurchaseReportRowDto
            {
                Code = GetLong(row, "Code", "code"),
                Invno = GetLong(row, "Invno", "InvNo", "invno", "invNo"),
                BillNo = GetString(row, "BillNo", "Billno", "billNo", "billno"),
                TrnType = GetString(row, "TrnType", "trntype"),
                Invtype = GetString(row, "Invtype", "InvType", "invtype"),
                ConDATE = GetString(row, "ConDATE", "ConDate", "Date", "conDATE", "conDate"),
                Due = GetInt(row, "Due", "due"),
                Party = GetString(row, "Party", "party"),
                Commission = GetString(row, "Commission", "CommAgent", "commAgent"),
                Purchaser = GetString(row, "Purchaser", "purchaser"),
                CurCode = GetLong(row, "CurCode", "curcode"),
                Broker = GetString(row, "Broker", "broker"),
                Expparty = GetString(row, "Expparty", "ExpParty", "expParty"),
                Category = GetString(row, "Category", "category"),
                Item = GetString(row, "Item", "item"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Carats = GetDouble(row, "Carats", "carats"),
                MRP = GetDouble(row, "MRP", "mrp"),
                Rate = GetDouble(row, "Rate", "rate"),
                Discount = GetDouble(row, "Discount", "discount"),
                PropId = GetString(row, "PropId", "propId"),
                PolishId = GetString(row, "PolishId", "polishId"),
                SymId = GetString(row, "SymId", "symId"),
                FLId = GetString(row, "FLId", "flId"),
                ColCla = GetString(row, "ColCla", "colCla"),
                MilkyId = GetString(row, "MilkyId", "milkyId"),
                LusterId = GetString(row, "LusterId", "lusterId"),
                RoughId = GetString(row, "RoughId", "roughId"),
                Symbol = GetString(row, "Symbol", "symbol"),
                DRate = GetDouble(row, "DRate", "dRate"),
                Less1 = GetDouble(row, "Less1", "less1"),
                Less2 = GetDouble(row, "Less2", "less2"),
                Less3 = GetDouble(row, "Less3", "less3"),
                AdatPer = GetDouble(row, "AdatPer", "adatPer"),
                Brokerper = GetDouble(row, "Brokerper", "brokerper"),
                Amount = GetDouble(row, "Amount", "amount"),
                MasterCurrencyRate = GetDouble(row, "MasterCurrencyRate", "masterCurrencyRate"),
                MasterCurrencyAmount = GetDouble(row, "MasterCurrencyAmount", "masterCurrencyAmount"),
                StockCurrencyRate = GetDouble(row, "StockCurrencyRate", "stockCurrencyRate"),
                StockCurrencyAmount = GetDouble(row, "StockCurrencyAmount", "stockCurrencyAmount"),
                Cert = GetString(row, "Cert", "cert"),
                CertificateNo = GetString(row, "CertificateNo", "certificateNo"),
                Carats2Dec = GetDouble(row, "Carats2Dec", "carats2Dec"),
                ShapeId = GetString(row, "ShapeId", "shapeId"),
                ColorId = GetString(row, "ColorId", "colorId"),
                SizeId = GetString(row, "SizeId", "sizeId"),
                ClarityId = GetString(row, "ClarityId", "clarityId"),
                User = GetString(row, "User", "user"),
                ExportNo = GetString(row, "ExportNo", "exportNo"),
                SinglePcs = GetString(row, "SinglePcs", "singlePcs"),
                MRPAmt = GetString(row, "MRPAmt", "mrpAmt")
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

        private static int GetInt(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            int n;
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n)
                ? n
                : 0;
        }

        private static long GetLong(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            long n;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n)
                ? n
                : 0;
        }

        private static double GetDouble(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            double n;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n)
                ? n
                : 0;
        }
    }
}
