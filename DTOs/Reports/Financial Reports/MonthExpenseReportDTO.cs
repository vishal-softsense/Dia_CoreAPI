using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    /// <summary>
    /// Matches MVC MonthExpenseData from AccReportModel / GetMonthExpenseReport.
    /// </summary>
    public class MonthExpenseReportDTO
    {
        [JsonPropertyName("AccGrpCode")]
        public int AccGrpCode { get; set; }

        [JsonPropertyName("AccGrpName")]
        public string? AccGrpName { get; set; }

        [JsonPropertyName("AccEffCode")]
        public string? AccEffCode { get; set; }

        [JsonPropertyName("AccTypeDisc")]
        public string? AccTypeDisc { get; set; }

        [JsonPropertyName("ExpYear")]
        public int ExpYear { get; set; }

        [JsonPropertyName("ExpMonth")]
        public int ExpMonth { get; set; }

        [JsonPropertyName("ExpMonthName")]
        public string? ExpMonthName { get; set; }

        [JsonPropertyName("StockCurrencyAmount")]
        public decimal StockCurrencyAmount { get; set; }

        [JsonPropertyName("MasterCurrencyAmount")]
        public decimal MasterCurrencyAmount { get; set; }
    }
}
