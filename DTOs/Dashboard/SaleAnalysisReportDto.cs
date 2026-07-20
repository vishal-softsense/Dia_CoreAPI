using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dia_CoreAPI.DTOs.Dashboard
{
    /// <summary>
    /// Matches MVC ShapeAnaDataModel (StockSearch.cs) — column names must map to
    /// GetSaleAnalysisData SP output.
    /// </summary>
    public class SaleAnalysisReportDto
    {
        // Grouping dimension fields (each SP returns one of these as the label column)
        public string Shape { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Clarity { get; set; }
        public string Sieve { get; set; }
        public string Lab { get; set; }
        public string Prop { get; set; }
        public string Polish { get; set; }
        public string Symmertry { get; set; }
        public string FL { get; set; }
        public string Kapan { get; set; }
        public string Series { get; set; }
        public string Aging { get; set; }
        public string Location { get; set; }
        public string PartyLocation { get; set; }

        [JsonPropertyName("cps")]
        public string CPS { get; set; }

        public string PriceRevision { get; set; }
        public string YearMonth { get; set; }
        public string StockID { get; set; }
        public string Month { get; set; }

        // Measure columns (mirrors ShapeAnaDataModel in StockSearch.cs exactly)
        public int Pcs { get; set; }
        public float PcsPer { get; set; }
        public double Amount { get; set; }
        public double Carats { get; set; }
        public float Caratsper { get; set; }
        public float AmountPer { get; set; }
        public float SaleAmtdollar { get; set; }
        public float Saleperdollar { get; set; }
        public float PLdollar { get; set; }
        public float DollarPLper { get; set; }
        public float PurCostdollar { get; set; }
        public float PLperOnPurAmtdollar { get; set; }
        public float SaleAmtINR { get; set; }
        public float Saleper { get; set; }
        public float PLINR { get; set; }

        [JsonPropertyName("inrPLper")]
        public float INRPLper { get; set; }

        public float PurCostINR { get; set; }
        public float PLperOnPurAmtINR { get; set; }
        public float COGSdollar { get; set; }
        public float COGSINR { get; set; }
        public float PLperOnCOGSAmtdollar { get; set; }
        public float PLperOnCOGSAmtINR { get; set; }
    }

    /// <summary>
    /// Matches MVC SaleRptFilter (SaleReportModel.cs) for the /Group-by/sale-data endpoint.
    /// </summary>
#nullable disable
    [ValidateNever]
    public class SaleAnalysisRequestDto
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Currency { get; set; }
        public string InvNo { get; set; }
        public string BillNo { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Category { get; set; }
        public string Party { get; set; }
        public string ExpParty { get; set; }
        public string Broker { get; set; }
        public bool PurRtn { get; set; }
        public string LotNo { get; set; }

        public List<string> Shape { get; set; }
        public List<string> Color { get; set; }
        public List<string> Clarity { get; set; }
        public List<string> Size { get; set; }
        public List<string> Proportion { get; set; }
        public List<string> Polish { get; set; }
        public List<string> Symmetry { get; set; }

        // React/JSON sends "lab"
        [JsonPropertyName("lab")]
        public List<string> LAB { get; set; }

        public List<string> Fluorescence { get; set; }
        public List<string> Milky { get; set; }
        public List<string> Luster { get; set; }
        public List<string> Natts { get; set; }
        public List<string> Mines { get; set; }
        public List<string> ProductType { get; set; }
        public List<string> Sieve { get; set; }

        /// <summary>
        /// Grouping dimension: Lab, Shape, Size, Color, Clarity, Prop, Polish,
        /// Symmertry, CPS, FL, Series, Location, YearMonth, Aging, PriceRevision,
        /// PartyLocation, StockID
        /// </summary>
        public string SaleAna { get; set; }
    }
#nullable restore
}
