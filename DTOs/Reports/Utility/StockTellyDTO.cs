using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class StockTellyPageDto
    {
        public List<StockTellyDropdownOptionDto> Shape { get; set; } = new();
        public List<StockTellyDropdownOptionDto> Color { get; set; } = new();
        public List<StockTellyDropdownOptionDto> Size { get; set; } = new();
        public List<StockTellyDropdownOptionDto> Clarity { get; set; } = new();
    }

    public class StockTellyDropdownOptionDto
    {
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class StockTellyFilterDto
    {
        public string? Date { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Clarity { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
    }

    public class StockTellyListDto
    {
        public int ItemCode { get; set; }
        public string LotNo { get; set; } = string.Empty;
        public int Pcs { get; set; }
        public double carats { get; set; }
        public double OnHand { get; set; }
        public double Rate { get; set; }
        public double Amount { get; set; }
    }

    public class StockTellyImportDto
    {
        public IFormFile? ImportFile { get; set; }
    }

    public class StocktellyEntryDto
    {
        public int ItemCode { get; set; }
        public string? Fromlotno { get; set; }
        public string? Tolotno { get; set; }
        public int Pcs { get; set; }
        public double Carats { get; set; }
        public double Rate { get; set; }
        public double Amount { get; set; }
        public double FromPrice { get; set; }
        public double ToPrice { get; set; }
        public DateTime TrnDate { get; set; }
        public int CompanyID { get; set; }
    }

    public class StockTellySaveRequestDto
    {
        public List<StocktellyEntryDto>? spList { get; set; }
        public List<StocktellyEntryDto>? spList1 { get; set; }
    }
}
