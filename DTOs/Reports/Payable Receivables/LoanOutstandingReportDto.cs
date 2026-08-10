using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Payable_Receivables
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class LoanOutstandingReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FromDateDue { get; set; }
        public string? ToDateDue { get; set; }
        public string? Party { get; set; }
        public bool LoanTaken { get; set; }
        public bool LoanGiven { get; set; }
        public string? Broker { get; set; }
        public string? Currency { get; set; }
        public bool Summary { get; set; }
        public bool PaidInvoice { get; set; }
    }

    public class LoanOutstandingReportDropdownsDto
    {
        public List<BasicDataDto> Currency { get; set; } = new();
    }

    public class LoanOutstandingReportResultDto
    {
        public long Code { get; set; }
        public string? Date { get; set; }
        public string? DueDate { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Party { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvType { get; set; }
        public string? DueDays { get; set; }
        public string? Totcts { get; set; }
        public string? PartyMobile { get; set; }
        public string? Broker { get; set; }
        public string? BrokerMobile { get; set; }
        public string? Amount { get; set; }
        public float PaidAmount { get; set; }
        public float ExDiff { get; set; }
        public float RemainAmount { get; set; }
        public float PostDateAmount { get; set; }
        public float ExportAmount { get; set; }
        public string? Seller { get; set; }
        public string? Symbol { get; set; }
        public string? Disc { get; set; }
        public string? Rate { get; set; }
        public string? Remarks { get; set; }
        public string? UserName { get; set; }
        public string? ExpPartyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? TrnType { get; set; }
    }
}
