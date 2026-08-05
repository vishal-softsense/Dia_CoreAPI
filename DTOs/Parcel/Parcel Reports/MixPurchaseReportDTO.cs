using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs
{
    public class MixPurchaseReportFilterDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
        public List<string> Shape { get; set; } = new List<string>();
        public List<string> Color { get; set; } = new List<string>();
        public List<string> Clarity { get; set; } = new List<string>();
        public List<string> Symmetry { get; set; } = new List<string>();
        public List<string> Sieve { get; set; } = new List<string>();
    }

    public class MixPurchaseReportDropdownsDTO
    {
        public List<BasicDataDto> Shape { get; set; } = new List<BasicDataDto>();
        public List<BasicDataDto> Size { get; set; } = new List<BasicDataDto>();
        public List<BasicDataDto> Color { get; set; } = new List<BasicDataDto>();
        public List<BasicDataDto> Clarity { get; set; } = new List<BasicDataDto>();
        public List<BasicDataDto> Sieve { get; set; } = new List<BasicDataDto>();
        public List<BasicDataDto> Currency { get; set; } = new List<BasicDataDto>();
    }

    public class MixPurchaseReportResultDTO
    {
        public string? Code { get; set; }
        public string? ConDATE { get; set; }
        public string? Invno { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? StockType { get; set; }
        public string? Stock_Id { get; set; }
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
    }
}