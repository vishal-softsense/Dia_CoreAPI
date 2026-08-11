using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Classes.Jewellery.JewelleryReports
{
    public class JewellerySaleReturnReportRepository : IJewellerySaleReturnReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewellerySaleReturnReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<JewellerySaleReturnReportDropdownsDto> GetDropdownsAsync(string companyId)
        {
            var purchaseTask = _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);

            var designTask = _db.SelectRawSql<JewDesignMaster>(
                "EXEC [GetALLMasterList] @Type='JewDesign', @Where = ''",
                companyId);

            var subCategoryTask = _db.SelectRawSql<JewSubCategoryMaster>(
                "EXEC [GetALLMasterList] @Type='JewSubCategory', @Where = ''",
                companyId);

            var metalTask = _db.SelectRawSql<JewMetalMaster>(
                "EXEC [GetALLMasterList] @Type='JewMetal', @Where = ''",
                companyId);

            var colorTask = _db.SelectRawSql<JewColorMaster>(
                "EXEC [GetALLMasterList] @Type='JewColor', @Where = ''",
                companyId);

            await Task.WhenAll(purchaseTask, designTask, subCategoryTask, metalTask, colorTask);

            return new JewellerySaleReturnReportDropdownsDto
            {
                PurchaseDropdowns = await purchaseTask,
                DesignList = await designTask,
                SubCategoryList = await subCategoryTask,
                MetalList = await metalTask,
                ColorList = await colorTask
            };
        }

        public async Task<IEnumerable<JewellerySaleReturnReportRowDto>> GetReportAsync(
            JewellerySaleReturnReportFilterDto filter,
            string companyId)
        {
            var where = BuildSaleReturnWhere(filter);
            // Ported from MVC: ReportProcessor.GetJewSaleReturnReport
            var sql = $"EXEC [GetJewSaleReturnReportData] @Where = '{where}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapSaleReturnRow).ToList();
        }

        // Ported from MVC: JewReportController.JewSaleReturnReportFilter
        private static string BuildSaleReturnWhere(JewellerySaleReturnReportFilterDto filter)
        {
            string strWhere = "";

            if (filter == null)
                return strWhere;

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND Date BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    strWhere += " AND invType IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND Party IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Category))
                    strWhere += " AND Category IN (''" + filter.Category + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND Broker IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.DesignName))
                    strWhere += " And DesignName In ( ''" + filter.DesignName + "'')";

                if (!string.IsNullOrWhiteSpace(filter.SubType))
                    strWhere += " AND SubType IN (''" + filter.SubType + "'')";

                if (!string.IsNullOrWhiteSpace(filter.SubCategory))
                    strWhere += " And SubCategory In ( ''" + filter.SubCategory + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewelleryCertificate))
                    strWhere += " And JewelleryCertificate In ( ''" + filter.JewelleryCertificate + "'')";

                if (!string.IsNullOrWhiteSpace(filter.MetalType))
                    strWhere += " And MetalType In ( ''" + filter.MetalType + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewColor))
                    strWhere += " And Color In ( ''" + filter.JewColor + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewPurity))
                    strWhere += " And Purity In ( ''" + filter.JewPurity + "'')";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo;
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    strWhere += " AND (StockItem IN (''" + strTmpNewLine + "''))";
                }

                strWhere += " and trntype = ''SJEWRtn''";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static JewellerySaleReturnReportRowDto MapSaleReturnRow(dynamic row)
        {
            return new JewellerySaleReturnReportRowDto
            {
                Code = GetLong(row, "Code", "code"),
                ConDATE = GetString(row, "ConDATE", "ConDate", "Date", "conDATE", "conDate"),
                invtype = GetString(row, "invtype", "Invtype", "InvType", "invType"),
                invno = GetString(row, "invno", "Invno", "InvNo", "invNo"),
                DesignName = GetString(row, "DesignName", "designName"),
                BillNo = GetString(row, "BillNo", "billNo", "billno"),
                TrnType = GetString(row, "TrnType", "trnType", "trntype"),
                SubCategory = GetString(row, "SubCategory", "subCategory"),
                JewelleryCertificate = GetString(row, "JewelleryCertificate", "jewelleryCertificate"),
                MetalType = GetString(row, "MetalType", "metalType"),
                Color = GetString(row, "Color", "color"),
                Purity = GetString(row, "Purity", "purity"),
                Due = GetInt(row, "Due", "due"),
                Party = GetString(row, "Party", "party"),
                Broker = GetString(row, "Broker", "broker"),
                Category = GetString(row, "Category", "category"),
                StockItem = GetString(row, "StockItem", "stockItem", "Item", "item"),
                ItemName = GetString(row, "ItemName", "itemName"),
                SubType = GetString(row, "SubType", "subType"),
                carats = GetDouble(row, "carats", "Carats", "Pcs", "pcs"),
                rate = GetDouble(row, "rate", "Rate"),
                Less1 = GetDouble(row, "Less1", "less1"),
                Less2 = GetDouble(row, "Less2", "less2"),
                Less3 = GetDouble(row, "Less3", "less3"),
                amount = GetDouble(row, "amount", "Amount"),
                Symbol = GetString(row, "Symbol", "symbol"),
                MasterCurrencyRate = GetDouble(row, "MasterCurrencyRate", "masterCurrencyRate"),
                MasterCurrencyAmount = GetDouble(row, "MasterCurrencyAmount", "masterCurrencyAmount"),
                StockCurrencyRate = GetDouble(row, "StockCurrencyRate", "stockCurrencyRate"),
                StockCurrencyAmount = GetDouble(row, "StockCurrencyAmount", "stockCurrencyAmount")
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
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out int n)
                ? n
                : 0;
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
