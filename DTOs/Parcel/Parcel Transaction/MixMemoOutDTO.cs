using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class MixMemoOutInvListDto
    {
        public long code { get; set; }
        public DateTime Date { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Inv { get; set; }
        public string? Party { get; set; }
        public string? InvType { get; set; }
        public string? Subtype { get; set; }
        public string? Category { get; set; }
        public float Carats { get; set; }
        public float Amount { get; set; }
        public string? Symbol { get; set; }
        public float MasterCurrencyAmount { get; set; }
        public float StockCurrencyAmount { get; set; }
        public string? Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime RoundDate { get; set; }
        public string? per { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class MixMemoOutPageDataDto
    {
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
    }

    public class MixMemoOutHeaderDto
    {
        public int Code { get; set; }
        public DateTime TrnDate { get; set; }
        public short DueDays { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime RoundDate { get; set; }
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
        public int CompanyID { get; set; }
    }

    public class MixMemoOutItemDto
    {
        public int SrNo { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float Weight { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float Back { get; set; }
        public float Rate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public float Amount { get; set; }
        public string? ExportNo { get; set; }
        public string? Status { get; set; }
        public float SpCarats { get; set; }
        public float ReturnCarats { get; set; }
        public string? StockType { get; set; }
        public string? InvDescription { get; set; }
        public string? Stock_ID { get; set; }
    }

    public class MixMemoOutSaveRequest
    {
        public MixMemoOutHeaderDto SalePur { get; set; } = new();
        public List<MixMemoOutItemDto> Items { get; set; } = new();
    }

    public class MixMemoOutDetailDto
    {
        public MixMemoOutHeaderDto Header { get; set; } = new();
        public List<MixMemoOutItemDto> Items { get; set; } = new();
    }

    public class MixMemoOutLotDetailDto
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float Weight { get; set; }
        public string? Carats { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float Back { get; set; }
        public float Rate { get; set; }
        public float Amount { get; set; }
        public string? FinalAmount { get; set; }
        public string? StockType { get; set; }
        public string? InvDescription { get; set; }
        public string? ExportNo { get; set; }
        public string? Status { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? SizeId { get; set; }
        public string? SieveId { get; set; }
        public float SpCarats { get; set; }
        public float ReturnCarats { get; set; }
    }

    public class MixMemoOutLotRequest
    {
        public string? LotNo { get; set; }
        public int CurCode { get; set; }
        public float SBaseRate { get; set; } = 1;
    }

    public class MixMemoOutDeleteRequest
    {
        public int Id { get; set; }
    }

    public class MixMemoOutAddLotRequest
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? ExportNo { get; set; }
        public float Carat { get; set; }
        public int Pcs { get; set; } = 1;
        public float MRP { get; set; }
        public float Rate { get; set; }
        public float Back { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float Amount { get; set; }
        public string? StockType { get; set; }
        public string? InvDescription { get; set; }
        public int editIndex { get; set; } = -1;
        public List<MixMemoOutItemDto> Items { get; set; } = new();
    }

    public class MixMemoOutGetItemFromListRequest
    {
        public string? LotNo { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public int CurCode { get; set; }
        public float SBaseRate { get; set; } = 1;
    }

    public class MixMemoOutRefreshBackRequest
    {
        public float refreshback { get; set; }
        public float refreshrate { get; set; }
        public float AvgBack { get; set; }
        public float AvgRate { get; set; }
        public List<MixMemoOutItemDto> Items { get; set; } = new();
    }

    public class MixMemoOutGridViewDto
    {
        public double TotalCarats { get; set; }
        public double TotalAmount { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public string? Alert { get; set; }
        public bool IsEdit { get; set; }
        public List<MixMemoOutItemDto> ExcelData { get; set; } = new();
    }

    public class MixMemoOutPrintItemDto
    {
        public string? StockNo { get; set; }
        public string? Description { get; set; }
        public int Qty { get; set; }
        public double Weight { get; set; }
        public double PricePerCarat { get; set; }
        public double Amount { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? PropId { get; set; }
        public string? PolishId { get; set; }
        public string? SymId { get; set; }
        public string? FLId { get; set; }
        public string? CertId { get; set; }
        public string? CertificateNo { get; set; }
        public double Rate { get; set; }
        public string? Back { get; set; }
        public double MasterCurrencyAmount { get; set; }
        /// <summary>MVC MixMemoDualPrint Return Cts / Kept Cts</summary>
        public double ReturnCarats { get; set; }
        public double IssueCarats { get; set; }
        /// <summary>MVC MemoRandumPending / GetMemoReportData — remaining carats &amp; amount</summary>
        public double Balance { get; set; }
        public double BalanceAmt { get; set; }
    }

    public class MixMemoOutPrintViewDto
    {
        public string? TrnDate { get; set; }
        public string? RoundDate { get; set; }
        public string? BillNo { get; set; }
        public string? InvNo { get; set; }
        public short DueDays { get; set; }
        public string? PartyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? ShipVia { get; set; }
        public string? Tracking { get; set; }
        public string? SalesPerson { get; set; }
        public string? SalesPersons { get; set; }
        /// <summary>MVC Dual Round Off = PlusMinusAmount * BaseRate; grand total = FinalAmount</summary>
        public double PlusMinusAmount { get; set; }
        public double BaseRate { get; set; }
        public double FinalAmount { get; set; }
        public List<MixMemoOutPrintItemDto> Items { get; set; } = new();
    }

    public class MixMemoOutShipViaOptionDto
    {
        public string? id { get; set; }
        public string? text { get; set; }
    }

    /// <summary>MVC common/get-Mixno-list shape (id=LotNo, text=display label)</summary>
    public class MixMemoOutLotOptionDto
    {
        public string? id { get; set; }
        public string? text { get; set; }
        public string? name { get; set; }
        public string? TextName { get; set; }
    }
}
