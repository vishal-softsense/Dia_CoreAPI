using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.FinancialTransaction.Loan
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class LoanListItemDto
    {
        public int Code { get; set; }
        public string? ConDate { get; set; }
        public string? Accgrpname { get; set; }
        public string? FromToAccount { get; set; }
        public double Amount { get; set; }
        public string? Cur { get; set; }
    }

    public class LoanPageDataDto
    {
        public List<BasicDataDto> CurrencyList { get; set; } = new();
        public string? DefaultCurrencyCode { get; set; }
        public float MBaseRate { get; set; }
        public float SBaseRate { get; set; }
    }

    public class LoanDetailDto
    {
        public int Code { get; set; }
        public int InvNo { get; set; }
        public DateTime Date { get; set; }
        public DateTime RenewDate { get; set; }
        public DateTime Due_Date { get; set; }
        public int DueDays { get; set; }
        public int Party { get; set; }
        public string? PartyName { get; set; }
        public int FTParty { get; set; }
        public string? FTPartyName { get; set; }
        public int Currency { get; set; }
        public float MBaseRate { get; set; }
        public float SBaseRate { get; set; }
        public int Broker { get; set; }
        public string? BrokerName { get; set; }
        public float Brokerper { get; set; }
        public float Amount { get; set; }
        public float InterestPer { get; set; }
        public int DivideDays { get; set; }
        public bool OutStanding { get; set; }
        public string? Disc { get; set; }
    }

    public class LoanSaveRequest
    {
        public int Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime RenewDate { get; set; }
        public DateTime Due_Date { get; set; }
        public int DueDays { get; set; }
        public int InvNo { get; set; }
        public int Party { get; set; }
        public int Broker { get; set; }
        public int FTParty { get; set; }
        public float Currency { get; set; }
        public float MBaseRate { get; set; }
        public float SBaseRate { get; set; }
        public float Brokerper { get; set; }
        public float Amount { get; set; }
        public float InterestPer { get; set; }
        public int DivideDays { get; set; }
        public string? Disc { get; set; }
        public bool OutStanding { get; set; }
    }

    public class LoanDeleteRequest
    {
        public int Id { get; set; }
    }

    internal class LoanSalepurRow
    {
        public int Code { get; set; }
        public string? ConDate { get; set; }
        public string? AccGrpName { get; set; }
        public string? FromToAccount { get; set; }
        public double TotAmount { get; set; }
        public string? Cur { get; set; }
        public int InvNo { get; set; }
        public DateTime TrnDate { get; set; }
        public DateTime RoundDate { get; set; }
        public DateTime DueDate { get; set; }
        public int CurCode { get; set; }
        public string? PartyCode { get; set; }
        public string? ExpPartyCode { get; set; }
        public string? BrokerCode { get; set; }
        public string? BrokerName { get; set; }
        public double MasterCurrencyRate { get; set; }
        public double StockCurrencyRate { get; set; }
        public float Brokerper { get; set; }
        public float InterestPer { get; set; }
        public int DivideDays { get; set; }
        public bool OutStanding { get; set; }
        public bool Outstanding { get; set; }
        public int Duedays { get; set; }
    }
}
