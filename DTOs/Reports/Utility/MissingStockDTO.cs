using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class MissingStockFilterDto
    {
        public string? StoneTypes { get; set; }
        public string? LotNo { get; set; }
        public string? Filters { get; set; }
        public string? Status { get; set; }
        public string? Date { get; set; }
    }

    /// <summary>Matches MVC Missingstock row returned by Sp_MissingStock.</summary>
    public class MissingStockReportDto
    {
        public string LotNo { get; set; } = string.Empty;
        public string CurStatus { get; set; } = string.Empty;
        public string StoneTypes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Filters { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }

    /// <summary>Kept for file naming consistency with other Utility DTOs.</summary>
    public class MissingStockDTO
    {
        public string? Message { get; set; }
    }
}
