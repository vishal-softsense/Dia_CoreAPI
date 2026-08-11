using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Payable_Receivables
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class SalePurchaseSummaryReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public int InvTypeId { get; set; }
        public string? SubTypeId { get; set; }
        public int PartyCode { get; set; }
        public int ExpPartyCode { get; set; }
        public int BrokerCode { get; set; }
        public int SellerCode { get; set; }
        public int BuyerCode { get; set; }
        public bool Sale { get; set; }
        public bool PUR { get; set; }
        public bool SaleRtn { get; set; }
        public bool PSaleRtn { get; set; }
        public bool PurRtn { get; set; }
        public bool PSALE { get; set; }
        public bool PPUR { get; set; }
        public bool MemoOut { get; set; }
    }

    public class SalePurchaseSummaryReportDropdownsDto
    {
        public List<BasicDataDto> Currency { get; set; } = new();
        public List<BasicDataDto> Type { get; set; } = new();
        public List<BasicDataDto> SubType { get; set; } = new();
        public List<BasicDataDto> Seller { get; set; } = new();
        public List<BasicDataDto> Buyer { get; set; } = new();
    }

    public class SalePurchaseSummaryReportResultDto
    {
        public string? SPDate { get; set; }
        public string? BillNo { get; set; }
        public long Invno { get; set; }
        public string? PartyName { get; set; }
        public double Totcts { get; set; }
        public double Rate { get; set; }
        public double Totamount { get; set; }
        public double Plusminamount { get; set; }
        public double InvAmt { get; set; }
        public double MasterCurrencyFinalAmount { get; set; }
        public double baserate { get; set; }
        public string? InvType { get; set; }
        public string? Sellername { get; set; }
        public int duedays { get; set; }
        public string? Remarks { get; set; }
        public string? ExpParty { get; set; }
        public string? BrokerName { get; set; }
        public string? Subtype { get; set; }
        public string? Status { get; set; }
        public double VatAmt { get; set; }
        public double VatAmtMasterCur { get; set; }
        public double VatPer { get; set; }
    }
}
