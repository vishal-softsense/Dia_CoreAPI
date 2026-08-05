namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireAgingReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? GroupBy { get; set; }
        public string? Party { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
    }

    public class SolitaireAgingReportRowDto
    {
        public string? DocDate { get; set; }
        public int DocId { get; set; }
        public string? AccountName { get; set; }
        public string? DueDate { get; set; }
        public double Amount { get; set; }
        public double RemainingAmount { get; set; }
        public double EarlyPayments1 { get; set; }
        public double EarlyPayments2 { get; set; }
        public double Days1To30 { get; set; }
        public double Days31To60 { get; set; }
        public double Days61To90 { get; set; }
        public double Days91To120 { get; set; }
        public double Days120Plus { get; set; }
        public string? SalesPerson { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
