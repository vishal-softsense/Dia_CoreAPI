using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.ParcelDashboard
{
    public class ParcelPurchaseAnalysisRequestDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Currency { get; set; }
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public string? Category { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? LotNo { get; set; }
        public List<string>? ShapeGroup { get; set; }
        public List<string>? ColorGroup { get; set; }
        public List<string>? Boxnm { get; set; }
        public List<string>? ItemGroup { get; set; }
        public List<string>? PartyLocation { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? Sieve { get; set; }
        public string? PurAna { get; set; }
    }

    public class ParcelPurchaseAnalysisResultDto
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
        public decimal CaratsPer { get; set; }
        public decimal PurAmtDollar { get; set; }
        public decimal PurDollarPer { get; set; }
        public decimal PurAmtInr { get; set; }
        public decimal PurInrPer { get; set; }
        public decimal DollarPerCarat { get; set; }
        public decimal BaseRate { get; set; }
        public decimal InrPerCarat { get; set; }
    }
}
