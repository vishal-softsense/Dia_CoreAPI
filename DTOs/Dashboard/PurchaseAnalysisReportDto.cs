using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dia_CoreAPI.DTOs.Dashboard
{
    public class PurchaseAnalysisReportDto
    {
        public string Shape { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Clarity { get; set; }
        public string Sieve { get; set; }

        public string Lab { get; set; }
        public string Prop { get; set; }
        public int PurPcs { get; set; }
        public float PurPcsper { get; set; }
        public double PurCarats { get; set; }
        public float PurCaratsper { get; set; }
        public float PurCostdollar { get; set; }
        public int SoldPcs { get; set; }
        public float SoldPcsper { get; set; }
        public double SoldCarats { get; set; }
        public float SoldCaratsper { get; set; }
        public float SaleAmtdollar { get; set; }
        public float Saleamtdollerper { get; set; }
        public float CostAmtdollar { get; set; }
        public float Salecostamtper { get; set; }
        public float SoldPLdollar { get; set; }
        public float SoldPLdollarper { get; set; }
        public float PLperOnPurAmtdollar { get; set; }
        public float SaleAmtINR { get; set; }
        public float Saleper { get; set; }
        public float SoldPLINR { get; set; }
        [JsonPropertyName("inrPLper")]
        public float INRPLper { get; set; }
        public float PurCostINR { get; set; }
        public float PLperOnPurAmtINR { get; set; }
        public int StockPcs { get; set; }
        public float StockPcsper { get; set; }
        public double StockCarats { get; set; }
        public float StockCaratsper { get; set; }
        public float StockAmtdollar { get; set; }
        public float StockAmtdollarper { get; set; }
        public float TotPLAmtdollar { get; set; }
        public float TotalPLdollarper { get; set; }
        public string Polish { get; set; }
        public string Symmetry { get; set; }
        public string Fluorescence { get; set; }
        [JsonPropertyName("cps")]
        public string CPS { get; set; }
        public string Series { get; set; }
        public string Location { get; set; }
        public string YearMonth { get; set; }
        public string Aging { get; set; }
        public string PriceRevision { get; set; }
        public string PartyLocation { get; set; }
        [JsonPropertyName("stockID")]
        public string StockID { get; set; }

        public string Month { get; set; }

        public int Pcs { get; set; }
        public float PcsPer { get; set; }

        public float Carats { get; set; }
        public float CaratsPer { get; set; }

        public float PurAmtDollar { get; set; }
        public float PurDollarPer { get; set; }

        public float PurAmtInr { get; set; }
        public float PurInrPer { get; set; }

        public float DollarPerCarat { get; set; }
        public float InrPerCarat { get; set; }
        public float BaseRate { get; set; }
    }

#nullable disable
    [ValidateNever]
    public class PurchaseAnalysisRequestDto
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
        public bool SaleRtn { get; set; }
        public string LotNo { get; set; }
        public List<string> Shape { get; set; }
        public List<string> Color { get; set; }
        public List<string> Clarity { get; set; }
        public List<string> Size { get; set; }
        public List<string> Proportion { get; set; }
        public List<string> Polish { get; set; }
        public List<string> Symmetry { get; set; }

        [JsonPropertyName("lab")]
        public List<string> LAB { get; set; }

        public List<string> Fluorescence { get; set; }
        public List<string> Milky { get; set; }
        public List<string> Luster { get; set; }
        public List<string> Natts { get; set; }
        public List<string> Mines { get; set; }
        public List<string> ProductType { get; set; }
        public List<string> Sieve { get; set; }
        public string PrintName { get; set; }
        public string PurAna { get; set; }

        public List<string> ShapeGroup { get; set; }
        public List<string> ColorGroup { get; set; }
        public List<string> Boxnm { get; set; }
        public List<string> ItemGroup { get; set; }
        public List<string> PartyLocation { get; set; }
    }
#nullable restore
}
