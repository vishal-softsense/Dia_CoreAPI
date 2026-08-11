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
    public class JewelleryMemoOutReturnReporteRepository : IJewelleryMemoOutReturnReporteRepository
    {
        private readonly ISqlDataAccess _db;

        public JewelleryMemoOutReturnReporteRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<JewelleryMemoOutReturnReporteRowDto>> GetReportAsync(
            JewelleryMemoOutReturnReporteFilterDto filter,
            string companyId)
        {
            var where = BuildWhere(filter);
            // Ported from MVC: ReportProcessor.GetJewMemoOutRtnReport
            var sql = $"EXEC [GetJewMemoOutRtnReportData] @Where = '{where}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        // Ported from MVC: JewReportController.JewmemoOutRtnReportFilter
        private static string BuildWhere(JewelleryMemoOutReturnReporteFilterDto filter)
        {
            string strWhere = "";

            if (filter == null)
                return strWhere;

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND A.SPdate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    strWhere += " AND B.CurCode IN (''" + filter.Currency + "'')";

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    strWhere += " AND CM.InvNo IN (''" + filter.InvNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "ALL")
                    strWhere += " AND A.invtype IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND CMParty.AccGrpname IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND tbroker.AccGrpname IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo;
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    strWhere += " AND titem.Stock_ID IN (''" + strTmpNewLine + "'')";
                }

                strWhere += " and A.trntype = ''JMemoOut'' and A.ReturnCarats > 0 ";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static JewelleryMemoOutReturnReporteRowDto MapRow(dynamic row)
        {
            return new JewelleryMemoOutReturnReporteRowDto
            {
                Code = GetLong(row, "Code", "code"),
                TrnDate = GetString(row, "TrnDate", "trnDate", "ConDATE", "conDATE"),
                ConDATE = GetString(row, "ConDATE", "conDATE", "TrnDate", "trnDate"),
                MemoNo = GetString(row, "MemoNo", "memoNo"),
                BillNo = GetString(row, "BillNo", "billNo", "billno"),
                Party = GetString(row, "Party", "party"),
                Lotno = GetString(row, "Lotno", "LotNo", "lotno", "Stock_ID", "stock_ID"),
                Stock_ID = GetString(row, "Stock_ID", "stock_ID", "Lotno", "LotNo"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Return = GetDouble(row, "Return", "return"),
                Price = GetDouble(row, "Price", "price", "Rate", "rate"),
                Amount = GetDouble(row, "Amount", "amount"),
                Broker = GetString(row, "Broker", "broker"),
                invtype = GetString(row, "invtype", "Invtype", "InvType")
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
