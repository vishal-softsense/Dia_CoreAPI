using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Payable_Receivables
{
    public class OutstandingReportRepository : IOutstandingReportRepository
    {
        private readonly ISqlDataAccess _db;

        public OutstandingReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

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

        public Task<IEnumerable<BasicDataDto>> GetDropdownDataAsync(string companyId)
        {
            return _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<List<OutstandingReportResultDto>> GetOutstandingDataAsync(
            string where,
            string curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string dispCurrency,
            string companyId)
        {
            string sql =
                @"Exec [GetOutstandingReportData] @Type = 'Outstanding',@Where = '" + where
                + "', @CurCode = '" + curCode
                + "',@SCurCode = '" + sCurCode
                + "',@MCurCode = '" + mCurCode
                + "',@MCurRate = '" + mCurRate
                + "',@CurMasterOperator = '" + curMasterOperator
                + "',@DisplayCur = '" + dispCurrency + "'";

            // Dynamic map (MVC OutstandingReportModel) — ensures SalesPerson and alt column names bind
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapOutstandingRow).ToList();
        }

        public async Task<List<OutstandingReportResultDto>> GetOutstandingSummDataAsync(
            string where,
            string curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string companyId)
        {
            string sql =
                @"Exec [GetOutstandingReportData] @Type = 'OutstandingSumm',@Where = '" + where
                + "', @CurCode = '" + curCode
                + "',@SCurCode = '" + sCurCode
                + "',@MCurCode = '" + mCurCode
                + "',@MCurRate = '" + mCurRate
                + "',@CurMasterOperator = '" + curMasterOperator + "'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapOutstandingRow).ToList();
        }

        public async Task<long> GetCurCodeAsync(string where, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='ID',@Where = '" + where + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return long.TryParse(result, out long id) ? id : 0;
        }

        public async Task<string> GetMCurOperatorAsync(string curCode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='MCurOperator',@Where = ' and ID=" + curCode + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return result ?? string.Empty;
        }

        public async Task<double> GetMCurRateAsync(string curCode, string companyId)
        {
            string sql = " EXEC [GetCurrencyDetail] @Type='McurBaseRate',@Where = '" + curCode + "' ";
            var result = await _db.SelectRawSqlSingle<string>(sql, companyId);
            return double.TryParse(result, out double rate) ? rate : 0.0;
        }

        public async Task<List<SalesPersonOptionDto>> GetSalesPersonsAsync(string companyId)
        {
            string sql = @"Exec [GetALLMasterList] @Type = 'ShowSalesPerson',@Where = 'and 1=1'";
            var result = await _db.SelectRawSql<SalesPersonOptionDto>(sql, companyId);
            return result?
                .Where(x => !string.IsNullOrWhiteSpace(x.SalesPerson))
                .GroupBy(x => x.SalesPerson.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new SalesPersonOptionDto { SalesPerson = g.First().SalesPerson.Trim() })
                .OrderBy(x => x.SalesPerson)
                .ToList()
                ?? new List<SalesPersonOptionDto>();
        }

        /// <summary>Maps GetOutstandingReportData row → MVC OutstandingReportModel fields.</summary>
        private static OutstandingReportResultDto MapOutstandingRow(dynamic row)
        {
            return new OutstandingReportResultDto
            {
                Code = GetLong(row, "Code", "code"),
                Date = GetString(row, "Date", "date"),
                DueDate = GetString(row, "DueDate", "dueDate"),
                InvNo = GetString(row, "InvNo", "invNo"),
                BillNo = GetString(row, "BillNo", "billNo"),
                Category = GetString(row, "Category", "category"),
                Party = GetString(row, "Party", "party"),
                Adat = GetString(row, "Adat", "adat"),
                Weight = GetFloat(row, "Weight", "weight"),
                Amount = GetString(row, "Amount", "amount"),
                TotINR = GetFloat(row, "TotINR", "totINR"),
                PaidINR = GetFloat(row, "PaidINR", "paidINR"),
                ProductType = GetString(row, "ProductType", "productType"),
                Pcs = GetFloat(row, "Pcs", "pcs"),
                InvoiceNo = GetString(row, "InvoiceNo", "invoiceNo"),
                InvType = GetString(row, "InvType", "invType", "SellerInvType"),
                DueDays = GetString(row, "DueDays", "dueDays"),
                Totcts = GetString(row, "Totcts", "totcts", "TotCts", "Weight"),
                PartyMobile = GetString(row, "PartyMobile", "partyMobile"),
                Broker = GetString(row, "Broker", "broker"),
                BrokerMobile = GetString(row, "BrokerMobile", "brokerMobile"),
                PaidAmount = GetFloat(row, "PaidAmount", "paidAmount"),
                ExDiff = GetFloat(row, "ExDiff", "exDiff"),
                RemainAmount = GetFloat(row, "RemainAmount", "remainAmount"),
                PostDateAmount = GetFloat(row, "PostDateAmount", "postDateAmount"),
                ExportAmount = GetFloat(row, "ExportAmount", "exportAmount"),
                Seller = GetString(row, "Seller", "seller"),
                SellerInvType = GetString(row, "SellerInvType", "sellerInvType"),
                Symbol = GetString(row, "Symbol", "symbol"),
                Disc = GetString(row, "Disc", "disc"),
                SellerPartyMobile = GetString(row, "SellerPartyMobile", "sellerPartyMobile"),
                Rate = GetString(row, "Rate", "rate"),
                Dollarrate = GetString(row, "Dollarrate", "dollarrate", "DollarRate"),
                Remarks = GetString(row, "Remarks", "remarks"),
                UserName = GetString(row, "UserName", "userName"),
                ExpPartyName = GetString(row, "ExpPartyName", "expPartyName", "ExpParty"),
                AddressLine1 = GetString(row, "AddressLine1", "addressLine1"),
                AddressLine2 = GetString(row, "AddressLine2", "addressLine2"),
                AddressLine3 = GetString(row, "AddressLine3", "addressLine3"),
                TrnType = GetString(row, "TrnType", "trnType"),
                // MVC OutstandingReportModel.SalesPerson — SP may return SalesPerson / SalesPersons / KAM
                SalesPerson = GetString(row, "SalesPerson", "salesPerson", "SalesPersons", "salesPersons", "KAM", "Kam", "Sales Person")
            };
        }

        private static object? GetRaw(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
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
            var raw = GetRaw(row, keys);
            if (raw == null) return null;
            var text = Convert.ToString(raw, CultureInfo.InvariantCulture);
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        private static long GetLong(dynamic row, params string[] keys)
        {
            var raw = GetRaw(row, keys);
            if (raw == null) return 0;
            return long.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out long n)
                ? n
                : 0;
        }

        private static float GetFloat(dynamic row, params string[] keys)
        {
            var raw = GetRaw(row, keys);
            if (raw == null) return 0f;
            return float.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out float n)
                ? n
                : 0f;
        }

        private class PermissionCheck
        {
            public int InsertData { get; set; }
            public int EditData { get; set; }
            public int DeleteData { get; set; }
            public int NavigateData { get; set; }
            public int SelectData { get; set; }
        }
    }
}
