using System;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    /// <summary>
    /// Matches MVC TrialBlFilter from AccReportModel.
    /// </summary>
    public class TrialBalanceSheetReportFilterDTO
    {
        public DateTime ToDate { get; set; }
        public int Curcode { get; set; }
    }

    /// <summary>
    /// Matches MVC LedgerDetail from AccReportModel / GetTrialBalanceSheet.
    /// </summary>
    public class TrialBalanceSheetReportDTO
    {
        [JsonPropertyName("Path")]
        public string? Path { get; set; }

        [JsonPropertyName("Path2")]
        public string? Path2 { get; set; }

        [JsonPropertyName("MainAccountName")]
        public string? MainAccountName { get; set; }

        [JsonPropertyName("Debit")]
        public decimal Debit { get; set; }

        [JsonPropertyName("Credit")]
        public decimal Credit { get; set; }

        [JsonPropertyName("TotDebit")]
        public decimal TotDebit { get; set; }

        [JsonPropertyName("TotCredit")]
        public decimal TotCredit { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; }

        [JsonPropertyName("AccGrpCode")]
        public string? AccGrpCode { get; set; }

        [JsonPropertyName("MDebit")]
        public decimal MDebit { get; set; }

        [JsonPropertyName("MCredit")]
        public decimal MCredit { get; set; }

        [JsonPropertyName("Party")]
        public string? Party { get; set; }
    }
}
