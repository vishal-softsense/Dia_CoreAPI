using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Classes.Jewellery.JewelleryReports
{
    public class JewelleryPurchaseReportRepository : IJewelleryPurchaseReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryPurchaseReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<JewelleryPurchaseReportRowDto>> GetReportAsync(
            JewelleryPurchaseReportFilterDto filter,
            string companyId)
        {
            var where = BuildPurchaseWhere(filter);
            var sql = $"EXEC [GetJewPurchaseReportData] @Where = '{where}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapPurchaseRow).ToList();
        }

        // Ported from MVC: JewReportController.GenerateJewelleryPurchaseFilter
        private static string BuildPurchaseWhere(JewelleryPurchaseReportFilterDto filter)
        {
            string strWhere = "";

            if (filter == null)
                return strWhere;

            if (!string.IsNullOrWhiteSpace(filter.Party))
                strWhere += " AND Party IN (''" + filter.Party + "'')";

            if (!string.IsNullOrWhiteSpace(filter.Broker))
                strWhere += " AND Broker IN (''" + filter.Broker + "'')";

            if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                strWhere += " AND Date BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

            if (!string.IsNullOrWhiteSpace(filter.Currency))
                strWhere += " AND CurCode IN (''" + filter.Currency + "'')";

            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo;
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                strWhere += " AND Item IN (''" + strTmpNewLine + "'')";
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
                strWhere += " AND invtype IN (''" + filter.Type + "'')";

            if (!string.IsNullOrWhiteSpace(filter.SubType))
                strWhere += " AND SubType IN (''" + filter.SubType + "'')";

            if (!string.IsNullOrWhiteSpace(filter.Category))
                strWhere += " AND Category IN (''" + filter.Category + "'')";

            if (!string.IsNullOrWhiteSpace(filter.BillNo))
                strWhere += " AND BillNo IN (''" + filter.BillNo + "'')";

            if (!string.IsNullOrWhiteSpace(filter.InvNo))
                strWhere += " AND InvNo IN (''" + filter.InvNo + "'')";

            strWhere += " AND trntype = ''PJEW''";

            return strWhere;
        }

        private static JewelleryPurchaseReportRowDto MapPurchaseRow(dynamic row)
        {
            return new JewelleryPurchaseReportRowDto
            {
                Code = GetLong(row, "Code", "code"),
                ItemCode = GetLong(row, "ItemCode", "itemCode"),
                Symbol = GetString(row, "Symbol", "symbol"),
                ConDATE = GetString(row, "ConDATE", "ConDate", "Date", "conDATE", "conDate"),
                Invno = GetString(row, "Invno", "InvNo", "invno", "invNo"),
                BillNo = GetString(row, "BillNo", "Billno", "billNo", "billno"),
                Party = GetString(row, "Party", "party"),
                Item = GetString(row, "Item", "item"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Category = GetString(row, "Category", "category"),
                SubType = GetString(row, "SubType", "subType"),
                IssueCarats = GetString(row, "IssueCarats", "issueCarats"),
                Discount = GetString(row, "Discount", "discount"),
                ReturnCarats = GetString(row, "ReturnCarats", "returnCarats"),
                Pcs = GetString(row, "Pcs", "pcs"),
                rate = GetString(row, "rate", "Rate"),
                amount = GetString(row, "amount", "Amount"),
                Broker = GetString(row, "Broker", "broker"),
                Less1 = GetString(row, "Less1", "less1"),
                Less2 = GetString(row, "Less2", "less2"),
                Less3 = GetString(row, "Less3", "less3"),
                MasterCurrencyRate = GetDouble(row, "MasterCurrencyRate", "masterCurrencyRate"),
                MasterCurrencyAmount = GetDouble(row, "MasterCurrencyAmount", "masterCurrencyAmount"),
                StockCurrencyRate = GetDouble(row, "StockCurrencyRate", "stockCurrencyRate"),
                StockCurrencyAmount = GetDouble(row, "StockCurrencyAmount", "stockCurrencyAmount"),
                ShapeId = GetString(row, "ShapeId", "shapeId"),
                SizeId = GetString(row, "SizeId", "sizeId"),
                ColorId = GetString(row, "ColorId", "colorId"),
                ClarityId = GetString(row, "ClarityId", "clarityId"),
                SieveId = GetString(row, "SieveId", "sieveId"),
                ImagePath = GetString(row, "ImagePath", "imagePath"),
                invtype = GetString(row, "invtype", "Invtype", "InvType", "invType")
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
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out long n)
                ? n
                : 0;
        }

        private static double GetDouble(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out double n)
                ? n
                : 0;
        }
    }
}
