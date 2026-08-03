namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Reports
{
    public class ParcelStockSearchReportFilterDto
    {
        public string? Date { get; set; }
        public string? Show { get; set; }
        public string? HoldUnHold { get; set; }
        public string? StoneTypes { get; set; }
        public string? Status { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
        public bool ExcludeBoxes { get; set; }
        public List<string>? ShapeGroup { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? ColorGroup { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Rough { get; set; }
        public List<string>? HA { get; set; }
        public List<string>? Location { get; set; }
        public List<string>? Fluorescence { get; set; }
        public List<string>? Milky { get; set; }
        public List<string>? Luster { get; set; }
        public List<string>? Natts { get; set; }
        public List<string>? Shade { get; set; }
        public List<string>? FlColor { get; set; }
        public List<string>? ItemGroup { get; set; }
        public List<string>? Pair { get; set; }
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
        public string? LotNo { get; set; }
        public string? PrintName { get; set; }
        public string? FilterSize { get; set; }
        public List<string>? StoneNo { get; set; }
        public string? Filters { get; set; }
        public string? StockAna { get; set; }
        public string? SieveId { get; set; }
        public List<string>? Sieve { get; set; }
        public List<string>? BoxNm { get; set; }
        public List<string>? Currency { get; set; }
        public List<string>? City { get; set; }
        public List<string>? State { get; set; }
        public List<string>? Country { get; set; }
        public List<string>? BoxName { get; set; }
        public string? GroupBy { get; set; }
    }

    public class ParcelStockrpt
    {
        public string? CurStatus { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? Shape { get; set; }
        public string? carats { get; set; }
        public string? OnHand { get; set; }
        public string? MemoOut { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Rate { get; set; }
        public string? Amount { get; set; }
        public string? CNo { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? BoxName { get; set; }
        public string? ItemGroupId { get; set; }
        public string? Country { get; set; }
        public int Pcs { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float PurChaseAmount { get; set; }
        public string? Sieve { get; set; }
        public string? MemoPartyName { get; set; }
        public string? HoldPartyName { get; set; }
        public string? SalePartyName { get; set; }
        public string? SRate { get; set; }
        public int ItemCode { get; set; }
    }

    public class ParcelStockLocationUpdateDTO
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public List<string> LotNumbers { get; set; } = new();
    }

    public class ParcelStockBoxGroupUpdateDTO
    {
        public string? BoxName { get; set; }
        public string? Group { get; set; }
        public List<string> LotNumbers { get; set; } = new();
    }
}
