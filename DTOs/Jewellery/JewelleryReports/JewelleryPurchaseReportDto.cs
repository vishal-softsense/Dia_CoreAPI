namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewelleryPurchaseReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
    }

    public class JewelleryPurchaseReportRowDto
    {
        public long Code { get; set; }
        public long ItemCode { get; set; }
        public string? Symbol { get; set; }
        public string? ConDATE { get; set; }
        public string? Invno { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? Item { get; set; }
        public string? ItemName { get; set; }
        public string? Category { get; set; }
        public string? SubType { get; set; }
        public string? IssueCarats { get; set; }
        public string? Discount { get; set; }
        public string? ReturnCarats { get; set; }
        public string? Pcs { get; set; }
        public string? rate { get; set; }
        public string? amount { get; set; }
        public string? Broker { get; set; }
        public string? Less1 { get; set; }
        public string? Less2 { get; set; }
        public string? Less3 { get; set; }
        public double MasterCurrencyRate { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyRate { get; set; }
        public double StockCurrencyAmount { get; set; }
        public string? ShapeId { get; set; }
        public string? SizeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? SieveId { get; set; }
        public string? ImagePath { get; set; }
        public string? invtype { get; set; }
    }
}
