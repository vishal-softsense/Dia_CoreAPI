using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireAdminStockReportDto
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
        public List<string>? StoneNo { get; set; }
        public List<string>? BoxNm { get; set; }
        public string? Filters { get; set; }
    }

    public class SolitaireAdminStockReportResponseDTO
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? Shape { get; set; }
        public decimal Carats { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Prop { get; set; }
        public string? Pol { get; set; }
        public string? Sym { get; set; }
        public string? FL { get; set; }
        public string? Diam { get; set; }
        public decimal Rapa { get; set; }
        public decimal PurchaseBack { get; set; }
        public decimal PurchaseRate { get; set; }
        public decimal PurChaseAmount { get; set; }
        public decimal ActualBack { get; set; }
        public decimal ActualRate { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal AskingBack { get; set; }
        public decimal AskingRate { get; set; }
        public decimal AskingAmount { get; set; }
        public decimal CPAVG { get; set; }
        public decimal CPAVGAmt { get; set; }
        public decimal SPAVG { get; set; }
        public decimal SPAVGAmt { get; set; }
        public string? LAB { get; set; }
        public string? CNo { get; set; }
        public string? HA { get; set; }
        public string? Shade { get; set; }
        public string? Nts { get; set; }
        public decimal Partyholdcts { get; set; }
        public string? Milky { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? Comments { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal MRPAmount { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? PagePath { get; set; }
        public string? CurStatus { get; set; }
        public string? Black { get; set; }
        public string? LaserInscription { get; set; }
        public string? MemberComments { get; set; }
        public string? EyeClean { get; set; }
        public string? FancyColor { get; set; }
        public string? BoxName { get; set; }
        public string? ItemGroupId { get; set; }
        public string? Packet { get; set; }
        public string? Kapan { get; set; }
        public decimal LW { get; set; }
        public decimal TDep { get; set; }
        public decimal TabPer { get; set; }
        public decimal CH { get; set; }
        public decimal CA { get; set; }
        public decimal PH { get; set; }
        public decimal PA { get; set; }
        public string? Brown { get; set; }
        public string? luster { get; set; }
        public string? Remarks { get; set; }
        public string? Tinc { get; set; }
        public string? cinc { get; set; }
        public string? IP { get; set; }
        public string? OIT { get; set; }
        public string? OIG { get; set; }
        public string? OIC { get; set; }
        public string? OIP { get; set; }
        public string? Size { get; set; }
        public string? FLColor { get; set; }
        public string? TypeID { get; set; }
        public decimal GirdlePer { get; set; }
        public string? girdlemm { get; set; }
        public string? Girdle { get; set; }
        public string? GirdleCondition { get; set; }
        public string? GrMin { get; set; }
        public string? GrMax { get; set; }
        public string? Culet { get; set; }
        public string? PDate { get; set; }
        public string? RDate { get; set; }
        public string? NewArrDate { get; set; }
        public string? WebRDate { get; set; }
        public string? NotShow { get; set; }
        public string? StrLn { get; set; }
        public string? CuletCondition { get; set; }
        public string? CertPath { get; set; }
        public string? HeartsImagePath { get; set; }
        public string? ArrowsImagePath { get; set; }
        public string? AssetImagePath { get; set; }
        public string? MemoPartyName { get; set; }
        public string? HoldPartyName { get; set; }
        public int Pcs { get; set; }
    }

    public class SolitaireAdminLocationUpdateDTO
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public List<string> LotNumbers { get; set; } = new();
    }

    public class SolitaireAdminBoxGroupUpdateDTO
    {
        public string? BoxName { get; set; }
        public string? Group { get; set; }
        public List<string> LotNumbers { get; set; } = new();
    }
}
