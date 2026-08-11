using System;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Reports.Financial_Reports
{
    /// <summary>
    /// Matches MVC InvoiceFilter (AccReportModel) for POST /report/invoice-ledger-report.
    /// </summary>
    public class InvoiceLedgerReportFilterDTO
    {
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? BrokerCode { get; set; }
        public string? BrokerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? FromDateDue { get; set; }
        public DateTime? ToDateDue { get; set; }
        public string? Category { get; set; }
        public string? intedatadays { get; set; }
        public string? intedataper { get; set; }
        public bool Summary { get; set; }
        public bool PaidInvoice { get; set; }
        public bool PaidBrokerage { get; set; }
        public bool FromDueChkBox { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
    }

    /// <summary>
    /// Matches MVC InvoiceLedgerData from AccReportModel / GetInvoiceLedgerReport.
    /// </summary>
    public class InvoiceLedgerReportDTO
    {
        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("InvNo")]
        public string? InvNo { get; set; }

        [JsonPropertyName("BillNo")]
        public string? BillNo { get; set; }

        [JsonPropertyName("TrnType")]
        public string? TrnType { get; set; }

        [JsonPropertyName("InvType")]
        public string? InvType { get; set; }

        [JsonPropertyName("Category")]
        public string? Category { get; set; }

        [JsonPropertyName("TrnDate")]
        public DateTime? TrnDate { get; set; }

        [JsonPropertyName("DueDate")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("PartyCode")]
        public string? PartyCode { get; set; }

        [JsonPropertyName("PartyName")]
        public string? PartyName { get; set; }

        [JsonPropertyName("BrokerCode")]
        public string? BrokerCode { get; set; }

        [JsonPropertyName("BrokerName")]
        public string? BrokerName { get; set; }

        [JsonPropertyName("TotCts")]
        public decimal? TotCts { get; set; }

        [JsonPropertyName("StockCurrencyFinalAmount")]
        public decimal? StockCurrencyFinalAmount { get; set; }

        [JsonPropertyName("MasterCurrencyFinalAmount")]
        public decimal? MasterCurrencyFinalAmount { get; set; }

        [JsonPropertyName("StockCurrencyRemainAmount")]
        public decimal? StockCurrencyRemainAmount { get; set; }

        [JsonPropertyName("MasterCurrencyRemainAmount")]
        public decimal? MasterCurrencyRemainAmount { get; set; }

        [JsonPropertyName("PayRecCode")]
        public string? PayRecCode { get; set; }

        [JsonPropertyName("PayRecDate")]
        public DateTime? PayRecDate { get; set; }

        [JsonPropertyName("PayRecStockCurrencyAmount")]
        public decimal? PayRecStockCurrencyAmount { get; set; }

        [JsonPropertyName("PayRecMasterCurrencyAmount")]
        public decimal? PayRecMasterCurrencyAmount { get; set; }

        [JsonPropertyName("PayRecDays")]
        public int? PayRecDays { get; set; }

        [JsonPropertyName("StockCurrencyInterest")]
        public decimal? StockCurrencyInterest { get; set; }

        [JsonPropertyName("MasterCurrencyInterest")]
        public decimal? MasterCurrencyInterest { get; set; }

        [JsonPropertyName("ModeOfPayment")]
        public string? ModeOfPayment { get; set; }
    }
}
