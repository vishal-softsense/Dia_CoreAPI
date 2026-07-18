namespace Dia_CoreAPI.DTOs.Dashboard
{
    public class MonthAnalysisReportDto
    {
        public string Month { get; set; }
        public string Year { get; set; }
        public string PartyName { get; set; }
        public int? SortKey { get; set; }

        public double? TotalPayable { get; set; }
        public double? TotalReceivable { get; set; }
        
        // Purchase/Sale details
        public int? Pcs { get; set; }
        public decimal? Cts { get; set; }
        public decimal? Amt { get; set; }
        public decimal? Disc { get; set; }
        public decimal? Rate { get; set; }

        public string YearMonth { get; set; }
        public string PurData { get; set; }
        public string SaleData { get; set; }
    }

    public class MonthAnalysisRequestDto
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromDueDate { get; set; }
        public string ToDueDate { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string DispCurrency { get; set; }
        public string Party { get; set; }
        public string Broker { get; set; }
        public string Commission { get; set; }
        public string Category { get; set; }
        public string GroupBy { get; set; }
        public string ReportType { get; set; }
        public bool? FromDueChkBox { get; set; }
    }
}
