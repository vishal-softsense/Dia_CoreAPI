using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Reports
{
    public class MixPurRtnReportDropdownsDTO
    {
        public List<BasicDataDto> Shape { get; set; } = new();
        public List<BasicDataDto> Size { get; set; } = new();
        public List<BasicDataDto> Color { get; set; } = new();
        public List<BasicDataDto> Clarity { get; set; } = new();
        public List<BasicDataDto> Sieve { get; set; } = new();
        public List<BasicDataDto> Currency { get; set; } = new();
    }

    public class MixPurRtnReportFilterDTO
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? Sieve { get; set; }
    }

    public class MixPurRtnReportResultDTO
    {
        public long Code { get; set; }
        public string? ConDATE { get; set; }
        public string? Invno { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? Item { get; set; }
        public decimal? IssueCarats { get; set; }
        public decimal? Discount { get; set; }
        public decimal? ReturnCarats { get; set; }
        public decimal? Carats { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public string? Broker { get; set; }
        public decimal? Less1 { get; set; }
        public decimal? Less2 { get; set; }
        public decimal? Less3 { get; set; }
        public decimal? MasterCurrencyAmount { get; set; }
        public decimal? StockCurrencyAmount { get; set; }
        public string? ShapeId { get; set; }
        public string? SizeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? SieveId { get; set; }
        public string? Lotno { get; set; }
        public string? StockType { get; set; }
    }
}