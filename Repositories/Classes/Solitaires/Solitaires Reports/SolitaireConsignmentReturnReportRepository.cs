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
    public class SolitaireConsignmentReturnReportRepository : ISolitaireConsignmentReturnReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireConsignmentReturnReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<SolitaireConsignmentReturnReportRowDto>> GetReportAsync(
            SolitaireConsignmentReturnReportFilterDto filter,
            string companyId)
        {
            var where = BuildConsignmentReturnWhere(filter);
            var sql = $"EXEC [GetSoliMemoInRtnReportData] @Where = '{where}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapConsignmentReturnRow).ToList();
        }

        private static string BuildConsignmentReturnWhere(SolitaireConsignmentReturnReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (filter == null) return strWhere;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND A.SPdate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    strWhere += " AND B.CurCode IN (''" + filter.Currency + "'')";

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    strWhere += " AND CM.InvNo IN (''" + filter.InvNo + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Type) && !string.Equals(filter.Type, "ALL", StringComparison.OrdinalIgnoreCase))
                    strWhere += " AND A.invtype IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    strWhere += " AND CMParty.AccGrpname IN (''" + filter.Party + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Broker))
                    strWhere += " AND tbroker.AccGrpname IN (''" + filter.Broker + "'')";

                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Trim();
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",")
                                                 .Replace("\n", ",")
                                                 .Replace(" ", ",")
                                                 .Replace("\r", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    strWhere += " AND D.LOTNO IN (''" + strTmpNewLine + "'') ";
                }

                strWhere += " and A.trntype = ''MemoIn'' and A.ReturnCarats > 0 ";
            }
            catch
            {
                // MVC swallows exceptions and returns partial where.
            }

            return strWhere;
        }

        private static SolitaireConsignmentReturnReportRowDto MapConsignmentReturnRow(dynamic row)
        {
            return new SolitaireConsignmentReturnReportRowDto
            {
                MemoNo = GetDouble(row, "MemoNo", "memono"),
                BillNo = GetString(row, "BillNo", "billno", "billNo"),
                Party = GetString(row, "Party", "party"),
                Code = GetLong(row, "Code", "code"),
                Lotno = GetString(row, "Lotno", "lotno", "LotNo", "lotNo"),
                ItemName = GetString(row, "ItemName", "itemname", "itemName"),
                TrnDate = GetString(row, "TrnDate", "trndate", "trnDate"),
                Invtype = GetString(row, "Invtype", "invtype", "invType"),
                Discount = GetDouble(row, "Discount", "discount"),
                Price = GetDouble(row, "Price", "price"),
                Amount = GetDouble(row, "Amount", "amount"),
                Return = GetDouble(row, "Return", "return"),
                ReturnDate = GetString(row, "ReturnDate", "returndate"),
                Balance = GetDouble(row, "Balance", "balance"),
                BalanceAmt = GetDouble(row, "BalanceAmt", "balanceamt"),
                Broker = GetString(row, "Broker", "broker"),
                IssUser = GetString(row, "IssUser", "issuser"),
                Seller = GetString(row, "Seller", "seller"),
                RtnUser = GetString(row, "RtnUser", "rtnuser"),
                Remarks = GetString(row, "Remarks", "remarks"),
                BalanceCarats3Dec = GetString(row, "BalanceCarats3Dec", "balancecarats3dec"),
                Estimate = GetDouble(row, "Estimate", "estimate"),
                MRP = GetDouble(row, "MRP", "mrp"),
                Pcs = GetInt(row, "Pcs", "pcs"),
                DueDays = GetString(row, "DueDays", "duedays"),
                RowIndex = GetInt(row, "RowIndex", "rowindex"),
                AddressLine1 = GetString(row, "AddressLine1", "addressline1"),
                AddressLine2 = GetString(row, "AddressLine2", "addressline2"),
                AddressLine3 = GetString(row, "AddressLine3", "addressline3"),
                RoundDate = GetString(row, "RoundDate", "rounddate"),
                ConDATE = GetString(row, "ConDATE", "condate", "conDate"),
                DateDiff = GetDouble(row, "DateDiff", "datediff"),
                Symbol = GetString(row, "Symbol", "symbol"),
                ShapeId = GetString(row, "ShapeId", "shapeid", "shapeId"),
                ColorId = GetString(row, "ColorId", "colorid", "colorId"),
                ClarityId = GetString(row, "ClarityId", "clarityid", "clarityId"),
                PropId = GetString(row, "PropId", "propid", "propId"),
                PolishId = GetString(row, "PolishId", "polishid", "polishId"),
                SymId = GetString(row, "SymId", "symid", "symId"),
                FLId = GetString(row, "FLId", "flid", "flId"),
                CertId = GetString(row, "CertId", "certid"),
                CertificateNo = GetString(row, "CertificateNo", "certificateno", "certificateNo"),
                SizeId = GetString(row, "SizeId", "sizeid", "sizeId"),
                SieveId = GetString(row, "SieveId", "sieveid"),
                SubType = GetString(row, "SubType", "subtype"),
                Category = GetString(row, "Category", "category"),
                Stock_ID = GetString(row, "Stock_ID", "stock_id", "Stock_Id"),
                MemoTime = GetString(row, "MemoTime", "memotime"),
                MemoReturnDate = GetString(row, "MemoReturnDate", "memoreturndate"),
                MemoReturnTime = GetString(row, "MemoReturnTime", "memoreturntime"),
                DueDate = GetString(row, "DueDate", "duedate", "dueDate"),
                StockType = GetString(row, "StockType", "stocktype"),
                Carats = GetDouble(row, "Carats", "carats"),
                Rate = GetDouble(row, "Rate", "rate"),
                SalesPerson = GetString(row, "SalesPerson", "salesperson")
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
