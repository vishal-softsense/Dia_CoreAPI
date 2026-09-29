using System;
using System.Collections.Generic;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.DTOs.FinancialTransaction.Payment
{
    public class JournalEntryListItemDto
    {
        public string? GCODE { get; set; }
        public string? code { get; set; }
        public string? Date { get; set; }
        public string? DueDate { get; set; }
        public string? Vtype { get; set; }
        public string? DAccount { get; set; }
        public string? CAccount { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
    }

    public class JournalEntryPageDataDto
    {
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
        public float MBaseRate { get; set; }
        public float SBaseRate { get; set; }
        public long MCurId { get; set; }
        public long SCurId { get; set; }
    }

    public class JournalEntryDetailDto
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

    /// <summary>MVC: JVPayDetails (Add) / IncomeExpenseDetails (Edit)</summary>
    public class JournalEntrySaveRequest
    {
        public string? GCODE { get; set; }
        public DateTime TransDate { get; set; }
        public long CrAccount { get; set; }
        public long DrAccount { get; set; }
        public long mainacc { get; set; }
        public long trnacc { get; set; }
        public float Amt { get; set; }
        public float Rec { get; set; }
        public float Paid { get; set; }
        public int CurCode { get; set; }
        public float BaseRate { get; set; }
        public float SRate { get; set; }
        public string? Desc { get; set; }
        public string? disc { get; set; }
        public string? MOp { get; set; }
        public string? SOp { get; set; }
    }

    public class JournalEntryDeleteRequest
    {
        public int Id { get; set; }
    }

    internal class JournalEntryListRow
    {
        public string? GCODE { get; set; }
        public string? code { get; set; }
        public string? Date { get; set; }
        public string? DueDate { get; set; }
        public string? Vtype { get; set; }
        public string? DAccount { get; set; }
        public string? CAccount { get; set; }
        public double Amount { get; set; }
        public string? Symbol { get; set; }
        public double MasterCurrencyAmount { get; set; }
        public double StockCurrencyAmount { get; set; }
    }

    internal class JournalEntryDetailRow
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

    internal class JournalEntryJvCodeRow
    {
        public int Code { get; set; }
    }
}
