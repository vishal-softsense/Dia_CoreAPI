using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class MixSaleInvListDto
    {
        public long code { get; set; }
        public DateTime Date { get; set; }
        public DateTime? RoundDate { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Inv { get; set; }
        public string? Party { get; set; }
        public string? InvType { get; set; }
        public string? Subtype { get; set; }
        public string? Category { get; set; }
        public float Carats { get; set; }
        public float Weight { get; set; }
        public float Amount { get; set; }
        public string? Symbol { get; set; }
        public float MasterCurrencyAmount { get; set; }
        public float StockCurrencyAmount { get; set; }
        public string? Status { get; set; }
        public DateTime DueDate { get; set; }
        public string? per { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class MixSalePageDataDto
    {
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public List<BasicDataDto> Shape { get; set; } = new();
        public List<BasicDataDto> Color { get; set; } = new();
        public List<BasicDataDto> Clarity { get; set; } = new();
        public List<BasicDataDto> Size { get; set; } = new();
        public List<BasicDataDto> Sieve { get; set; } = new();
        public List<BasicDataDto> ProductType { get; set; } = new();
        public List<BasicDataDto> ItemGroup { get; set; } = new();
        public List<BasicDataDto> Address { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
    }

    public class MixSaleHeaderDto
    {
        public int Code { get; set; }
        public DateTime TrnDate { get; set; }
        public string? Mode { get; set; }
        public bool chkGrading { get; set; }
        public bool chkoldsale { get; set; }
        public short DueDays { get; set; }
        public DateTime DueDate { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? InvType { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public int CurCode { get; set; }
        public float BaseRate { get; set; }
        public float SBaseRate { get; set; }
        public long PartyCode { get; set; }
        public string? PartyName { get; set; }
        public long? AdatCode { get; set; }
        public string? AdatName { get; set; }
        public float AdatPer { get; set; }
        public float AdatAmount { get; set; }
        public long? BrokerCode { get; set; }
        public string? BrokerName { get; set; }
        public float BrokerPer { get; set; }
        public float BrokerAmount { get; set; }
        public string? InvDisc { get; set; }
        public float TotCts { get; set; }
        public float TotAmount { get; set; }
        public float PlusMinusAmount { get; set; }
        public float NetAmount { get; set; }
        public float NetMAmount { get; set; }
        public float RoundAmount { get; set; }
        public float FinalMAmount { get; set; }
        public float FinalMPayRecAmount { get; set; }
        public float VatPer { get; set; }
        public float VatAmount { get; set; }
        public float CGSTPer { get; set; }
        public float CGSTAmt { get; set; }
        public float SGSTPer { get; set; }
        public float SGSTAmt { get; set; }
        public float IGSTPer { get; set; }
        public float IGSTAmt { get; set; }
        public string? Term { get; set; }
        public long ExpPartyCode { get; set; }
        public float InterestPer { get; set; }
        public int DivideDays { get; set; }
        public string? ShipVia { get; set; }
        public string? DeliverTo { get; set; }
        public string? Tracking { get; set; }
        public string? SalesPerson { get; set; }
        public float LabCharges { get; set; }
        public float FreightCharges { get; set; }
        public string? Description { get; set; }
        public string? ConsignmentNo { get; set; }
        public string? MemoNo { get; set; }
        public int Address { get; set; } = 1;
        public DateTime? RoundDate { get; set; }
    }

    public class MixSaleItemDto
    {
        public int SrNo { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? Shape { get; set; }
        public string? ShapeId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ColorId { get; set; }
        public string? Clarity { get; set; }
        public string? ClarityId { get; set; }
        public string? Sieve { get; set; }
        public string? SieveId { get; set; }
        public string? PShape { get; set; }
        public string? PSize { get; set; }
        public string? PColor { get; set; }
        public string? PClarity { get; set; }
        public string? PSieve { get; set; }
        public float Weight { get; set; }
        public float Carats { get; set; }
        public float IssueCarats { get; set; }
        public int Pcs { get; set; }
        public string? StockType { get; set; }
        public string? SuppLotNo { get; set; }
        public string? GroupId { get; set; }
        public string? StoneType { get; set; }
        public string? RMK { get; set; }
        public float MRP { get; set; }
        public float PurchaseMRP { get; set; }
        public float PurchaseBack { get; set; }
        public float PMRPDown { get; set; }
        public float PurchaseRate { get; set; }
        public float Rate { get; set; }
        public float PurAmount { get; set; }
        public float Amount { get; set; }
        public string? Cut { get; set; }
        public string? Polish { get; set; }
        public string? Symmetry { get; set; }
        public string? FL { get; set; }
        public string? Proportion { get; set; }
        public float LW { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }
        public float TotDep { get; set; }
        public float TablePer { get; set; }
        public string? Culet { get; set; }
        public string? GMin { get; set; }
        public string? GMax { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float GirdlePer { get; set; }
        public string? GirdleCondition { get; set; }
        public string? Brown { get; set; }
        public string? Green { get; set; }
        public string? Milky { get; set; }
        public string? HA { get; set; }
        public string? Natts { get; set; }
        public string? EyeClean { get; set; }
        public string? Shade { get; set; }
        public string? Luster { get; set; }
        public string? OIT { get; set; }
        public string? OIG { get; set; }
        public string? OIC { get; set; }
        public string? OIP { get; set; }
        public string? BIT { get; set; }
        public string? BIS { get; set; }
        public string? LAB { get; set; }
        public string? CertificateNo { get; set; }
        public string? InscriptionText { get; set; }
        public float CH { get; set; }
        public float CA { get; set; }
        public float PA { get; set; }
        public float PH { get; set; }
        public float StrLn { get; set; }
        public float LrHalf { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? LabComments { get; set; }
        public string? ReportDate { get; set; }
        public string? MemberRemarks { get; set; }
        public string? FLColor { get; set; }
        public string? CuletCondition { get; set; }
        public string? FancyColor { get; set; }
        public string? FancyColorIntensity { get; set; }
        public string? FancyColorOvertone { get; set; }
        public string? ItemGroup { get; set; }
        public string? MfgSeriesID { get; set; }
        public string? MfgPacketID { get; set; }
        public float AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float ActualBack { get; set; }
        public float ActualRate { get; set; }
        public float BottomBack { get; set; }
        public float BottomRate { get; set; }
        public float B2BBack { get; set; }
        public float B2BRate { get; set; }
        public float B2CBack { get; set; }
        public float B2CRate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public bool Chk { get; set; }
        public float MemoTransCode { get; set; }
        public float RtnCarats { get; set; }
        public float ReturnCarats { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? PagePath { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? CountryOfMfg { get; set; }
        public string? Status { get; set; }
    }

    public class MixSaleSaveRequest
    {
        public MixSaleHeaderDto SalePur { get; set; } = new();
        public List<MixSaleItemDto> Items { get; set; } = new();
    }

    public class MixSaleDetailDto
    {
        public MixSaleHeaderDto Header { get; set; } = new();
        public List<MixSaleItemDto> Items { get; set; } = new();
    }

    public class MixSaleLotDetailDto
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float Weight { get; set; }
        /// <summary>SP GetLotNoDataForAll (Type=Mix) returns Carats as string — mapped to Weight in repository.</summary>
        public string? Carats { get; set; }
        public float IssueCarats { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        /// <summary>SP column name for MRP on lot lookup (string in MVC LotNoData).</summary>
        public string? PurchaseMRP { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public string? PurchaseAmount { get; set; }
        public string? FinalAmount { get; set; }
        public string? Shape { get; set; }
        public string? ShapeId { get; set; }
        public string? Color { get; set; }
        public string? ColorId { get; set; }
        public string? Clarity { get; set; }
        public string? ClarityId { get; set; }
        public string? Size { get; set; }
        public string? Sieve { get; set; }
        public string? SieveId { get; set; }
        public string? StockType { get; set; }
        public string? Polish { get; set; }
        public string? Symmetry { get; set; }
        public string? FL { get; set; }
        public string? Proportion { get; set; }
        public string? LAB { get; set; }
        public string? CertificateNo { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }
        public float TotDep { get; set; }
        public float TablePer { get; set; }
        public float LW { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? PagePath { get; set; }
        public string? Brown { get; set; }
        public string? Milky { get; set; }
        public string? HA { get; set; }
        public string? Natts { get; set; }
        public string? EyeClean { get; set; }
        public string? Shade { get; set; }
        public string? GMin { get; set; }
        public string? GMax { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float GirdlePer { get; set; }
        public string? Culet { get; set; }
        public string? OIT { get; set; }
        public string? OIG { get; set; }
        public string? OIC { get; set; }
        public string? OIP { get; set; }
        public string? BIT { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? CountryOfMfg { get; set; }
    }

    public class MixSaleLotRequest
    {
        public string? LotNo { get; set; }
    }

    // MixConsignmentRequest / MixConsignmentRowDto are defined in MixPurchaseDto.cs (same namespace).

    public class MixSaleDeleteRequest
    {
        public int Id { get; set; }
    }

    public class MixSaleAddLotRequest
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? ShapeId { get; set; }
        public string? SizeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? SieveId { get; set; }
        public float Carat { get; set; }
        public float Weight { get; set; }
        public float IssueCarats { get; set; }
        public float ReturnCarats { get; set; }
        public float PurchaseBack { get; set; }
        public int Pcs { get; set; } = 1;
        public float MRP { get; set; }
        public float Rate { get; set; }
        public float Back { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float Amount { get; set; }
        public float MemoTransCode { get; set; }
        public float RtnCarats { get; set; }
        public string? StockType { get; set; }
        public string? SuppLotNo { get; set; }
        public float AskingRate { get; set; }
        public float ActualRate { get; set; }
        public string? GroupId { get; set; }
        public string? StoneType { get; set; }
        public string? RMK { get; set; }
        public int editIndex { get; set; } = -1;
        public List<MixSaleItemDto> Items { get; set; } = new();
    }

    public class MixSaleGridViewDto
    {
        public double TotalCarats { get; set; }
        public double TotalAmount { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public string? Alert { get; set; }
        public List<MixSaleItemDto> ExcelData { get; set; } = new();
    }

    public class MixSalePrintItemDto
    {
        public string? StockNo { get; set; }
        public string? Description { get; set; }
        public int Qty { get; set; }
        public double Weight { get; set; }
        public double PricePerCarat { get; set; }
        public double Amount { get; set; }
    }

    public class MixSalePrintViewDto
    {
        public string? TrnDate { get; set; }
        public string? BillNo { get; set; }
        public string? InvNo { get; set; }
        public short DueDays { get; set; }
        public string? PartyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public List<MixSalePrintItemDto> Items { get; set; } = new();
    }

    public class MixSaleStatusItemDto
    {
        public string? Type { get; set; }
        public DateTime? Date { get; set; }
        public int? Qty { get; set; }
        public double? Wt { get; set; }
        public string? Description { get; set; }
        public double? Rate { get; set; }
        public double? Tax { get; set; }
        public double? Amount { get; set; }
        public int IsPayment { get; set; }
    }

    public class MixSaleStatusDto
    {
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public string? Status { get; set; }
        public string? Party { get; set; }
        public string? AddLine1 { get; set; }
        public string? AddLine2 { get; set; }
        public string? AddLine3 { get; set; }
        public string? Mobile { get; set; }
        public string? CPerson { get; set; }
        public string? SalesPerson { get; set; }
        public string? ShipVia { get; set; }
        public List<MixSaleStatusItemDto> Items { get; set; } = new();
    }
}
