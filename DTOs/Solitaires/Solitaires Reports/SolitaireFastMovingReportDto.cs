using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireFastMovingReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Fluorescence { get; set; }
    }

    public class SolitaireFastMovingReportRowDto
    {
        public string? ClarityName { get; set; }
        public string? ColorName { get; set; }

        public double Opening_Cts { get; set; }
        public double Opening_Pcs { get; set; }
        public double Opening_pcsper { get; set; }
        public double Opening_USDperCts { get; set; }
        public double Opening_DiscPct { get; set; }
        public double Opening_Age_Total { get; set; }

        public double Purchase_Cts { get; set; }
        public double Purchase_Pcs { get; set; }
        public double Purchase_pcsper { get; set; }
        public double Purchase_USDperCts { get; set; }
        public double Purchase_DiscPct { get; set; }
        public double Purchase_Age_Total { get; set; }

        public double Sale_Cts { get; set; }
        public double Sale_Pcs { get; set; }
        public double Sale_pcsper { get; set; }
        public double Sale_USDperCts { get; set; }
        public double Sale_DiscPct { get; set; }
        public double Sale_Age_Total { get; set; }

        public double Closing_Cts { get; set; }
        public double Closing_Pcs { get; set; }
        public double Closing_pcsper { get; set; }
        public double Closing_USDperCts { get; set; }
        public double Closing_DiscPct { get; set; }
        public double Closing_Age_Total { get; set; }

        public string? LotNo { get; set; }
        public string? TrnType { get; set; }
        public string? SPdate { get; set; }
        public int pcs { get; set; }
        public double Carats { get; set; }
        public double mrp { get; set; }
        public double Amount { get; set; }
        public double Disc { get; set; }
        public int Age { get; set; }
    }
}
