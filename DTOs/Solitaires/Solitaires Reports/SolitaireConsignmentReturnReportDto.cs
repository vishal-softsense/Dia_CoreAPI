using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireConsignmentReturnReportFilterDto
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

    public class SolitaireConsignmentReturnReportRowDto
    {
        public double MemoNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public long Code { get; set; }
        public string? Lotno { get; set; }
        public string? ItemName { get; set; }
        public string? TrnDate { get; set; }
        public string? Invtype { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Return { get; set; }
        public string? ReturnDate { get; set; }
        public double Balance { get; set; }
        public double BalanceAmt { get; set; }
        public string? Broker { get; set; }
        public string? IssUser { get; set; }
        public string? Seller { get; set; }
        public string? RtnUser { get; set; }
        public string? Remarks { get; set; }
        public string? BalanceCarats3Dec { get; set; }
        public double Estimate { get; set; }
        public double MRP { get; set; }
        public int Pcs { get; set; }
        public string? DueDays { get; set; }
        public int RowIndex { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? RoundDate { get; set; }
        public string? ConDATE { get; set; }
        public double DateDiff { get; set; }
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
        public string? SizeId { get; set; }
        public string? SieveId { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? Stock_ID { get; set; }
        public string? MemoTime { get; set; }
        public string? MemoReturnDate { get; set; }
        public string? MemoReturnTime { get; set; }
        public string? DueDate { get; set; }
        public string? StockType { get; set; }
        public double Carats { get; set; }
        public double Rate { get; set; }
        public string? SalesPerson { get; set; }
    }
}
