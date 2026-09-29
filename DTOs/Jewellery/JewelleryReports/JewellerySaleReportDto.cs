using System.Collections.Generic;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewellerySaleReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? Category { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
        public string? DesignName { get; set; }
        public string? SubType { get; set; }
        public string? SubCategory { get; set; }
        public string? InvNo { get; set; }
        public string? JewelleryCertificate { get; set; }
        public string? MetalType { get; set; }
        public string? JewColor { get; set; }
        public string? JewPurity { get; set; }
        /// <summary>MVC FromRChkBox — when true, filter by RoundDate (Print Date).</summary>
        public bool FromRChkBox { get; set; }
        /// <summary>MVC RDate — Print Date (RoundDate from).</summary>
        public string? RDate { get; set; }
        /// <summary>MVC RToDate — Print To Date (RoundDate to).</summary>
        public string? RToDate { get; set; }
    }

    public class JewellerySaleReportRowDto
    {
        public long Code { get; set; }
        public string? ConDATE { get; set; }
        public string? invtype { get; set; }
        public string? invno { get; set; }
        public string? DesignName { get; set; }
        public string? BillNo { get; set; }
        public string? TrnType { get; set; }
        public string? SubCategory { get; set; }
        public string? JewelleryCertificate { get; set; }
        public string? MetalType { get; set; }
        public string? Color { get; set; }
        public string? Purity { get; set; }
        public int Due { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? Category { get; set; }
        public string? StockItem { get; set; }
        public string? ItemName { get; set; }
        public string? SubType { get; set; }
        public double carats { get; set; }
        public double rate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public double amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyRate { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyRate { get; set; }
        public double StockCurrencyAmount { get; set; }
    }

    public class JewellerySaleReportDropdownsDto
    {
        public IEnumerable<Dia_CoreAPI.DTOs.StockAccount.BasicDataDto> PurchaseDropdowns { get; set; } = new List<Dia_CoreAPI.DTOs.StockAccount.BasicDataDto>();
        public IEnumerable<JewDesignMaster> DesignList { get; set; } = new List<JewDesignMaster>();
        public IEnumerable<JewSubCategoryMaster> SubCategoryList { get; set; } = new List<JewSubCategoryMaster>();
        public IEnumerable<JewMetalMaster> MetalList { get; set; } = new List<JewMetalMaster>();
        public IEnumerable<JewColorMaster> ColorList { get; set; } = new List<JewColorMaster>();
    }
}
