using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    // Request model for Solitaire Stock History (MVC: Stock Register Report)
    public class SolitaireStockHistoryReportDto
    {
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }

        public List<string>? SelectedTypes { get; set; }

        public string? LotNo { get; set; }

        // Date range (yyyy-MM-dd)
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        // Pricing mode
        public bool SalpurAvg { get; set; }
        public bool PurAvg { get; set; }
        public bool MarketAvg { get; set; }
    }

    // Direct mapping of GetStockRegister result-set columns.
    // Column names are based on MVC's ReportController StockRegisterssrs row mapping:
    // Trndate, Lotnosearch, RelatedRemark, InCts, InRate, InAmount, OutCts, OutRate, OutAmount, BalCts, BalRate, BalAmount, Trntype, invcode
    public class SolitaireStockHistoryReportSpRowDto
    {
        public DateTime? Trndate { get; set; }
        public string? Lotnosearch { get; set; }
        public string? RelatedRemark { get; set; }

        public decimal? InCts { get; set; }
        public decimal? InRate { get; set; }
        public decimal? InAmount { get; set; }

        public decimal? OutCts { get; set; }
        public decimal? OutRate { get; set; }
        public decimal? OutAmount { get; set; }

        public decimal? BalCts { get; set; }
        public decimal? BalRate { get; set; }
        public decimal? BalAmount { get; set; }

        public string? Trntype { get; set; }
        public string? Invcode { get; set; }
    }

    // Response model returned to React UI.
    public class SolitaireStockHistoryReportResponseDTO
    {
        public string? TrnDate { get; set; }
        public string? Trntype { get; set; }
        public string? Perticulors { get; set; }

        public string? LotNo { get; set; }

        public decimal? InCts { get; set; }
        public decimal? InRate { get; set; }
        public decimal? InAmount { get; set; }

        public decimal? OutCts { get; set; }
        public decimal? OutRate { get; set; }
        public decimal? OutAmount { get; set; }

        public decimal? BalCts { get; set; }
        public decimal? BalRate { get; set; }
        public decimal? BalAmount { get; set; }

        public string? invcode { get; set; }
    }
}
