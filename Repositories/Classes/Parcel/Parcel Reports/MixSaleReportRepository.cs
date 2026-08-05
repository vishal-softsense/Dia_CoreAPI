using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Reports
{
    public class MixSaleReportRepository : IMixSaleReportRepository
    {
        private readonly ISqlDataAccess _db;

        public MixSaleReportRepository(ISqlDataAccess db)
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
        public Task<IEnumerable<BasicDataDto>> GetSaleReportDropDownAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }
        #endregion

        

        #region Report Data
        public async Task<IEnumerable<MixSaleReportResultDTO>> GetGeneralSaleParcelReportAsync(
            string whereClause,
            string companyId)
        {
            var sql = $"EXEC [GetGeneralSaleReportData] @where = '{whereClause}'";
            var rows = await _db.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }
        #endregion

        #region Mapping Helpers
        private static MixSaleReportResultDTO MapReportRow(dynamic row)
        {
            return new MixSaleReportResultDTO
            {
                Code = GetString(row, "Code", "code"),
                ConDATE = GetString(row, "ConDATE", "conDATE", "Date", "date"),
                Invno = GetString(row, "InvNo", "invNo", "Invno"),
                BillNo = GetString(row, "BillNo", "billNo", "billno"),
                Party = GetString(row, "Party", "party"),
                Item = GetString(row, "Item", "item"),
                IssueCarats = GetDecimal(row, "IssueCarats", "issueCarats"),
                Discount = GetDecimal(row, "Discount", "discount"),
                ReturnCarats = GetDecimal(row, "ReturnCarats", "returnCarats"),
                Carats = GetDecimal(row, "Carats", "carats"),
                Rate = GetDecimal(row, "Rate", "rate"),
                Amount = GetDecimal(row, "Amount", "amount"),
                Broker = GetString(row, "Broker", "broker"),
                Less1 = GetDecimal(row, "Less1", "less1"),
                Less2 = GetDecimal(row, "Less2", "less2"),
                Less3 = GetDecimal(row, "Less3", "less3"),
                MasterCurrencyAmount = GetDecimal(row, "MasterCurrencyAmount", "masterCurrencyAmount"),
                StockCurrencyAmount = GetDecimal(row, "StockCurrencyAmount", "stockCurrencyAmount"),
                CurCode = GetLong(row, "CurCode", "curCode"),
                StockType = GetString(row, "StockType", "stockType"),
                Stock_Id = GetString(row, "Stock_ID", "stock_ID", "Stock_Id"),
                ShapeId = GetString(row, "Shape", "shape", "ShapeId", "shapeId"),
                ColorId = GetString(row, "Color", "color", "ColorId", "colorId"),
                ClarityId = GetString(row, "Clarity", "clarity", "ClarityId", "clarityId"),
                SymId = GetString(row, "SymId", "symId", "Symmetry"),
                SizeId = GetString(row, "Size", "size", "SizeId", "sizeId"),
                SieveId = GetString(row, "Sieve", "sieve", "SieveId", "sieveId"),
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

        private static long? GetLong(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return null;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out long result) ? result : null;
        }

        private static decimal? GetDecimal(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return null;
            return decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal n) ? n : null;
        }
        #endregion
    }
}