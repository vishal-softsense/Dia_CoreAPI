using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Reports
{
    public class MixConsignmentInRtnReportRepository : IMixConsignmentInRtnReportRepository
    {
        private readonly ISqlDataAccess _db;

        public MixConsignmentInRtnReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        #region Permission
        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _db.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }
        #endregion

        #region Dropdowns
        public Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }
        #endregion

        
        #region Report Data
        public async Task<IEnumerable<MixConsignmentInRtnReportResultDTO>> GetMixMemoInRtnReportAsync(
            string whereClause,
            string companyId)
        {
            var sql = $"EXEC [GetMixMemoInRtnReportData] @Where = '{whereClause}'";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }
        #endregion

        #region Mapping Helpers
        private static MixConsignmentInRtnReportResultDTO MapReportRow(dynamic row)
        {
            return new MixConsignmentInRtnReportResultDTO
            {
                RowIndex = GetInt(row, "RowIndex", "rowIndex"),
                RoundDate = GetString(row, "RoundDate", "roundDate"),
                ConDATE = GetString(row, "ConDATE", "conDATE", "Date", "date"),
                DateDiff = GetFloat(row, "DateDiff", "dateDiff"),
                MemoNo = GetFloat(row, "MemoNo", "memoNo"),
                BillNo = GetString(row, "BillNo", "billNo"),
                Party = GetString(row, "Party", "party"),
                DueDays = GetString(row, "DueDays", "dueDays"),
                AddressLine1 = GetString(row, "AddressLine1", "addressLine1"),
                AddressLine2 = GetString(row, "AddressLine2", "addressLine2"),
                AddressLine3 = GetString(row, "AddressLine3", "addressLine3"),
                Code = GetLong(row, "Code", "code"),
                Pcs = GetInt(row, "Pcs", "pcs"),
                Lotno = GetString(row, "Lotno", "lotno", "LOTNO"),
                ItemName = GetString(row, "ItemName", "itemName"),
                Discount = GetFloat(row, "Discount", "discount"),
                Price = GetFloat(row, "Price", "price"),
                Amount = GetFloat(row, "Amount", "amount"),
                Return = GetFloat(row, "Return", "return"),
                ReturnDate = GetString(row, "ReturnDate", "returnDate"),
                Balance = GetFloat(row, "Balance", "balance"),
                BalanceAmt = GetFloat(row, "BalanceAmt", "balanceAmt"),
                Broker = GetString(row, "Broker", "broker"),
                IssUser = GetString(row, "IssUser", "issUser"),
                Seller = GetString(row, "Seller", "seller"),
                RtnUser = GetString(row, "RtnUser", "rtnUser"),
                Remarks = GetString(row, "Remarks", "remarks"),
                BalanceCarats3Dec = GetString(row, "BalanceCarats3Dec", "balanceCarats3Dec"),
                Estimate = GetFloat(row, "Estimate", "estimate"),
                MRP = GetFloat(row, "MRP", "mrp"),
                Symbol = GetString(row, "Symbol", "symbol"),
                ShapeId = GetString(row, "ShapeId", "shapeId"),
                ColorId = GetString(row, "ColorId", "colorId"),
                ClarityId = GetString(row, "ClarityId", "clarityId"),
                PropId = GetString(row, "PropId", "propId"),
                PolishId = GetString(row, "PolishId", "polishId"),
                SymId = GetString(row, "SymId", "symId"),
                FLId = GetString(row, "FLId", "flId"),
                CertId = GetString(row, "CertId", "certId"),
                CertificateNo = GetString(row, "CertificateNo", "certificateNo"),
                Invtype = GetString(row, "invtype", "Invtype", "InvType", "invType"),
                MemoTime = GetString(row, "MemoTime", "memoTime"),
                MemoReturnDate = GetString(row, "MemoReturnDate", "memoReturnDate"),
                MemoReturnTime = GetString(row, "MemoReturnTime", "memoReturnTime"),
                SizeId = GetString(row, "SizeId", "sizeId"),
                SieveId = GetString(row, "SieveId", "sieveId", "sieveid"),
                TrnDate = GetString(row, "TrnDate", "trnDate"),
                DueDate = GetString(row, "DueDate", "dueDate"),
                SubType = GetString(row, "SubType", "subType"),
                Category = GetString(row, "Category", "category"),
                Stock_ID = GetString(row, "Stock_ID", "stock_ID", "stock_id"),
                StockType = GetString(row, "StockType", "stockType"),
                Carats = GetFloat(row, "Carats", "carats"),
                Rate = GetFloat(row, "Rate", "rate"),
                SalesPerson = GetString(row, "SalesPerson", "salesPerson")
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
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out int result) ? result : 0;
        }

        private static long GetLong(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out long result) ? result : 0;
        }

        private static float GetFloat(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return 0f;
            return float.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out float n) ? n : 0f;
        }
        #endregion
    }
}