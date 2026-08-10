using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using StockBasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    public class LedgerReportDropdownsDTO
    {
        public List<StockBasicDataDto> Currency { get; set; } = new();
    }

    public class LedgerReportFilterDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Curcode { get; set; }
        public int Party { get; set; }
        public string? AccountName { get; set; }
        public string? PartyName { get; set; }
    }

    /// <summary>
    /// Dual Ledger row — matches MVC DataTables columns from SP_H_LedgerDatewiseReport.
    /// Explicit property names so Account1/Date1/VType1 are never dropped in JSON.
    /// </summary>
    public class DualLedgerRowDTO
    {
        [JsonPropertyName("Date")]
        public object? Date { get; set; }

        [JsonPropertyName("DueDate")]
        public object? DueDate { get; set; }

        [JsonPropertyName("VType")]
        public string? VType { get; set; }

        [JsonPropertyName("Account")]
        public string? Account { get; set; }

        [JsonPropertyName("Carats")]
        public object? Carats { get; set; }

        [JsonPropertyName("SDebitAmount")]
        public object? SDebitAmount { get; set; }

        [JsonPropertyName("MDebitAmount")]
        public object? MDebitAmount { get; set; }

        [JsonPropertyName("InvDescription")]
        public string? InvDescription { get; set; }

        [JsonPropertyName("Date1")]
        public object? Date1 { get; set; }

        [JsonPropertyName("DueDate1")]
        public object? DueDate1 { get; set; }

        [JsonPropertyName("VType1")]
        public string? VType1 { get; set; }

        [JsonPropertyName("Account1")]
        public string? Account1 { get; set; }

        [JsonPropertyName("Carats1")]
        public object? Carats1 { get; set; }

        [JsonPropertyName("SCreditAmount")]
        public object? SCreditAmount { get; set; }

        [JsonPropertyName("MCreditAmount")]
        public object? MCreditAmount { get; set; }

        [JsonPropertyName("InvDescription1")]
        public string? InvDescription1 { get; set; }

        [JsonPropertyName("GCode")]
        public object? GCode { get; set; }

        [JsonPropertyName("VCode")]
        public object? VCode { get; set; }

        [JsonPropertyName("GCode1")]
        public object? GCode1 { get; set; }

        [JsonPropertyName("VCode1")]
        public object? VCode1 { get; set; }
    }

    /// <summary>
    /// Matches MVC LedgerListDetail property names/casing for JSON output.
    /// </summary>
    public class LedgerListDetailDTO
    {
        [JsonPropertyName("vcode")]
        public long Vcode { get; set; }

        [JsonPropertyName("JVDate")]
        public string? JVDate { get; set; }

        [JsonPropertyName("VType")]
        public string? VType { get; set; }

        [JsonPropertyName("Account")]
        public string? Account { get; set; }

        [JsonPropertyName("StockDebit")]
        public decimal StockDebit { get; set; }

        [JsonPropertyName("StockCredit")]
        public decimal StockCredit { get; set; }

        [JsonPropertyName("StockFinalAmount")]
        public decimal StockFinalAmount { get; set; }

        [JsonPropertyName("MasterDebit")]
        public decimal MasterDebit { get; set; }

        [JsonPropertyName("MasterCredit")]
        public decimal MasterCredit { get; set; }

        [JsonPropertyName("MasterFinalAmount")]
        public decimal MasterFinalAmount { get; set; }

        [JsonPropertyName("FormattedJvDate")]
        public string? FormattedJvDate { get; set; }

        [JsonPropertyName("InvNo")]
        public string? InvNo { get; set; }

        [JsonPropertyName("GCode")]
        public long GCode { get; set; }
    }

    /// <summary>
    /// Matches MVC LedgerReportVM for Account/LedgerReportPrintJS.
    /// </summary>
    public class LedgerPrintDTO
    {
        [JsonPropertyName("PartyName")]
        public string? PartyName { get; set; }

        [JsonPropertyName("AddressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("AddressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonPropertyName("AddressLine3")]
        public string? AddressLine3 { get; set; }

        [JsonPropertyName("DateRange")]
        public string? DateRange { get; set; }

        [JsonPropertyName("GenerateDate")]
        public string? GenerateDate { get; set; }

        [JsonPropertyName("ThrouhDate")]
        public string? ThrouhDate { get; set; }

        [JsonPropertyName("ToDate")]
        public string? ToDate { get; set; }

        [JsonPropertyName("OpeningBalance")]
        public decimal OpeningBalance { get; set; }

        [JsonPropertyName("BalanceAmt")]
        public decimal BalanceAmt { get; set; }

        [JsonPropertyName("Items")]
        public List<LedgerPrintItemDTO> Items { get; set; } = new();
    }

    /// <summary>
    /// Matches MVC LedgerItemVM for LedgerReportJS_Print.
    /// </summary>
    public class LedgerPrintItemDTO
    {
        [JsonPropertyName("JVDate")]
        public string? JVDate { get; set; }

        [JsonPropertyName("VType")]
        public string? VType { get; set; }

        [JsonPropertyName("Account")]
        public string? Account { get; set; }

        [JsonPropertyName("InvNo")]
        public string? InvNo { get; set; }

        [JsonPropertyName("StockDebit")]
        public decimal? StockDebit { get; set; }

        [JsonPropertyName("StockCredit")]
        public decimal? StockCredit { get; set; }

        [JsonPropertyName("StockFinalAmount")]
        public decimal? StockFinalAmount { get; set; }
    }

    public class LedgerAccountAddressDTO
    {
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
    }
}
