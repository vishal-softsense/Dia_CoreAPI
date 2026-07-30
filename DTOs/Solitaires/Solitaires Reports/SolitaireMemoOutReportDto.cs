using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireMemoOutReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? Seller { get; set; }
        public bool ChkRtnDate { get; set; }
        public string? FromRtnDate { get; set; }
        public string? ToRtnDate { get; set; }
        public bool Clear { get; set; }
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
    }

    public class SolitaireMemoOutReportRowDto
    {
        public long Code { get; set; }
        public string? ConDATE { get; set; }
        public double DateDiff { get; set; }
        public string? MemoNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? Lotno { get; set; }
        public string? ItemName { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Return { get; set; }
        public string? MemoReturnDate { get; set; }
        public double Balance { get; set; }
        public double BalanceAmt { get; set; }
        public string? Broker { get; set; }
        public double Carats { get; set; }
        public string? IssUser { get; set; }
        public string? Seller { get; set; }
        public string? RtnUser { get; set; }
        public string? Remarks { get; set; }
        public string? BalanceCarats3Dec { get; set; }
        public double Estimate { get; set; }
        public double MRP { get; set; }
        public string? Symbol { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? PropId { get; set; }
        public string? PolishId { get; set; }
        public string? SymId { get; set; }
        public string? FLId { get; set; }
        public string? CertId { get; set; }
        public string? CertificateNo { get; set; }
        public string? Invtype { get; set; }
        public string? MemoTime { get; set; }
        public string? MemoReturnTime { get; set; }
    }
}
