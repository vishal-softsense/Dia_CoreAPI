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
    public class SolitaireSalePerformanceReportRepository : ISolitaireSalePerformanceReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireSalePerformanceReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<SolitaireSalePerformanceReportRowDto>> GetReportAsync(
            SolitaireSalePerformanceReportFilterDto filter,
            string companyId)
        {
            var where = BuildSalePerformanceWhere(filter);
            var stockPrice = string.IsNullOrWhiteSpace(filter?.StockPrices)
                ? "Purchase Price"
                : filter.StockPrices.Trim();

            string sql;
            if (stockPrice == "Base Price" || stockPrice == "Asking Price")
            {
                var baseRate = await GetSBaseRateAsync(companyId);
                // MVC: GetSalePerformanceReporWithBaseRate
                sql = $"EXEC [GetSalePerformanceReportData]@Type = 'SalePerformanceReport', @Where = '{where}',@StockPrice = '{stockPrice}',@BaseRate = '{baseRate}' ";
            }
            else
            {
                // MVC: GetSalePerformanceRepor (Purchase Price)
                sql = $"EXEC [GetSalePerformanceReportData]@Type = 'SalePerformanceReport', @Where = '{where}',@StockPrice = '{stockPrice}' ";
            }

            var rows = await _db.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        // Ported from MVC: ReportController.SalePerformanceReportFilter
        private static string BuildSalePerformanceWhere(SolitaireSalePerformanceReportFilterDto filter)
        {
            string where = "";
            try
            {
                if (filter == null) return where;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    where += " AND Pdate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo;
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(" ", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    where += " AND Lotno IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref where, filter.Shape, "ShapeId");
                AppendInClause(ref where, filter.Color, "ColorId");
                AppendInClause(ref where, filter.Clarity, "ClarityId");
                AppendInClause(ref where, filter.Size, "SizeId");
                AppendInClause(ref where, filter.Proportion, "PropId");
                AppendInClause(ref where, filter.Polish, "PolishId");
                AppendInClause(ref where, filter.Symmetry, "SymId");
                AppendInClause(ref where, filter.LAB, "CertId", extraSpaceBeforeIn: true);

                string? status = filter.Status;
                if (!string.IsNullOrEmpty(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                {
                    if (status == "OnHand")
                        where += " And Status in (''S'') ";
                    else if (status == "OnMemo")
                        where += " And Status in (''M'',''I'',''L'',''PS'') ";
                    else if (status == "OnHand&Int.Memo")
                        where += " And Status in (''S'',''I'') ";
                    else if (status == "OnHand + Memo In")
                        where += " And Status in (''S'',''MI'') ";
                    else if (status == "OnHand + Party Hold")
                        where += " And Status in (''S'',''PH'') ";
                    else if (status == "Memo In")
                        where += " And Status in (''MI'') ";
                    else if (status == "Party Hold")
                        where += " And Status in (''PH'') ";
                    else if (status == "All - Party Hold")
                        where += " And Status not in (''PH'') ";
                }

                string show = filter.Show ?? "All";
                if (!string.Equals(show, "All", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(show, "Sold", StringComparison.OrdinalIgnoreCase))
                    {
                        where += "And Status=''Sold''";
                    }
                    else if (string.Equals(show, "UnSold", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(show, "Unsold", StringComparison.OrdinalIgnoreCase))
                    {
                        // MVC checks "UnSold"; UI option value is "Unsold" — accept both
                        where += " And Status <> ''Sold'' ";
                    }
                }
            }
            catch
            {
                // Swallow like MVC
            }

            return where;
        }

        private static void AppendInClause(
            ref string where,
            List<string>? values,
            string column,
            bool extraSpaceBeforeIn = false)
        {
            if (values == null || values.Count == 0) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");

            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length <= 0) return;

            if (extraSpaceBeforeIn)
                where += " And  " + column + " In (" + joined + ")";
            else
                where += " And " + column + " In (" + joined + ")";
        }

        // MVC page: Session["SBaseRate"] = SqlDataAccess.GetScurBaseRate(DefaultCurrency)
        private async Task<string> GetSBaseRateAsync(string companyId)
        {
            try
            {
                var defaultCur = await _db.ExecuteQuerySingle<string>(
                    "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                    new { },
                    companyId) ?? "";

                if (string.IsNullOrWhiteSpace(defaultCur)) return "0";

                var param = new Dictionary<string, object>
                {
                    { "@Type", "ScurBaseRate" },
                    { "@Where", defaultCur }
                };
                var result = await _db.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
                return string.IsNullOrWhiteSpace(result) ? "0" : result;
            }
            catch
            {
                return "0";
            }
        }

        private static SolitaireSalePerformanceReportRowDto MapReportRow(dynamic row)
        {
            return new SolitaireSalePerformanceReportRowDto
            {
                LotNo = GetString(row, "LotNo", "lotNo"),
                Status = GetString(row, "Status", "status"),
                CertId = GetString(row, "CertId", "certId"),
                ShapeId = GetString(row, "ShapeId", "shapeId"),
                ColorId = GetString(row, "ColorId", "colorId"),
                ClarityId = GetString(row, "ClarityId", "clarityId"),
                PropId = GetString(row, "PropId", "propId"),
                PolishId = GetString(row, "PolishId", "polishId"),
                SymId = GetString(row, "SymId", "symId"),
                PCarats = GetDouble(row, "PCarats", "pCarats"),
                ConsDate = GetString(row, "ConsDate", "consDate"),
                ProcessDays = GetDouble(row, "ProcessDays", "processDays"),
                PDate = GetString(row, "PDate", "pDate"),
                PurPrice = GetDouble(row, "PurPrice", "purPrice"),
                PurDollar = GetDouble(row, "PurDollar", "purDollar"),
                PurBaseRate = GetDouble(row, "PurBaseRate", "purBaseRate"),
                PurRs = GetDouble(row, "PurRs", "purRs"),
                CertExpdollar = GetDouble(row, "CertExpdollar", "certExpdollar"),
                CertExpRs = GetDouble(row, "CertExpRs", "certExpRs"),
                PurCostdollar = GetDouble(row, "PurCostdollar", "purCostdollar"),
                PurCostRs = GetDouble(row, "PurCostRs", "purCostRs"),
                SDueDate = GetString(row, "SDueDate", "sDueDate"),
                SaleDays = GetDouble(row, "SaleDays", "saleDays"),
                SalePrice = GetDouble(row, "SalePrice", "salePrice"),
                Saledollar = GetDouble(row, "Saledollar", "saledollar", "SaleDollar"),
                BaseRate = GetDouble(row, "BaseRate", "baseRate"),
                SaleRs = GetDouble(row, "SaleRs", "saleRs"),
                PLdollar = GetDouble(row, "PLdollar", "pldollar", "PLDollar"),
                PLdollarper = GetDouble(row, "PLdollarper", "pldollarper"),
                PLRs = GetDouble(row, "PLRs", "plRs"),
                PLRsper = GetDouble(row, "PLRsper", "plRsper"),
                Adatdollar = GetDouble(row, "Adatdollar", "adatdollar"),
                AdatRs = GetDouble(row, "AdatRs", "adatRs"),
                Brokeragedollar = GetDouble(row, "Brokeragedollar", "brokeragedollar"),
                BrokerageRs = GetDouble(row, "BrokerageRs", "brokerageRs"),
                TotPurCostdollar = GetDouble(row, "TotPurCostdollar", "totPurCostdollar"),
                FinalPLdollar = GetDouble(row, "FinalPLdollar", "finalPLdollar"),
                FinalPLdollarper = GetDouble(row, "FinalPLdollarper", "finalPLdollarper"),
                TotPurCostRs = GetDouble(row, "TotPurCostRs", "totPurCostRs"),
                FinalPLRs = GetDouble(row, "FinalPLRs", "finalPLRs"),
                FinalPLRsper = GetDouble(row, "FinalPLRsper", "finalPLRsper"),
                Interestdollar = GetDouble(row, "Interestdollar", "interestdollar"),
                TotPurCostdollarWithInt = GetDouble(row, "TotPurCostdollarWithInt", "totPurCostdollarWithInt"),
                FinalPLdollarAfterInt = GetDouble(row, "FinalPLdollarAfterInt", "finalPLdollarAfterInt"),
                InterestRs = GetDouble(row, "InterestRs", "interestRs"),
                TotPurCostRsWithInt = GetDouble(row, "TotPurCostRsWithInt", "totPurCostRsWithInt"),
                FinalPLRsAfterInt = GetDouble(row, "FinalPLRsAfterInt", "finalPLRsAfterInt"),
                Itemcode = GetString(row, "Itemcode", "ItemCode", "itemcode"),
                KapanNo = GetString(row, "KapanNo", "kapanNo"),
                SizeId = GetString(row, "SizeId", "sizeId")
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
            return GetRawValue(row, keys)?.ToString();
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
