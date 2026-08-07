using System;
using System.Collections.Generic;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Reports
{
    public class MixConsignmentInRtnReportDropdownsDTO
    {
        public List<BasicDataDto> Currency { get; set; } = new();
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> Shape { get; set; } = new();
        public List<BasicDataDto> Color { get; set; } = new();
        public List<BasicDataDto> Size { get; set; } = new();
        public List<BasicDataDto> Clarity { get; set; } = new();
        public List<BasicDataDto> LAB { get; set; } = new();
        public List<BasicDataDto> CPS { get; set; } = new();
        public List<BasicDataDto> HA { get; set; } = new();
        public List<BasicDataDto> Proportion { get; set; } = new();
        public List<BasicDataDto> Polish { get; set; } = new();
        public List<BasicDataDto> Symmetry { get; set; } = new();
        public List<BasicDataDto> Fluorescence { get; set; } = new();
        public List<BasicDataDto> FlColor { get; set; } = new();
        public List<BasicDataDto> Brown { get; set; } = new();
        public List<BasicDataDto> Green { get; set; } = new();
        public List<BasicDataDto> Milky { get; set; } = new();
        public List<BasicDataDto> Luster { get; set; } = new();
        public List<BasicDataDto> Natts { get; set; } = new();
        public List<BasicDataDto> Shade { get; set; } = new();
        public List<BasicDataDto> Rough { get; set; } = new();
        public List<BasicDataDto> Mines { get; set; } = new();
        public List<BasicDataDto> ProductType { get; set; } = new();
        public List<BasicDataDto> Sieve { get; set; } = new();
    }

    public class MixConsignmentInRtnReportFilterDTO
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; } = "ALL";
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
    }

    public class MixConsignmentInRtnReportResultDTO
    {
        public int RowIndex { get; set; }
        public string? RoundDate { get; set; }
        public string? ConDATE { get; set; }
        public float DateDiff { get; set; }
        public float MemoNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? DueDays { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public long Code { get; set; }
        public int Pcs { get; set; }
        public string? Lotno { get; set; }
        public string? ItemName { get; set; }
        public float Discount { get; set; }
        public float Price { get; set; }
        public float Amount { get; set; }
        public float Return { get; set; }
        public string? ReturnDate { get; set; }
        public float Balance { get; set; }
        public float BalanceAmt { get; set; }
        public string? Broker { get; set; }
        public string? IssUser { get; set; }
        public string? Seller { get; set; }
        public string? RtnUser { get; set; }
        public string? Remarks { get; set; }
        public string? BalanceCarats3Dec { get; set; }
        public float Estimate { get; set; }
        public float MRP { get; set; }
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
        public string? MemoReturnDate { get; set; }
        public string? MemoReturnTime { get; set; }
        public string? SizeId { get; set; }
        public string? SieveId { get; set; }
        public string? TrnDate { get; set; }
        public string? DueDate { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? Stock_ID { get; set; }
        public string? StockType { get; set; }
        public float Carats { get; set; }
        public float Rate { get; set; }
        public string? SalesPerson { get; set; }
    }
}