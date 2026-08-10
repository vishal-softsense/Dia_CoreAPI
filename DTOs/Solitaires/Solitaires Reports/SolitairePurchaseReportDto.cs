using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitairePurchaseReportFilterDto
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
        public string? ExpParty { get; set; }
        public string? Broker { get; set; }
        public bool SaleRtn { get; set; }
        public string? LotNo { get; set; }

        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Fluorescence { get; set; }
        public List<string>? Milky { get; set; }
        public List<string>? Luster { get; set; }
        public List<string>? Natts { get; set; }
        public List<string>? Mines { get; set; }
        public List<string>? ProductType { get; set; }
        public List<string>? Sieve { get; set; }

        public string? PrintName { get; set; }
    }

    public class SolitairePurchaseReportRowDto
    {
        public long Code { get; set; }
        public long Invno { get; set; }
        public string? BillNo { get; set; }
        public string? TrnType { get; set; }
        public string? Invtype { get; set; }
        public string? ConDATE { get; set; }
        public int Due { get; set; }
        public string? Party { get; set; }
        public string? Commission { get; set; }
        public string? Purchaser { get; set; }
        public long CurCode { get; set; }
        public string? Broker { get; set; }
        public string? Expparty { get; set; }
        public string? Category { get; set; }
        public string? Item { get; set; }
        public string? ItemName { get; set; }
        public double Carats { get; set; }
        public double MRP { get; set; }
        public double Rate { get; set; }
        public double Discount { get; set; }
        public string? PropId { get; set; }
        public string? PolishId { get; set; }
        public string? SymId { get; set; }
        public string? FLId { get; set; }
        public string? ColCla { get; set; }
        public string? MilkyId { get; set; }
        public string? LusterId { get; set; }
        public string? RoughId { get; set; }
        public string? Symbol { get; set; }
        public double DRate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public double AdatPer { get; set; }
        public double Brokerper { get; set; }
        public double Amount { get; set; }
        public double MasterCurrencyRate { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyRate { get; set; }
        public double StockCurrencyAmount { get; set; }
        public string? Cert { get; set; }
        public string? CertificateNo { get; set; }
        public double Carats2Dec { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? SizeId { get; set; }
        public string? ClarityId { get; set; }
        public string? User { get; set; }
        public string? ExportNo { get; set; }
        public string? SinglePcs { get; set; }
        public string? MRPAmt { get; set; }
    }
}
