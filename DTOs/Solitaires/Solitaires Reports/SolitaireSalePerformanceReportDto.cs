using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireSalePerformanceReportFilterDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? Show { get; set; }
        public string? HoldUnHold { get; set; }
        public string? StoneTypes { get; set; }
        public string? Status { get; set; }
        public string? StockPrices { get; set; }
        public string? ExpParty { get; set; }
        public string? LotNo { get; set; }

        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Polish { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Fluorescence { get; set; }
        public List<string>? Milky { get; set; }
        public List<string>? Luster { get; set; }
        public List<string>? Natts { get; set; }
        public List<string>? Mines { get; set; }
        public List<string>? ProductType { get; set; }
        public List<string>? Sieve { get; set; }
    }

    public class SolitaireSalePerformanceReportRowDto
    {
        public string? LotNo { get; set; }
        public string? Status { get; set; }
        public string? CertId { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? ClarityId { get; set; }
        public string? PropId { get; set; }
        public string? PolishId { get; set; }
        public string? SymId { get; set; }
        public double PCarats { get; set; }
        public string? ConsDate { get; set; }
        public double ProcessDays { get; set; }
        public string? PDate { get; set; }
        public double PurPrice { get; set; }
        public double PurDollar { get; set; }
        public double PurBaseRate { get; set; }
        public double PurRs { get; set; }
        public double CertExpdollar { get; set; }
        public double CertExpRs { get; set; }
        public double PurCostdollar { get; set; }
        public double PurCostRs { get; set; }
        public string? SDueDate { get; set; }
        public double SaleDays { get; set; }
        public double SalePrice { get; set; }
        public double Saledollar { get; set; }
        public double BaseRate { get; set; }
        public double SaleRs { get; set; }
        public double PLdollar { get; set; }
        public double PLdollarper { get; set; }
        public double PLRs { get; set; }
        public double PLRsper { get; set; }
        public double Adatdollar { get; set; }
        public double AdatRs { get; set; }
        public double Brokeragedollar { get; set; }
        public double BrokerageRs { get; set; }
        public double TotPurCostdollar { get; set; }
        public double FinalPLdollar { get; set; }
        public double FinalPLdollarper { get; set; }
        public double TotPurCostRs { get; set; }
        public double FinalPLRs { get; set; }
        public double FinalPLRsper { get; set; }
        public double Interestdollar { get; set; }
        public double TotPurCostdollarWithInt { get; set; }
        public double FinalPLdollarAfterInt { get; set; }
        public double InterestRs { get; set; }
        public double TotPurCostRsWithInt { get; set; }
        public double FinalPLRsAfterInt { get; set; }
        public string? Itemcode { get; set; }
        public string? KapanNo { get; set; }
        public string? SizeId { get; set; }
    }
}
