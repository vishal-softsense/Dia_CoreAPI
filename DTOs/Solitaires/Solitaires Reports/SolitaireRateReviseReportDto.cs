using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireRateReviseReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? LotNo { get; set; }

        public List<string>? Shape { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Fluorescence { get; set; }
    }

    public class SolitaireRateReviseReportRowDto
    {
        public string? Date { get; set; }
        public string? LotNo { get; set; }
        public decimal? Carats { get; set; }
        public decimal? OldRate { get; set; }
        public decimal? NewRate { get; set; }
        public decimal? OldPer { get; set; }
        public decimal? NewPer { get; set; }
        public decimal? PLAmount { get; set; }
        public string? UserName { get; set; }

        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? SizeId { get; set; }

        public int TotalItems { get; set; }

        // Not shown in the MVC DataTable UI, but returned by the underlying SP.
        public decimal? MSP { get; set; }
        public decimal? Oldback { get; set; }
        public decimal? NewBack { get; set; }
    }
}
