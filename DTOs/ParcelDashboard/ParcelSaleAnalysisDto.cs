using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.ParcelDashboard
{
    public class ParcelSaleAnalysisRequestDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
        public List<string>? ShapeGroup { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? ColorGroup { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Sieve { get; set; }
        public List<string>? Boxnm { get; set; }
        public List<string>? PartyLocation { get; set; }
        public List<string>? ItemGroup { get; set; }
        public string? SaleAna { get; set; }
    }

    public class ParcelSaleAnalysisResultDto
    {
        public string? GroupName { get; set; }
        public string? Shape { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Sieve { get; set; }
        public string? Location { get; set; }
        public string? PartyLocation { get; set; }
        public string? StockID { get; set; }
        public string? Month { get; set; }
        public decimal Pcs { get; set; }
        public decimal PcsPer { get; set; }
        public decimal Carats { get; set; }
        public decimal Caratsper { get; set; }
        public decimal SaleAmtdollar { get; set; }
        public decimal Saleperdollar { get; set; }
        public decimal PLdollar { get; set; }
        public decimal DollarPLper { get; set; }
        public decimal COGSdollar { get; set; }
        public decimal PLperOnCOGSAmtdollar { get; set; }
        public decimal SaleAmtINR { get; set; }
        public decimal Saleper { get; set; }
        public decimal PLINR { get; set; }
        public decimal INRPLper { get; set; }
        public decimal COGSINR { get; set; }
        public decimal PLperOnCOGSAmtINR { get; set; }
        public decimal AvgPrc { get; set; }
    }

    public class ParcelSalePivotDto
    {
        public string? LotNo { get; set; }
        public string? Shape { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Sieve { get; set; }
        public string? Location { get; set; }
        public string? PartyLocation { get; set; }
        public string? ItemGroupId { get; set; }
        public string? BoxName { get; set; }
        public decimal Pcs { get; set; }
        public decimal Carats { get; set; }
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Back { get; set; }
    }
}
