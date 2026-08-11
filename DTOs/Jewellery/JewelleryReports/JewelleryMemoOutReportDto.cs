namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewelleryMemoOutReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FromRtnDate { get; set; }
        public string? ToRtnDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public bool ChkRtnDate { get; set; }
        public bool Clear { get; set; }
        public string? Stock_ID { get; set; }
    }

    public class JewelleryMemoOutReportRowDto
    {
        public long Code { get; set; }
        public string? ConDATE { get; set; }
        public double DateDiff { get; set; }
        public string? MemoNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? Stock_ID { get; set; }
        public string? ItemName { get; set; }
        public double Pcs { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Return { get; set; }
        public string? MemoReturnDate { get; set; }
        public double Balance { get; set; }
        public double BalanceAmt { get; set; }
        public string? Broker { get; set; }
        public string? IssUser { get; set; }
        public string? Symbol { get; set; }
        public string? CertId { get; set; }
        public string? Invtype { get; set; }
        public string? MemoTime { get; set; }
        public string? MemoReturnTime { get; set; }
        public int ItemCode { get; set; }
        public string? ImagePath { get; set; }
    }
}
