using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireConsumptionReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? FromDateDue { get; set; }
        public string? ToDateDue { get; set; }
        public string? ToDate { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Symmetry { get; set; }
        public string? LotNo { get; set; }
        public bool FromDueChkBox { get; set; }
    }

    public class SolitaireConsumptionReportRowDto
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? Shp { get; set; }
        public string? Col { get; set; }
        public string? Clarity { get; set; }
        public string? StatusCategory { get; set; }

        public int Hold_Pcs { get; set; }
        public int Memo_Pcs { get; set; }
        public int Stock_Pcs { get; set; }
        public int Sold_Pcs { get; set; }

        public float Hold_Carats { get; set; }
        public float Memo_Carats { get; set; }
        public float Stock_Carats { get; set; }
        public float Sold_Carats { get; set; }

        public float Hold_AvgDiscPercent { get; set; }
        public float Memo_AvgDiscPercent { get; set; }
        public float Stock_AvgDiscPercent { get; set; }
        public float Sold_AvgDiscPercent { get; set; }

        public float Hold_Amt { get; set; }
        public float Memo_Amt { get; set; }
        public float Stock_Amt { get; set; }
        public float Sold_Amt { get; set; }

        public float Hold_Rapa { get; set; }
        public float Memo_Rapa { get; set; }
        public float Stock_Rapa { get; set; }
        public float Sold_Rapa { get; set; }
    }
}
