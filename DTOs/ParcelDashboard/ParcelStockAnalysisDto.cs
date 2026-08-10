using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.ParcelDashboard
{
    public class ParcelStockAnalysisRequestDto
    {
        public string? Date { get; set; }
        public string? Show { get; set; }
        public string? HoldUnHold { get; set; }
        public string? StoneTypes { get; set; }
        public string? Status { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public string? LotNo { get; set; }
        public List<string>? ShapeGroup { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? ColorGroup { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Sieve { get; set; }
        public List<string>? Location { get; set; }
        public List<string>? ItemGroup { get; set; }
        public List<string>? BoxNm { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? HA { get; set; }
        public List<string>? Fluorescence { get; set; }
        public List<string>? Milky { get; set; }
        public List<string>? Luster { get; set; }
        public List<string>? Natts { get; set; }
        public List<string>? Shade { get; set; }
        public List<string>? FlColor { get; set; }
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
        public float PAFrm { get; set; }
        public float PATo { get; set; }
        public bool PurchaseChkBox { get; set; }
        public bool ReviseCheckBox { get; set; }
        public bool NewArrivalChkBox { get; set; }
        public bool WebRevisedChkBox { get; set; }
        public bool UnholdDateChkBox { get; set; }
        public bool LabDateChkBox { get; set; }
        public string? FrmPurchaseDate { get; set; }
        public string? ToPurchaseDate { get; set; }
        public string? FrmReviseDate { get; set; }
        public string? ToReviseDate { get; set; }
        public string? FrmNewArrivalDate { get; set; }
        public string? ToNewArrivalDate { get; set; }
        public string? FrmWebRevisedDate { get; set; }
        public string? ToWebRevisedDate { get; set; }
        public string? FrmUnholdDate { get; set; }
        public string? ToUnholdDate { get; set; }
        public string? FrmLabDate { get; set; }
        public string? ToLabDate { get; set; }
        public string? StockAna { get; set; }
    }

    public class ParcelChartDataDto
    {
        public string? Shape { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Clarity { get; set; }
        public string? Lab { get; set; }
        public string? Prop { get; set; }
        public string? Polish { get; set; }
        public string? Symmertry { get; set; }
        public string? Symmetry { get; set; }
        public string? FL { get; set; }
        public string? Kapan { get; set; }
        public string? Series { get; set; }
        public string? Location { get; set; }
        public string? CPS { get; set; }
        public string? Aging { get; set; }
        public string? StoneAge { get; set; }
        public string? PriceRevision { get; set; }
        public string? YearMonth { get; set; }
        public int Pcs { get; set; }
        public float PcsPer { get; set; }
        public double Amount { get; set; }
        public double Carats { get; set; }
        public float CaratsPer { get; set; }
        public float AmountPer { get; set; }
        public double? Rate { get; set; }
        public double? Back { get; set; }
        public double? AskingRate { get; set; }
        public double? AskingBack { get; set; }
        public string? StockID { get; set; }
        public string? Sieve { get; set; }
        public string? ItemGroup { get; set; }
    }

    public class ParcelStockPivotDto
    {
        public string? LotNo { get; set; }
        public string? Shape { get; set; }
        public float Carats { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Prop { get; set; }
        public string? Pol { get; set; }
        public string? Sym { get; set; }
        public string? FL { get; set; }
        public float Rapa { get; set; }
        public float Back { get; set; }
        public float Rate { get; set; }
        public float Amount { get; set; }
        public string? LAB { get; set; }
        public string? CNo { get; set; }
        public string? HA { get; set; }
        public string? Nts { get; set; }
        public float MRPAmount { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Location { get; set; }
        public string? Brown { get; set; }
        public string? Milky { get; set; }
        public string? luster { get; set; }
        public string? Shade { get; set; }
        public string? Packet { get; set; }
        public string? Kapan { get; set; }
        public string? Size { get; set; }
        public string? FLColor { get; set; }
        public string? BoxName { get; set; }
        public string? ItemGroupId { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float PurChaseAmount { get; set; }
        public string? CurStatus { get; set; }
        public float AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float AskingAmount { get; set; }
        public int Pcs { get; set; }
        public string? Sieve { get; set; }
    }

    public class ParcelStockPivotRawDto
    {
        public string? LotNo { get; set; }
        public string? Shape { get; set; }
        public string? Shp { get; set; }
        public string? carats { get; set; }
        public string? OnHand { get; set; }
        public string? Color { get; set; }
        public string? Col { get; set; }
        public string? Clarity { get; set; }
        public string? Cla { get; set; }
        public string? Size { get; set; }
        public string? Amount { get; set; }
        public string? Rate { get; set; }
        public string? Back { get; set; }
        public string? Sieve { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? BoxName { get; set; }
        public string? ItemGroupId { get; set; }
        public string? CurStatus { get; set; }
        public string? Prop { get; set; }
        public string? Pol { get; set; }
        public string? Sym { get; set; }
        public string? FL { get; set; }
        public string? LAB { get; set; }
        public string? CNo { get; set; }
        public string? HA { get; set; }
        public string? Nts { get; set; }
        public string? Brown { get; set; }
        public string? Milky { get; set; }
        public string? luster { get; set; }
        public string? Shade { get; set; }
        public string? Packet { get; set; }
        public string? Kapan { get; set; }
        public string? FLColor { get; set; }
        public int Pcs { get; set; }
        public float Rapa { get; set; }
        public float MRPAmount { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float PurChaseAmount { get; set; }
        public float AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float AskingAmount { get; set; }
    }
}
