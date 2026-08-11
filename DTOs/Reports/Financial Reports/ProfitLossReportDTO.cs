using System;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    /// <summary>
    /// Matches MVC TradingAcFilter from AccReportModel.
    /// </summary>
    public class ProfitLossReportFilterDTO
    {
        public DateTime ToDate { get; set; }
        public int Curcode { get; set; }
        public string? CalType { get; set; }
    }

    /// <summary>
    /// Matches MVC PLReport from AccReportModel / SPReportTAPL.
    /// </summary>
    public class ProfitLossReportDTO
    {
        [JsonPropertyName("DAccGrpName")]
        public string? DAccGrpName { get; set; }

        [JsonPropertyName("DMBalAmt")]
        public decimal DMBalAmt { get; set; }

        [JsonPropertyName("DSBalAmt")]
        public decimal DSBalAmt { get; set; }

        [JsonPropertyName("CAccGrpName")]
        public string? CAccGrpName { get; set; }

        [JsonPropertyName("CMBalAmt")]
        public decimal CMBalAmt { get; set; }

        [JsonPropertyName("CSBalAmt")]
        public decimal CSBalAmt { get; set; }
    }
}
