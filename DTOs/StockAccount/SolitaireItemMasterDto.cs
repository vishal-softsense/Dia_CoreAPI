using System;
using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.StockAccount
{
    public class PriceIndex
    {
        public string? Cut { get; set; }
        public string? Clarity { get; set; }
        public string? Color { get; set; }
        public float CaratsFrom { get; set; }
        public float CaratsTo { get; set; }
        public float Rate { get; set; }
    }

    public class ItemMaster
    {
        public int ItemCode { get; set; }
        public string LotNo { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string? LAB { get; set; }
        public string? CertificateNo { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? SizeId { get; set; }
        public string? ClarityId { get; set; }
        public float Weight { get; set; }
        public float MRP { get; set; }
    }

    public class SolitaireItemMasterDto
    {
        [Key]
        public int SinglePcs { get; set; }
        public string? StoneTypes { get; set; }
        public int ItemCode { get; set; }

        [Required(ErrorMessage = "Enter Item ID/LotNo")]
        public string LotNo { get; set; } = string.Empty;

        public string? ItemName { get; set; }
        public string? ItemGroupID { get; set; }

        [Required(ErrorMessage = "Enter Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter Product Type")]
        public string ProductType { get; set; } = string.Empty;

        public float Pcs { get; set; }
        public float Weight { get; set; }
        
        public string? SieveId { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? SizeId { get; set; }
        public string? ClarityId { get; set; }
        public string? FancyColor { get; set; }
        public string? FancyColorIntensity { get; set; }
        public string? FancyColorOvertone { get; set; }

        public float MRP { get; set; }
        public float Back { get; set; }
        public float AskingRate { get; set; }
        public float AskingAmount { get; set; }

        public string? BoxName { get; set; }
        public float BoxRate { get; set; }

        public string? LAB { get; set; }
        public string? CertId { get; set; }
        public string? PropId { get; set; }
        public string? PolishId { get; set; }
        public string? SymId { get; set; }
        public string? FLId { get; set; }
        public string? TotDepth { get; set; }
        public string? HAID { get; set; }
        public string? BrownId { get; set; }
        public string? GreenId { get; set; }
        public string? MilkyId { get; set; }
        public string? LusterId { get; set; }
        public string? ButhId { get; set; }
        public string? NattsId { get; set; }
        public string? ShadeId { get; set; }
        public string? OIC { get; set; }
        public float ActualBack { get; set; }
        public float ActualRate { get; set; }
        public float ActualAmount { get; set; }

        public string? TableInclusion { get; set; }
        public string? CrownInclusion { get; set; }
        public string? GirdleMax { get; set; }
        public string? GirdleMin { get; set; }
        public string? GirdleMaxMS { get; set; }
        public string? GirdleMinMS { get; set; }
        public string? CuletId { get; set; }
        public string? CertificateNo { get; set; }

        public string? HA { get; set; }
        public string? Proportion { get; set; }
        public string? Polish { get; set; }
        public string? Symmetry { get; set; }
        public string? Fluorescence { get; set; }
        public string? FlColor { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }
        public string? Diameter { get; set; }
        public string? Measurement { get; set; }
        public string? LWRatio { get; set; }

        public float TotalDepthPer { get; set; }
        public float TablePer { get; set; }
        public float CrownHeight { get; set; }
        public float CrownAngle { get; set; }
        public float PavilionDepth { get; set; }
        public float PavilionHeight { get; set; }
        public float PavilionPer { get; set; }
        public float GirdlePer { get; set; }
        public string? GirdleCondition { get; set; }

        public float GirdleMinPer { get; set; }
        public float GirdleMaxPer { get; set; }
        public float GirdleMinMM { get; set; }
        public float GirdleMaxMM { get; set; }
        public string? Culet { get; set; }
        public string? CuletCondition { get; set; }
        public float StrLn { get; set; }
        public float LrHalf { get; set; }
        public bool LaserInscribed { get; set; }
        public string? LaserInscriptionText { get; set; }
        public string? Instruction { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? LABComments { get; set; }
        public string? MemberComments { get; set; }
        public string? Brown { get; set; }
        public string? Green { get; set; }
        public string? Milky { get; set; }
        public string? Luster { get; set; }
        public string? Buth { get; set; }
        public string? Natts { get; set; }
        public string? EyeClean { get; set; }
        public string? Shade { get; set; }
        public string? BIT { get; set; }
        public string? BIS { get; set; }
        public string? OIT { get; set; }
        public string? OIS { get; set; }
        public string? OIP { get; set; }
        public string? OIG { get; set; }
        public string? PolishFeatures { get; set; }
        public string? SymmetryFeatures { get; set; }
        public string? ColorDescription { get; set; }
        public string? ClarityDescription { get; set; }
        public string? ClarityStatus { get; set; }
        public string? Graining { get; set; }
        public string? Painting { get; set; }
        public string? IdealImagePath { get; set; }
        public string? AssetImagePath { get; set; }
        public string? HeartsImagePath { get; set; }
        public string? ArrowsImagePath { get; set; }

        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? B2BDNAPage { get; set; }
        public string? B2CDNAPage { get; set; }
        public string? CertificatePath { get; set; }
        public string? PlottingImagePath { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime RateReviseDate { get; set; }
        public DateTime NewArrivalDate { get; set; }
        public DateTime StoneLastUnholdDate { get; set; }
        public string? PublicCommnets { get; set; }
        public string? PrivateCommnets { get; set; }
        public string? AdminCommnets { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float PurchaseAmount { get; set; }
        public float SellerBack { get; set; }
        public float SellerRate { get; set; }
        public float SellerAmount { get; set; }
        public float BottomBack { get; set; }
        public float BottomRate { get; set; }
        public float B2BBack { get; set; }
        public float B2BRate { get; set; }
        public float B2CBack { get; set; }
        public float B2CRate { get; set; }
        public string? MfgSeriesID { get; set; }
        public string? MfgPacketID { get; set; }
        public string? RoughID { get; set; }
        public string? MinesID { get; set; }
        public string? InStatus { get; set; }
        public string? CurrrentStatus { get; set; }
        public string? Hold { get; set; }

        public string? CityCode { get; set; }
        public string? CityName { get; set; }
        public string? City { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? CityID { get; set; }
        public string? Country { get; set; }      
    }

    public class SyncListData
    {
        public int Groupid { get; set; }
        public string? Name { get; set; }
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int Port { get; set; }
        public string? Category { get; set; }
    }

    public class ItemMasterDto
    {
        public int ItemCode { get; set; }
        public string LotNo { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }        
        public string? ClarityId { get; set; }
        public string? CutProp { get; set; }
        public string? PropId { get; set; }
        public string? Sym { get; set; }
        public string? Pol { get; set; }
        public string? CertificateNo { get; set; }
        public string? ReportNumber { get; set; }
        public string? Measurement1 { get; set; }
        public string? Measurement2 { get; set; }
        public string? Measurement3 { get; set; }
        public string? TableDiameter { get; set; }
        public string? TotalDepth { get; set; }
        public string? CrownHeight { get; set; }
        public string? CrownAngle { get; set; }
        public string? PavilionDepth { get; set; }
        public string? PavilionAngel { get; set; }
        public string? GirdleName { get; set; }
        public string? CuletSize { get; set; }
        public string? Fluorescence { get; set; }
        public string? LaserInscriptionText { get; set; }
        public string? Weight { get; set; }
        public string? IdealImagePath { get; set; }
        public string? AssetImagePath { get; set; }
        public string? HeartsImagePath { get; set; }
        public string? ArrowsImagePath { get; set; }
        public string? ImagePath { get; set; }
    }
}
