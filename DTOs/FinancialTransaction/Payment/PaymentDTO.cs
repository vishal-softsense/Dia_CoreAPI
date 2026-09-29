using System;
using System.Collections.Generic;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.DTOs.FinancialTransaction.Payment
{
    public class PaymentListItemDto
    {
        public string? GCODE { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? code { get; set; }
        public DateTime Date { get; set; }
        public string? DueDate { get; set; }
        public string? Vtype { get; set; }
        public string? Account { get; set; }
        public string? Party { get; set; }
        public string? CD { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
        public string? disc { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        public int CurCode { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
    }

    public class PaymentDetailDto
    {
        public string? GCODE { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public DateTime Date { get; set; }
        public string? Vtype { get; set; }
        public string? Account { get; set; }
        public string? Party { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
        public string? disc { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        public int CurCode { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
        public string? AdjustmentType { get; set; }
        public long MainAccCode { get; set; }
        public string? MainAccName { get; set; }
        public long PartyAccCode { get; set; }
        public string? PartyAccName { get; set; }
    }

    public class PaymentDeleteRequest
    {
        public int Id { get; set; }
    }

    public class PaymentEditDetailsRequest
    {
        public string? Gcode { get; set; }
        public string AdjustmentType { get; set; } = "Payment";
    }

    public class PaymentPageDataDto
    {
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
        public float MBaseRate { get; set; }
        public float SBaseRate { get; set; }
        public long MCurId { get; set; }
        public long SCurId { get; set; }
    }

    public class PaymentJvListItemDto
    {
        public string? GCODE { get; set; }
        public string? code { get; set; }
        public DateTime Date { get; set; }
        public string? Vtype { get; set; }
        public string? DAccount { get; set; }
        public string? CAccount { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
        public int CurCode { get; set; }
        public string? disc { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
    }

    public class PaymentPendingInvDto
    {
        public string? Date { get; set; }
        public string? InvNo { get; set; }
        public string? InvCode { get; set; }
        public string? DueDate { get; set; }
        public string? BillNo { get; set; }
        public double TotCts { get; set; }
        public string? Symbol { get; set; }
        public double SCurAmt { get; set; }
        public double BaseRate { get; set; }
        public double MCurAmt { get; set; }
        public double AdUSDAmount { get; set; }
        public double PayINRRate { get; set; }
        public double AdINRCurAmt { get; set; }
        public double RemUSDAmount { get; set; }
        public double RemINRCurAmt { get; set; }
        public string? TrnType { get; set; }
    }

    public class PaymentPendingInvRequest
    {
        public string? Party { get; set; }
        public string? TrnType { get; set; }
        public int Currency { get; set; }
        public string Mode { get; set; } = "Invoice";
    }

    public class PaymentLineDto
    {
        public string? PDate { get; set; }
        public string? InvNo { get; set; }
        public string? InvCode { get; set; }
        public float BaseRate { get; set; }
        public float AdMCurAmt { get; set; }
        public float AdSCurAmt { get; set; }
    }

    public class PaymentSaveRequest
    {
        public string? GCODE { get; set; }
        public DateTime TransDate { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
        public float Rec { get; set; }
        public float Paid { get; set; }
        public int CurCode { get; set; }
        public float BaseRate { get; set; }
        public float SRate { get; set; }
        public string? Desc { get; set; }
        public string? disc { get; set; }
        public string? MOp { get; set; }
        public string? SOp { get; set; }
        public float adjusdTotalAmt { get; set; }
        public float adjinramount { get; set; }
        public string? PayType { get; set; }
        public bool isJVMode { get; set; }
        public List<PaymentLineDto>? Lines { get; set; }
    }

    internal class PaymentListRow
    {
        public string? GCODE { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? code { get; set; }
        public DateTime Date { get; set; }
        public string? DueDate { get; set; }
        public string? Vtype { get; set; }
        public string? Account { get; set; }
        public string? Party { get; set; }
        public string? CD { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
        public string? disc { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        /// <summary>SQL may return float; keep as double then cast when mapping.</summary>
        public double CurCode { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
    }

    internal class PaymentJvListRow
    {
        public string? GCODE { get; set; }
        public string? code { get; set; }
        public DateTime Date { get; set; }
        public string? Vtype { get; set; }
        public string? DAccount { get; set; }
        public string? CAccount { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
        public int CurCode { get; set; }
        public string? disc { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
    }

    internal class PaymentPendingInvRow
    {
        public string? Date { get; set; }
        public string? InvNo { get; set; }
        public string? InvCode { get; set; }
        public string? DueDate { get; set; }
        public string? BillNo { get; set; }
        public double TotCts { get; set; }
        public string? Symbol { get; set; }
        public double SCurAmt { get; set; }
        public double BaseRate { get; set; }
        public double MCurAmt { get; set; }
        public double AdUSDAmount { get; set; }
        public double PayINRRate { get; set; }
        public double AdINRCurAmt { get; set; }
        public double RemUSDAmount { get; set; }
        public double RemINRCurAmt { get; set; }
        public string? TrnType { get; set; }
    }

    internal class JvCodeRow
    {
        public int Code { get; set; }
    }
}
