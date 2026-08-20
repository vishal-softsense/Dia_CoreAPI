using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class SolitairePurchaseReturnInvListDto
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
        public string? per { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class SolitairePurchaseReturnPageDataDto
    {
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
    }

    public class SolitairePurchaseReturnHeaderDto
    {
        public int Code { get; set; }
        public DateTime TrnDate { get; set; }
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
        public float LabCharges { get; set; }
        public float FreightCharges { get; set; }
        public string? Description { get; set; }
    }

    public class SolitairePurchaseReturnItemDto
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
    }

    public class SolitairePurchaseReturnSaveRequest
    {
        public SolitairePurchaseReturnHeaderDto SalePur { get; set; } = new();
        public List<SolitairePurchaseReturnItemDto> Items { get; set; } = new();
    }

    public class SolitairePurchaseReturnDetailDto
    {
        public SolitairePurchaseReturnHeaderDto Header { get; set; } = new();
        public List<SolitairePurchaseReturnItemDto> Items { get; set; } = new();
    }

    public class SolitairePurchaseReturnLotDetailDto
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
    }

    public class SolitairePurchaseReturnLotRequest
    {
        public string? LotNo { get; set; }
    }

    public class SolitairePurchaseReturnDeleteRequest
    {
        public int Id { get; set; }
    }

    public class SolitairePurchaseReturnAddLotRequest
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
        public int editIndex { get; set; } = -1;
        public List<SolitairePurchaseReturnItemDto> Items { get; set; } = new();
    }

    public class SolitairePurchaseReturnGetItemFromListRequest
    {
        public string? LotNo { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public int CurCode { get; set; }
        public List<SolitairePurchaseReturnItemDto> Items { get; set; } = new();
    }

    public class SolitairePurchaseReturnGridViewDto
    {
        public double TotalCarats { get; set; }
        public double TotalAmount { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public string? Alert { get; set; }
        public bool IsEdit { get; set; }
        public List<SolitairePurchaseReturnItemDto> ExcelData { get; set; } = new();
    }
}
