using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class SolitaireSaleReturnInvListDto
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

    public class SolitaireSaleReturnPageDataDto
    {
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
    }

    public class SolitaireSaleReturnHeaderDto
    {
        public int Code { get; set; }
        public DateTime TrnDate { get; set; }
        public bool chkoldsale { get; set; }
        public bool OutStanding { get; set; }
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

    public class SolitaireSaleReturnItemDto
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
        public float MemoTransCode { get; set; }
        public float MemoTranCode { get; set; }
        public float ReturnCarats { get; set; }
        public float RtnCarats { get; set; }
        public string? Status { get; set; }
        public float SpCarats { get; set; }
        public string? SRtnStatus { get; set; }
    }

    public class SolitaireSaleReturnSaveRequest
    {
        public SolitaireSaleReturnHeaderDto SalePur { get; set; } = new();
        public List<SolitaireSaleReturnItemDto> Items { get; set; } = new();
    }

    public class SolitaireSaleReturnDetailDto
    {
        public SolitaireSaleReturnHeaderDto Header { get; set; } = new();
        public List<SolitaireSaleReturnItemDto> Items { get; set; } = new();
    }

    public class SolitaireSaleReturnLotDetailDto
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

    public class SolitaireSaleReturnLotRequest
    {
        public string? LotNo { get; set; }
        public int CurCode { get; set; }
        public float SBaseRate { get; set; } = 1;
    }

    public class SolitaireSaleReturnDeleteRequest
    {
        public int Id { get; set; }
    }

    public class SolitaireSaleReturnAddLotRequest
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
        public float MemoTransCode { get; set; }
        public float ReturnCarats { get; set; }
        public int editIndex { get; set; } = -1;
        public List<SolitaireSaleReturnItemDto> Items { get; set; } = new();
    }

    public class SolitaireSaleReturnGetItemFromListRequest
    {
        public string? LotNo { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public int CurCode { get; set; }
        public float SBaseRate { get; set; } = 1;
    }

    public class SolitaireSaleReturnRefreshBackRequest
    {
        public float refreshback { get; set; }
        public float refreshrate { get; set; }
        public float AvgBack { get; set; }
        public float AvgRate { get; set; }
        public List<SolitaireSaleReturnItemDto> Items { get; set; } = new();
    }

    public class SolitaireSaleReturnGridViewDto
    {
        public double TotalCarats { get; set; }
        public double TotalAmount { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public string? Alert { get; set; }
        public bool IsEdit { get; set; }
        public List<SolitaireSaleReturnItemDto> ExcelData { get; set; } = new();
    }

    public class SolitaireSaleReturnSaleRequest
    {
        public string? PartyCode { get; set; }
        public string? InvNo { get; set; }
        public string? MemoNo { get; set; }
    }

    public class SolitaireSaleReturnSaleRowDto
    {
        public string? Date { get; set; }
        public string? MemoNo { get; set; }
        public string? Lotno { get; set; }
        public double MReturnCarats { get; set; }
        public double carats { get; set; }
        public double Sale { get; set; }
        public double RtnCts { get; set; }
        public double BalanceCts { get; set; }
        public double rate { get; set; }
        public double MRP { get; set; }
        public double Back { get; set; }
        public string? Symbol { get; set; }
        public string? itemname { get; set; }
        public double MemoTransCode { get; set; }
        public float CurCode { get; set; }
        public string? ShipVia { get; set; }
        public string? Tracking { get; set; }
        public string? SalesPerson { get; set; }
    }

    public class SolitaireSaleReturnFillRowDto
    {
        public bool Chk { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? ExportNo { get; set; }
        public float Weight { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float Rate { get; set; }
        public float Back { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float MemoTransCode { get; set; }
        public float ReturnCarats { get; set; }
    }

    public class SolitaireSaleReturnFillRequest
    {
        public List<SolitaireSaleReturnFillRowDto> Rows { get; set; } = new();
        public List<SolitaireSaleReturnItemDto> Items { get; set; } = new();
    }

    public class SolitaireSaleReturnPrintItemDto
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
    }

    public class SolitaireSaleReturnPrintViewDto
    {
        public string? TrnDate { get; set; }
        public string? BillNo { get; set; }
        public string? InvNo { get; set; }
        public short DueDays { get; set; }
        public string? PartyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? ShipVia { get; set; }
        public string? Tracking { get; set; }
        public List<SolitaireSaleReturnPrintItemDto> Items { get; set; } = new();
    }
}
