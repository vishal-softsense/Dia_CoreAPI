using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Financial_Reports
{
    public class InvoiceLedgerReportRepository : IInvoiceLedgerReportRepository
    {
        private readonly ISqlDataAccess _db;

        public InvoiceLedgerReportRepository(ISqlDataAccess db)
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

        /// <summary>
        /// Matches MVC ReportProcessor.GetInvoiceLedgerReport → EXEC [GetInvoiceLedgerReport]
        /// </summary>
        public async Task<IEnumerable<InvoiceLedgerReportDTO>> GetInvoiceLedgerReportAsync(
            string where,
            string where2,
            string date,
            string calculationDays,
            string calculationPer,
            string companyId)
        {
            string sql =
                "EXEC [GetInvoiceLedgerReport] @Where = '" + (where ?? string.Empty) +
                "' ,  @Where2 = '" + (where2 ?? string.Empty) +
                "' , @Date = '" + (date ?? string.Empty) +
                "', @CalculationDays = '" + (calculationDays ?? string.Empty) +
                "', @CalculationPer = '" + (calculationPer ?? string.Empty) + "'";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapRow).ToList();
        }

        private static InvoiceLedgerReportDTO MapRow(dynamic row)
        {
            return new InvoiceLedgerReportDTO
            {
                Code = GetString(row, "Code", "code"),
                InvNo = GetString(row, "InvNo", "invNo"),
                BillNo = GetString(row, "BillNo", "billNo"),
                TrnType = GetString(row, "TrnType", "trnType"),
                InvType = GetString(row, "InvType", "invType"),
                Category = GetString(row, "Category", "category"),
                TrnDate = GetDateTime(row, "TrnDate", "trnDate"),
                DueDate = GetDateTime(row, "DueDate", "dueDate"),
                PartyCode = GetString(row, "PartyCode", "partyCode"),
                PartyName = GetString(row, "PartyName", "partyName"),
                BrokerCode = GetString(row, "BrokerCode", "brokerCode"),
                BrokerName = GetString(row, "BrokerName", "brokerName"),
                TotCts = GetNullableDecimal(row, "TotCts", "totCts"),
                StockCurrencyFinalAmount = GetNullableDecimal(row, "StockCurrencyFinalAmount", "stockCurrencyFinalAmount"),
                MasterCurrencyFinalAmount = GetNullableDecimal(row, "MasterCurrencyFinalAmount", "masterCurrencyFinalAmount"),
                StockCurrencyRemainAmount = GetNullableDecimal(row, "StockCurrencyRemainAmount", "stockCurrencyRemainAmount"),
                MasterCurrencyRemainAmount = GetNullableDecimal(row, "MasterCurrencyRemainAmount", "masterCurrencyRemainAmount"),
                PayRecCode = GetString(row, "PayRecCode", "payRecCode"),
                PayRecDate = GetDateTime(row, "PayRecDate", "payRecDate"),
                PayRecStockCurrencyAmount = GetNullableDecimal(row, "PayRecStockCurrencyAmount", "payRecStockCurrencyAmount"),
                PayRecMasterCurrencyAmount = GetNullableDecimal(row, "PayRecMasterCurrencyAmount", "payRecMasterCurrencyAmount"),
                PayRecDays = GetNullableInt(row, "PayRecDays", "payRecDays"),
                StockCurrencyInterest = GetNullableDecimal(row, "StockCurrencyInterest", "stockCurrencyInterest"),
                MasterCurrencyInterest = GetNullableDecimal(row, "MasterCurrencyInterest", "masterCurrencyInterest"),
                ModeOfPayment = GetString(row, "ModeOfPayment", "modeOfPayment")
            };
        }

        private static object? GetRawValue(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k =>
                        string.Equals((k ?? string.Empty).Trim(), key, StringComparison.OrdinalIgnoreCase));
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

        private static DateTime? GetDateTime(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return null;
            if (v is DateTime dt) return dt;
            return DateTime.TryParse(
                Convert.ToString(v, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed)
                ? parsed
                : null;
        }

        private static int? GetNullableInt(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return null;
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out int n)
                ? n
                : null;
        }

        private static decimal? GetNullableDecimal(dynamic row, params string[] keys)
        {
            var v = GetRawValue(row, keys);
            if (v == null) return null;
            return decimal.TryParse(
                Convert.ToString(v, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal n)
                ? n
                : null;
        }
    }
}
