using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewelleryStockSearchFilterDto
    {
        public string? Date { get; set; }
        public string? Show { get; set; }
        public string? HoldUnHold { get; set; }
        public string? Status { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
        public string? Filters { get; set; }
        public string? StockId { get; set; }
        public string? JewDesign { get; set; }
        public string? JewCategory { get; set; }
        public string? JewSubCategory { get; set; }
        public int JewSize { get; set; }
        public string? JewCertificate { get; set; }
        public string? JewCertificateNo { get; set; }
        public string? JewMetalType { get; set; }
        public string? JewColor { get; set; }
        public string? JewPurity { get; set; }
        public string? JewLotNo { get; set; }
        public string? DiamondType { get; set; }
        public List<string>? DiamondShape { get; set; }
        public List<string>? DiamondSize { get; set; }
        public List<string>? DiamondColor { get; set; }
        public List<string>? DiamondClarity { get; set; }
    }

    public class JewelleryStockReportItemDto
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? Shape { get; set; }
        public float Carats { get; set; }
        public string? Color { get; set; }
        public float Rate { get; set; }
        public float Amount { get; set; }
        public float AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float AskingAmount { get; set; }
        public float CPAVG { get; set; }
        public float CPAVGAmt { get; set; }
        public float SPAVG { get; set; }
        public float SPAVGAmt { get; set; }
        public string? LAB { get; set; }
        public string? CNo { get; set; }
        public float Partyholdcts { get; set; }
        public string? Comments { get; set; }
        public float MRPAmount { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? CertPath { get; set; }
        public string? HeartsImagePath { get; set; }
        public string? ArrowsImagePath { get; set; }
        public string? AssetImagePath { get; set; }
        public string? PagePath { get; set; }
        public string? CurStatus { get; set; }
        public string? Milky { get; set; }
        public string? luster { get; set; }
        public string? Shade { get; set; }
        public string? Remarks { get; set; }
        public string? Packet { get; set; }
        public string? Kapan { get; set; }
        public string? Black { get; set; }
        public string? Size { get; set; }
        public string? FLColor { get; set; }
        public string? TypeID { get; set; }
        public string? Culet { get; set; }
        public string? PDate { get; set; }
        public string? RDate { get; set; }
        public string? WebRDate { get; set; }
        public string? LaserInscription { get; set; }
        public string? MemberComments { get; set; }
        public string? EyeClean { get; set; }
        public string? FancyColor { get; set; }
        public string? StrLn { get; set; }
        public string? CuletCondition { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float PurChaseAmount { get; set; }
        public int Pcs { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public float FinalJAmount { get; set; }
        public float NetWeight { get; set; }
        public float GrossWeight { get; set; }
        public string? MetalType { get; set; }
        public float TotalMetalAmount { get; set; }
        public float TotalDiamondAmount { get; set; }
        public float TotalStoneAmount { get; set; }
        public float TotalMakingCharges { get; set; }
        public float TotalSettingCharges { get; set; }
        public float TotalHandlingCharges { get; set; }
        public float TotalCertificateCharges { get; set; }
        public float OtherCharges { get; set; }
        public float HallmarkCharges { get; set; }
        public int JewCode { get; set; }
        public string? JewColor { get; set; }
        public string? JewPurity { get; set; }
    }

    public class JewelleryStockDiamondDetailDto
    {
        public int JewCode { get; set; }
        public string? DiamondLotNo { get; set; }
        public string? DiamondType { get; set; }
        public string? DiamondShape { get; set; }
        public string? DiamondSize { get; set; }
        public string? DiamondSieve { get; set; }
        public string? DiamondColor { get; set; }
        public string? DiamondClarity { get; set; }
        public int DiamondPcs { get; set; }
        public float DiamondCarat { get; set; }
        public float DiamondPrice { get; set; }
        public float DiamondAmount { get; set; }
        public string? DiamondLab { get; set; }
    }

    public class JewelleryStockSearchResponseDto
    {
        public JewelleryStockReportItemDto Jewellery { get; set; } = new JewelleryStockReportItemDto();
        public List<JewelleryStockDiamondDetailDto> DiamondDetails { get; set; } = new List<JewelleryStockDiamondDetailDto>();
        public List<JewelleryStockDiamondDetailDto> StoneDetails { get; set; } = new List<JewelleryStockDiamondDetailDto>();
    }

    public class JewelleryStockDropdownsDto
    {
        public IEnumerable<object> DesignList { get; set; } = new List<object>();
        public IEnumerable<object> CategoryList { get; set; } = new List<object>();
        public IEnumerable<object> SubCategoryList { get; set; } = new List<object>();
        public IEnumerable<object> MetalTypeList { get; set; } = new List<object>();
        public IEnumerable<object> JewColorList { get; set; } = new List<object>();
        public IEnumerable<object> MasterDropdowns { get; set; } = new List<object>();
        public IEnumerable<object> ItemMasterDropdowns { get; set; } = new List<object>();
    }

    public class JewelleryStockIdsRequestDto
    {
        public string? SearchText { get; set; }
    }
}
