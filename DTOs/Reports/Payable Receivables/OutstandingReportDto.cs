using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Payable_Receivables
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class OutstandingReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FromDateDue { get; set; }
        public string? ToDateDue { get; set; }
        public string? Party { get; set; }
        public string? DispCurrency { get; set; }
        public string? Adat { get; set; }
        public bool Payable { get; set; }
        public bool Receivable { get; set; }
        public string? Broker { get; set; }
        public string? Seller { get; set; }
        public string? SalesPerson { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
        /// <summary>MVC Outstanding #TrnType filter (comma-separated, e.g. PSale,PPur).</summary>
        public string? TrnType { get; set; }
        public string? ProductType { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Currency { get; set; }
        public bool Summary { get; set; }
        public bool PaidInvoice { get; set; }
        public bool CheckAllCur { get; set; }
        public bool FromDueChkBox { get; set; }
    }

    public class OutstandingReportDropdownsDto
    {
        public List<BasicDataDto> Currency { get; set; } = new();
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Category { get; set; } = new();
        public List<BasicDataDto> Account { get; set; } = new();
        public List<BasicDataDto> ProductType { get; set; } = new();
        public List<SalesPersonOptionDto> SalesPersons { get; set; } = new();
    }

    public class SalesPersonOptionDto
    {
        public string SalesPerson { get; set; } = string.Empty;
    }

    public class OutstandingReportResultDto
    {
        public long Code { get; set; }
        public string? Date { get; set; }
        public string? DueDate { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Category { get; set; }
        public string? Party { get; set; }
        public string? Adat { get; set; }
        public float Weight { get; set; }
        public string? Amount { get; set; }
        public float TotINR { get; set; }
        public float PaidINR { get; set; }
        public string? ProductType { get; set; }
        public float Pcs { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvType { get; set; }
        public string? DueDays { get; set; }
        public string? Totcts { get; set; }
        public string? PartyMobile { get; set; }
        public string? Broker { get; set; }
        public string? BrokerMobile { get; set; }
        public float PaidAmount { get; set; }
        public float ExDiff { get; set; }
        public float RemainAmount { get; set; }
        public float PostDateAmount { get; set; }
        public float ExportAmount { get; set; }
        public string? Seller { get; set; }
        public string? SellerInvType { get; set; }
        public string? Symbol { get; set; }
        public string? Disc { get; set; }
        public string? SellerPartyMobile { get; set; }
        public string? Rate { get; set; }
        public string? Dollarrate { get; set; }
        public string? Remarks { get; set; }
        public string? UserName { get; set; }
        public string? ExpPartyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? TrnType { get; set; }
        /// <summary>MVC Outstanding report column SalesPerson.</summary>
        public string? SalesPerson { get; set; }
    }
}
