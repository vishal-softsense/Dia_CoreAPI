using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class MixSaleReturnInvListDto
    {
        public long code { get; set; }
        public DateTime Date { get; set; }
        public DateTime RoundDate { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Inv { get; set; }
        public string? Party { get; set; }
        public string? InvType { get; set; }
        public string? Subtype { get; set; }
        public string? Category { get; set; }
        public float Weight { get; set; }
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

    public class MixSaleReturnPageDataDto
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
        public List<BasicDataDto> Address { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
    }

    public class MixSaleReturnHeaderDto
    {
        public int Code { get; set; }
        public DateTime TrnDate { get; set; }
        public DateTime RoundDate { get; set; }
        public string? Mode { get; set; }
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
        public string? MemoNo { get; set; }
        public float LabCharges { get; set; }
        public float FreightCharges { get; set; }
        public int Address { get; set; } = 1;
    }

    public class MixSaleReturnItemDto
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
        public float IssueCarats { get; set; }
        public float ReturnCarats { get; set; }
        public float Back { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float COGS { get; set; }
        public float Rate { get; set; }
        public float SRate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public float Amount { get; set; }
        public string? ExportNo { get; set; }
        public string? StockType { get; set; }
        public string? InvDescription { get; set; }
        public float MemoTransCode { get; set; }
        /// <summary>SP GetSalepurTran often returns MemoTranCode (no 's'); mapped then copied to MemoTransCode.</summary>
        public float MemoTranCode { get; set; }
    }

    public class MixSaleReturnSaveRequest
    {
        public MixSaleReturnHeaderDto SalePur { get; set; } = new();
        public List<MixSaleReturnItemDto> Items { get; set; } = new();
    }

    public class MixSaleReturnDetailDto
    {
        public MixSaleReturnHeaderDto Header { get; set; } = new();
        public List<MixSaleReturnItemDto> Items { get; set; } = new();
    }

    public class MixSaleReturnLotDetailDto
    {
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float Weight { get; set; }
        public string? Carats { get; set; }
        public float IssueCarats { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float Back { get; set; }
        public float Rate { get; set; }
        public float Amount { get; set; }
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
        public string? InvDescription { get; set; }
    }

    public class MixSaleReturnLotRequest
    {
        public string? LotNo { get; set; }
    }

    public class MixSaleReturnDeleteRequest
    {
        public int Id { get; set; }
    }

    public class MixSaleReturnAddLotRequest
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
        public float Back { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float COGS { get; set; }
        public float Rate { get; set; }
        public float SRate { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float Amount { get; set; }
        public string? ExportNo { get; set; }
        public float MemoTransCode { get; set; }
        public string? StockType { get; set; }
        public string? InvDescription { get; set; }
        public int editIndex { get; set; } = -1;
        public List<MixSaleReturnItemDto> Items { get; set; } = new();
    }

    public class MixSaleReturnGetItemFromListRequest
    {
        public string? LotNo { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public int CurCode { get; set; }
        public List<MixSaleReturnItemDto> Items { get; set; } = new();
    }

    public class MixSaleReturnGridViewDto
    {
        public double TotalCarats { get; set; }
        public double TotalAmount { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public string? Alert { get; set; }
        public bool IsEdit { get; set; }
        public List<MixSaleReturnItemDto> ExcelData { get; set; } = new();
    }

    /// <summary>MVC MixSaleRtnPrint_USA / MixSaleRtnPrintInv header + lines.</summary>
    public class MixSaleReturnPrintViewDto
    {
        public string? TrnDate { get; set; }
        public string? RoundDate { get; set; }
        public string? DueDate { get; set; }
        public string? BillNo { get; set; }
        public string? InvNo { get; set; }
        public string? MemoNo { get; set; }
        public string? DueDays { get; set; }
        public string? PartyName { get; set; }
        public string? BrokerName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? ShipVia { get; set; }
        public string? SalesPersons { get; set; }
        public string? Tracking { get; set; }
        public double PlusMinusAmount { get; set; }
        public double BaseRate { get; set; }
        public double FinalAmount { get; set; }
        public string? bnm { get; set; }
        public string? BankAccNo { get; set; }
        public string? ABARoutingNumber { get; set; }
        public string? SwiftCode { get; set; }
        public string? IbanNo { get; set; }
        public string? ZelleID { get; set; }
        public string? CompnyTaxRegisterNumber { get; set; }
        public List<MixSaleReturnPrintItemDto> Items { get; set; } = new();
    }

    public class MixSaleReturnPrintItemDto
    {
        public string? InvNo { get; set; }
        public string? Lotno { get; set; }
        public string? StockNo { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public int Pcs { get; set; }
        public int Qty { get; set; }
        public double Carats { get; set; }
        public double Weight { get; set; }
        public double Rate { get; set; }
        public double PricePerCarat { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double Amount { get; set; }
    }

    /// <summary>MVC: CsaleToCsalertnView / Sp_SelectCSaleForCSaleRtn result (MemoToSaleList).</summary>
    public class MixSaleReturnCSaleLookupRequest
    {
        public string? PartyCode { get; set; }
        public string? MemoNo { get; set; }
    }

    public class MixSaleReturnCSaleLineDto
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
        public string? StockType { get; set; }
        public string? JewItem { get; set; }
        public string? JewItemName { get; set; }
    }

    public class MixSaleReturnCSaleFillLineRequest
    {
        public bool Chk { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float IssueCarats { get; set; }
        public float Weight { get; set; }
        public float ReturnCarats { get; set; }
        public float Rate { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float MemoTransCode { get; set; }
        public string? ExportNo { get; set; }
        public string? StockType { get; set; }
        public string? PShape { get; set; }
        public string? PColor { get; set; }
        public string? PSieve { get; set; }
        public string? PSize { get; set; }
        public string? PClarity { get; set; }
        public int Pcs { get; set; }

        public string? InvDescription { get; set; }
    }

    public class MixSaleReturnCSaleFillRequest
    {
        public List<MixSaleReturnCSaleFillLineRequest> Lines { get; set; } = new();
        public List<MixSaleReturnItemDto> Items { get; set; } = new();
    }
}
