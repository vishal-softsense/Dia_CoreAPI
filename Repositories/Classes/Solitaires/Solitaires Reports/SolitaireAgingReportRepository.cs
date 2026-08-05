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
    public class SolitaireAgingReportRepository : ISolitaireAgingReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireAgingReportRepository(ISqlDataAccess db)
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

        public async Task<IEnumerable<SolitaireAgingReportRowDto>> GetReportAsync(
            SolitaireAgingReportFilterDto filter,
            string companyId)
        {
            var where = BuildAgingWhere(filter);
            var type = string.Equals(filter?.GroupBy, "InvoiceWise", StringComparison.OrdinalIgnoreCase)
                ? "InvWise"
                : "PartyWise";

            // MVC: ReportProcessor.GetAgingReport => EXEC [SP_AgingReport] @Where, @Type
            var sql = $"EXEC [SP_AgingReport]  @Where = '{where}',@Type ='{type}' ";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        // Ported from MVC: ReportController.AgingReport(AgingFilter)
        private static string BuildAgingWhere(SolitaireAgingReportFilterDto filter)
        {
            var where = "";
            try
            {
                if (filter == null) return where;

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    where += " AND tsp.TrnDate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";

                if (!string.IsNullOrWhiteSpace(filter.Party))
                    where += "and tparty.AccGrpCode = " + filter.Party;

                if (!string.IsNullOrWhiteSpace(filter.InvNo))
                    where += "and tsp.InvNo = " + filter.InvNo;

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    where += " AND tsp.invtype IN (''" + filter.Type + "'')";

                if (!string.IsNullOrWhiteSpace(filter.SubType))
                    where += " AND tsp.SubType IN (''" + filter.SubType + "'')";

                if (!string.IsNullOrWhiteSpace(filter.Category))
                    where += " AND tsp.Category IN (''" + filter.Category + "'')";

                if (!string.IsNullOrWhiteSpace(filter.BillNo))
                    where += " AND tsp.BillNo IN (''" + filter.BillNo + "'')";
            }
            catch
            {
                // Swallow like other migrated reports — return partial where
            }

            return where;
        }

        private static SolitaireAgingReportRowDto MapReportRow(dynamic row)
        {
            return new SolitaireAgingReportRowDto
            {
                DocDate = GetString(row, "DocDate", "docDate"),
                DocId = GetInt(row, "DocId", "docId"),
                AccountName = GetString(row, "AccountName", "accountName"),
                DueDate = GetString(row, "DueDate", "dueDate"),
                Amount = GetDouble(row, "Amount", "amount"),
                RemainingAmount = GetDouble(row, "RemainingAmount", "remainingAmount"),
                EarlyPayments1 = GetDouble(row, "EarlyPayments1", "earlyPayments1"),
                EarlyPayments2 = GetDouble(row, "EarlyPayments2", "earlyPayments2"),
                Days1To30 = GetDouble(row, "Days1To30", "days1To30"),
                Days31To60 = GetDouble(row, "Days31To60", "days31To60"),
                Days61To90 = GetDouble(row, "Days61To90", "days61To90"),
                Days91To120 = GetDouble(row, "Days91To120", "days91To120"),
                Days120Plus = GetDouble(row, "Days120Plus", "days120Plus"),
                SalesPerson = GetString(row, "SalesPerson", "salesPerson"),
                City = GetString(row, "City", "city"),
                Country = GetString(row, "Country", "country")
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
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0;
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
