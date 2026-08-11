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
    public class JewelleryMemoOutReportRepository : IJewelleryMemoOutReportRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryMemoOutReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<JewelleryMemoOutReportRowDto>> GetReportAsync(
            JewelleryMemoOutReportFilterDto filter,
            string companyId)
        {
            var where = BuildMemoWhere(filter);
            // Ported from MVC: ReportProcessor.JewGetMemoReport
            var sql = $"EXEC [GetJewMemoReportData] @Where = '{where}',@Type = ''";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        // Ported from MVC: JewReportController.JewMemoReportFilter
        private static string BuildMemoWhere(JewelleryMemoOutReportFilterDto filter)
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
                        strWhere += " AND   convert(date,tmtran.MemoReturnDate)  BETWEEN ''" + filter.FromRtnDate + "'' And ''" + filter.ToRtnDate + "''";
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
                    strWhere += "  And (tmtran.pcs - tmtran.spcarats - tmtran.ReturnCarats) <= 0";
                else
                    strWhere += "And (tmtran.pcs - tmtran.spcarats - tmtran.ReturnCarats) > 0";

                strWhere += " and tm.trntype = ''JMemoOut''";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static JewelleryMemoOutReportRowDto MapReportRow(dynamic row)
        {
            return new JewelleryMemoOutReportRowDto
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
                MemoReturnDate = GetString(row, "MemoReturnDate", "memoReturnDate", "ReturnDate", "returnDate"),
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
