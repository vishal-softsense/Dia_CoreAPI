using System;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    /// <summary>
    /// Matches MVC BalSheetFilter from AccReportModel.
    /// </summary>
    public class BalanceSheetReportFilterDTO
    {
        public DateTime ToDate { get; set; }
        public int Curcode { get; set; }
        public string? CalType { get; set; }
    }

    /// <summary>
    /// Matches MVC BLSheetDetail from AccReportModel / SPReportBL.
    /// </summary>
    public class BalanceSheetReportDTO
    {
        [JsonPropertyName("LAccGrpName")]
        public string? LAccGrpName { get; set; }

        [JsonPropertyName("LMBalAmt")]
        public decimal LMBalAmt { get; set; }

        [JsonPropertyName("LSBalAmt")]
        public decimal LSBalAmt { get; set; }

        [JsonPropertyName("AAccGrpName")]
        public string? AAccGrpName { get; set; }

        [JsonPropertyName("AMBalAmt")]
        public decimal AMBalAmt { get; set; }

        [JsonPropertyName("ASBalAmt")]
        public decimal ASBalAmt { get; set; }

        [JsonPropertyName("Party")]
        public string? Party { get; set; }
    }
}
