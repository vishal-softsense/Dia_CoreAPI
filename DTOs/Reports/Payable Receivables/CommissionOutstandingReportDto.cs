using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Payable_Receivables
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class CommissionOutstandingReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FromDateDue { get; set; }
        public string? ToDateDue { get; set; }
        public string? Party { get; set; }
        public bool Brokerage { get; set; }
        public bool Adat { get; set; }
        public string? AdatName { get; set; }
        public string? Currency { get; set; }
        public bool Summary { get; set; }
        public bool PaidInvoice { get; set; }
        public bool PaidBrokerage { get; set; }
        public string? SalesPerson { get; set; }
    }

    public class CommissionOutstandingReportDropdownsDto
    {
        public List<BasicDataDto> Currency { get; set; } = new();
        public List<CommissionSalesPersonDto> SalesPersons { get; set; } = new();
    }

    public class CommissionSalesPersonDto
    {
        public string? SalesPerson { get; set; }
        public string? Name { get; set; }
    }

    public class CommissionOutstandingReportResultDto
    {
        public long Code { get; set; }
        public string? Date { get; set; }
        public string? InvoiceNo { get; set; }
        public string? BillNo { get; set; }
        public string? DueDays { get; set; }
        public string? DueDate { get; set; }
        public string? Party { get; set; }
        public string? PartyMobile { get; set; }
        public string? Broker { get; set; }
        public string? Adat { get; set; }
        public string? Name { get; set; }
        public string? BrokerMobile { get; set; }
        public string? AdatMobile { get; set; }
        public string? Totcts { get; set; }
        public string? Rate { get; set; }
        public string? SalesPerson { get; set; }
        public float Amount { get; set; }
        public string? Symbol { get; set; }
        public float Stockrate { get; set; }
        public string? Remarks { get; set; }
        public string? Trntype { get; set; }
        public string? InvType { get; set; }
        public float PaidAmount { get; set; }
        public float RemainAmount { get; set; }
        public string? Seller { get; set; }
        public string? Issuser { get; set; }
        public string? UserName { get; set; }
        public string? Partycode { get; set; }
        public float ExDiff { get; set; }
        public float Brokeragepaidamount { get; set; }
        public float MasterCurrencyFinalAmount { get; set; }
        public float MasterCurrencyInvPaidAmt { get; set; }
        public float MasterCurrencyInvExDiffAmt { get; set; }
        public float MasterCurrencyInvRemainAmt { get; set; }
        public float Invoiceamount { get; set; }
        public float BrokerAmount { get; set; }
        public float BrokerPaid { get; set; }
        public float BrokerRemain { get; set; }
        public float AdatAmount { get; set; }
        public float AdatPaid { get; set; }
        public float AdatRemain { get; set; }
    }
}
