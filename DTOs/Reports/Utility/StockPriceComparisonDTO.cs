using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class StockPriceComparisonFilterDto
    {
        public List<string>? LAB { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? CPS { get; set; }
        public List<string>? Fluorescence { get; set; }
    }

    public class StockPriceComparisonPageDto
    {
        public List<StockPriceComparisonDropdownOptionDto> LAB { get; set; } = new();
        public List<StockPriceComparisonDropdownOptionDto> Shape { get; set; } = new();
        public List<StockPriceComparisonDropdownOptionDto> Size { get; set; } = new();
        public List<StockPriceComparisonDropdownOptionDto> Fluorescence { get; set; } = new();
        public List<StockPriceComparisonDropdownOptionDto> CPS { get; set; } = new();
    }

    public class StockPriceComparisonDropdownOptionDto
    {
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Matches MVC StockPriceComparisonReport (SP_StockPriceComparison Type=Report).</summary>
    public class StockPriceComparisonReportDto
    {
        public string CertId { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Flou { get; set; } = string.Empty;
        public string CPSID { get; set; } = string.Empty;
        public string Clarity { get; set; } = string.Empty;
        public int Pcs { get; set; }
        public decimal DiscPercent { get; set; }
        public decimal FinalMRP { get; set; }
        public string SizeRange { get; set; } = string.Empty;
        public decimal FCarat { get; set; }
        public decimal TCarat { get; set; }
    }

    /// <summary>Kept for file naming consistency with other Utility DTOs.</summary>
    public class StockPriceComparisonDTO
    {
        public string? Message { get; set; }
    }
}
