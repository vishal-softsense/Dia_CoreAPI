using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dia_CoreAPI.DTOs.Dashboard
{
    public class StockAnalysisReportDto
    {
        public string Shape { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Clarity { get; set; }
        public string Lab { get; set; }
        public string Prop { get; set; }
        public string Polish { get; set; }
        public string Symmetry { get; set; }
        public string FL { get; set; }
        public string Kapan { get; set; }
        public string Series { get; set; }
        public string Location { get; set; }
        public string CPS { get; set; }
        public string Aging { get; set; }
        public string StoneAge { get; set; }
        public string PriceRevision { get; set; }
        public string YearMonth { get; set; }
        public string StockID { get; set; }
        public string Sieve { get; set; }
        public string ItemGroup { get; set; }
        public int Pcs { get; set; }
        public float PcsPer { get; set; }
        public double Amount { get; set; }
        public double Carats { get; set; }
        public float CaratsPer { get; set; }
        public float AmountPer { get; set; }
    }

    public class StockAnalysisPivotByFilterDto
    {
        public string Id { get; set; }
        public int GIA_Pcs { get; set; }
        public float GIA_Cts { get; set; }
        public float GIA_Amt { get; set; }
        public float GIA_AvgRate { get; set; }
        public float GIA_AvgPercent { get; set; }
        public int HRD_Pcs { get; set; }
        public float HRD_Cts { get; set; }
        public float HRD_Amt { get; set; }
        public float HRD_AvgRate { get; set; }
        public float HRD_AvgPercent { get; set; }
        public int IGI_Pcs { get; set; }
        public float IGI_Cts { get; set; }
        public float IGI_Amt { get; set; }
        public float IGI_AvgRate { get; set; }
        public float IGI_AvgPercent { get; set; }
        public int CGL_Pcs { get; set; }
        public float CGL_Cts { get; set; }
        public float CGL_Amt { get; set; }
        public float CGL_AvgRate { get; set; }
        public float CGL_AvgPercent { get; set; }
        public int Total_Pcs { get; set; }
        public float Total_Cts { get; set; }
        public float Total_Amt { get; set; }
        public float Total_AvgRate { get; set; }
        public float Total_AvgPercent { get; set; }
    }

    public class StockAnalysisPivotFixedDto
    {
        public string LotNo { get; set; }
        public string Shape { get; set; }
        public float Carats { get; set; }
        public string Color { get; set; }
        public string Clarity { get; set; }
        public string Prop { get; set; }
        public string Pol { get; set; }
        public string Sym { get; set; }
        public string FL { get; set; }
        public float Rapa { get; set; }
        public float Back { get; set; }
        public float Rate { get; set; }
        public float Amount { get; set; }
        public string LAB { get; set; }
        public string Size { get; set; }
        public float MRPAmount { get; set; }
        public float AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float AskingAmount { get; set; }
        public int Pcs { get; set; }
        public float PurChaseAmount { get; set; }
        public string Sieve { get; set; }
    }

#nullable disable
    [ValidateNever]
    public class StockAnalysisRequestDto
    {
        public string Date { get; set; }
        public string Show { get; set; }
        public string HoldUnHold { get; set; }
        public string StoneTypes { get; set; }
        public string Status { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
        public bool ExcludeBoxes { get; set; }
        public List<string> ShapeGroup { get; set; }
        public List<string> Shape { get; set; }
        public List<string> ColorGroup { get; set; }
        public List<string> Color { get; set; }
        public List<string> Size { get; set; }
        public List<string> Clarity { get; set; }
        public List<string> Proportion { get; set; }
        public List<string> Polish { get; set; }
        public List<string> Symmetry { get; set; }

        [JsonPropertyName("lab")]
        public List<string> LAB { get; set; }

        public List<string> Rough { get; set; }
        public List<string> HA { get; set; }
        public List<string> Location { get; set; }
        public List<string> Fluorescence { get; set; }
        public List<string> Milky { get; set; }
        public List<string> Luster { get; set; }
        public List<string> Natts { get; set; }
        public List<string> Shade { get; set; }
        public List<string> FlColor { get; set; }
        public List<string> ItemGroup { get; set; }
        public List<string> Pair { get; set; }
        public List<string> BoxNm { get; set; }
        public float CtsFrm { get; set; }
        public float CtsTo { get; set; }
        public float BackFrm { get; set; }
        public float BackTo { get; set; }
        public float PriceFrm { get; set; }
        public float PriceTo { get; set; }
        public int LotFrm { get; set; }
        public int LotTo { get; set; }
        public float TDepthFrm { get; set; }
        public float TDepthTo { get; set; }
        public float TablePerFrm { get; set; }
        public float TablePerTo { get; set; }
        public float CAFrm { get; set; }
        public float CATo { get; set; }
        public float CHFrm { get; set; }
        public float CHTo { get; set; }
        public float PHFrm { get; set; }
        public float PHTo { get; set; }
        public int PAFrm { get; set; }
        public int PATo { get; set; }
        public bool PurchaseChkBox { get; set; }
        public bool ReviseCheckBox { get; set; }
        public bool NewArrivalChkBox { get; set; }
        public bool WebRevisedChkBox { get; set; }
        public bool UnholdDateChkBox { get; set; }
        public bool LabDateChkBox { get; set; }
        public string FrmPurchaseDate { get; set; }
        public string ToPurchaseDate { get; set; }
        public string FrmReviseDate { get; set; }
        public string ToReviseDate { get; set; }
        public string FrmNewArrivalDate { get; set; }
        public string ToNewArrivalDate { get; set; }
        public string FrmWebRevisedDate { get; set; }
        public string ToWebRevisedDate { get; set; }
        public string FrmUnholdDate { get; set; }
        public string ToUnholdDate { get; set; }
        public string FrmLabDate { get; set; }
        public string ToLabDate { get; set; }
        public string LotNo { get; set; }
        public string FilterSize { get; set; }
        public string Filters { get; set; }
        public string StockAna { get; set; }
        public string GroupBy { get; set; }
    }
#nullable restore
}
