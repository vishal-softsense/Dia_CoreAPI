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
    public class JewelleryConsignMentInReportRepository : IJewelleryConsignMentInReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryConsignMentInReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<JewelleryConsignMentInReportDropdownsDto> GetDropdownsAsync(string companyId)
        {
            var purchaseTask = _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);

            var designTask = _db.SelectRawSql<JewDesignMaster>(
                "EXEC [GetALLMasterList] @Type='JewDesign', @Where = ''",
                companyId);

            var categoryTask = _db.SelectRawSql<JewCategoryMaster>(
                "EXEC [GetALLMasterList] @Type='JewCategory', @Where = ''",
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

            await Task.WhenAll(purchaseTask, designTask, categoryTask, subCategoryTask, metalTask, colorTask);

            return new JewelleryConsignMentInReportDropdownsDto
            {
                PurchaseDropdowns = await purchaseTask,
                DesignList = await designTask,
                CategoryList = await categoryTask,
                SubCategoryList = await subCategoryTask,
                MetalList = await metalTask,
                ColorList = await colorTask
            };
        }

        public async Task<IEnumerable<JewelleryConsignMentInReportRowDto>> GetReportAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId)
        {
            var where = BuildWhere(filter);
            // Ported from MVC: ReportProcessor.JewGetMemoReport
            var sql = $"EXEC [GetJewMemoReportData] @Where = '{where}',@Type = ''";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        public async Task<IEnumerable<JewelleryConsignMentInSummaryRowDto>> GetSummaryAsync(
            JewelleryConsignMentInReportFilterDto filter,
            string companyId)
        {
            var where = BuildWhere(filter);
            // Ported from MVC: ReportProcessor.JewConsignmentSummaryReport(..., "Jewconssummary")
            var sql = $"EXEC [GetJewMemoReportData] @Where = '{where}',@Type = 'Jewconssummary'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapSummaryRow).ToList();
        }

        // Ported from MVC: JewReportController.JewConsiReportFilter
        private static string BuildWhere(JewelleryConsignMentInReportFilterDto filter)
        {
            string strWhere = "";

            if (filter == null)
                return strWhere;

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND tm.trndate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (filter.ChkRtnDate)
                {
                    if (!string.IsNullOrWhiteSpace(filter.FromRtnDate) && !string.IsNullOrWhiteSpace(filter.ToRtnDate))
                        strWhere += " AND convert(date,tmtran.MemoReturnDate)  BETWEEN ''" + filter.FromRtnDate + "'' And ''" + filter.ToRtnDate + "''";
                }

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    strWhere += " AND tcur.ID IN (''" + filter.Currency + "'')";

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    strWhere += " AND tm.InvNo IN (''" + filter.InvNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "ALL")
                    strWhere += " AND tm.invtype IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND tparty.AccGrpname IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND tbroker.AccGrpname IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Stock_ID))
                {
                    string strTmpNewLine = filter.Stock_ID;
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    strWhere += " AND titem.Stock_ID IN (''" + strTmpNewLine + "'')";
                }

                if (filter.Clear)
                {
                    // Show clear consignment — no live balance filter (MVC)
                }
                else
                {
                    strWhere += "And (tmtran.pcs - tmtran.spcarats - tmtran.ReturnCarats) > 0";
                }

                if (!string.IsNullOrWhiteSpace(filter.JewDesign))
                    strWhere += " And titem.DesignName In ( ''" + filter.JewDesign + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewCategory))
                    strWhere += " And titem.Category In ( ''" + filter.JewCategory + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewSubCategory))
                    strWhere += " And titem.SubCategory In ( ''" + filter.JewSubCategory + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewCertificate))
                    strWhere += " And titem.JewelleryCertificate In ( ''" + filter.JewCertificate + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewMetalType))
                    strWhere += " And titem.MetalType In ( ''" + filter.JewMetalType + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewColor))
                    strWhere += " And titem.Color In ( ''" + filter.JewColor + "'')";

                if (!string.IsNullOrWhiteSpace(filter.JewPurity))
                    strWhere += " And titem.Purity In ( ''" + filter.JewPurity + "'')";

                strWhere += " and tm.trntype = ''JMemoIn''";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static JewelleryConsignMentInReportRowDto MapReportRow(dynamic row)
        {
            return new JewelleryConsignMentInReportRowDto
            {
                Code = GetLong(row, "Code", "code"),
                ConDATE = GetString(row, "ConDATE", "ConDate", "conDATE", "conDate"),
                DateDiff = GetDouble(row, "DateDiff", "dateDiff"),
                MemoNo = GetString(row, "MemoNo", "memoNo"),
                BillNo = GetString(row, "BillNo", "billNo", "billno"),
                Party = GetString(row, "Party", "party"),
                Stock_ID = GetString(row, "Stock_ID", "Stock_Id", "stock_ID", "stock_id"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Pcs = GetDouble(row, "Pcs", "pcs", "Carats", "carats"),
                Price = GetDouble(row, "Price", "price"),
                Amount = GetDouble(row, "Amount", "amount"),
                Return = GetDouble(row, "Return", "return"),
                MemoReturnDate = GetString(row, "MemoReturnDate", "memoReturnDate", "ReturnDate"),
                Balance = GetDouble(row, "Balance", "balance"),
                BalanceAmt = GetDouble(row, "BalanceAmt", "balanceAmt"),
                Broker = GetString(row, "Broker", "broker"),
                IssUser = GetString(row, "IssUser", "issUser"),
                Symbol = GetString(row, "Symbol", "symbol"),
                CertId = GetString(row, "CertId", "certId", "Cert", "cert"),
                Invtype = GetString(row, "Invtype", "InvType", "invtype"),
                MemoTime = GetString(row, "MemoTime", "memoTime"),
                MemoReturnTime = GetString(row, "MemoReturnTime", "memoReturnTime"),
                ItemCode = GetInt(row, "ItemCode", "itemCode"),
                ImagePath = GetString(row, "ImagePath", "imagePath")
            };
        }

        private static JewelleryConsignMentInSummaryRowDto MapSummaryRow(dynamic row)
        {
            return new JewelleryConsignMentInSummaryRowDto
            {
                TotalCarats = GetDouble(row, "TotalCarats", "totalCarats"),
                ReturnCarats = GetDouble(row, "ReturnCarats", "returnCarats"),
                PurchasedCarats = GetDouble(row, "PurchasedCarats", "purchasedCarats"),
                BalanceCarats = GetDouble(row, "BalanceCarats", "balanceCarats")
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
