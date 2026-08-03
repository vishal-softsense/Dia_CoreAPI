namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    // Matches MVC: KapanSummaryFilter
    public class SolitaireKapanSummaryReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? LotNo { get; set; }
        /// <summary>"include" | "exclude" — matches MVC radio kapanFilter</summary>
        public string? FilterType { get; set; }
    }

    // Matches MVC: KapanSummaryReport (SP: sp_KapanSummary)
    public class SolitaireKapanSummaryReportRowDto
    {
        public string? KapanNo { get; set; }

        // Opening + Purchase
        public double OP_Pcs { get; set; }
        public double OP_Carats { get; set; }
        public double OP_Amount { get; set; }
        public double OP_AvgRate { get; set; }
        public double OP_AvgRapa { get; set; }
        public double OP_AvgBack { get; set; }

        // StkTrn + Sale
        public double Sale_Pcs { get; set; }
        public double Sale_Carats { get; set; }
        public double Sale_Amount { get; set; }
        public double Sale_AvgRate { get; set; }
        public double Sale_AvgRapa { get; set; }
        public double Sale_AvgBack { get; set; }

        // Stock
        public double Stock_Pcs { get; set; }
        public double Stock_Carats { get; set; }
        public double Stock_Amount { get; set; }
        public double Stock_AvgRate { get; set; }
        public double Stock_AvgRapa { get; set; }
        public double Stock_AvgBack { get; set; }

        // Memo
        public double MemoOut_Pcs { get; set; }
        public double MemoOut_Carats { get; set; }
        public double MemoOut_Amount { get; set; }
        public double MemoOut_AvgRate { get; set; }
        public double MemoOut_AvgRapa { get; set; }
        public double MemoOut_AvgBack { get; set; }
    }
}
