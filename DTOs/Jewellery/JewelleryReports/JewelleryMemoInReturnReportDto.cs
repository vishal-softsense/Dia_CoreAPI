namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewelleryMemoInReturnReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
    }

    public class JewelleryMemoInReturnReportRowDto
    {
        public long Code { get; set; }
        public string? TrnDate { get; set; }
        public string? ConDATE { get; set; }
        public string? MemoNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? Lotno { get; set; }
        public string? Stock_ID { get; set; }
        public string? ItemName { get; set; }
        public double Return { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string? Broker { get; set; }
        public string? invtype { get; set; }
    }
}
