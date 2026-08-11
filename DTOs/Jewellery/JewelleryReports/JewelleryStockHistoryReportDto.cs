using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Jewellery.JewelleryReports
{
    public class JewelleryStockHistoryReportFilterDto
    {
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? selectedTypes { get; set; }
        public List<string>? selectedTypesTrn { get; set; }
        public string? LotNo { get; set; }
        public string? type { get; set; }
        public bool Salepur { get; set; }
        public bool Memo { get; set; }
        public bool SalpurAvg { get; set; }
        public bool PurAvg { get; set; }
        public bool MarketAvg { get; set; }
        public bool StkEff { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? RadioValue { get; set; }
    }

    public class JewelleryStockHistoryReportDatatableDto
    {
        public long trncode { get; set; }
        public long invcode { get; set; }
        public int Itemcode { get; set; }
        public string StockID { get; set; } = string.Empty;
        public string TrnDate { get; set; } = string.Empty;
        public string TrnType { get; set; } = string.Empty;
        public string Particulars { get; set; } = string.Empty;

        public decimal StockIn { get; set; }
        public decimal StockInPrice { get; set; }
        public decimal StockInAmount { get; set; }

        public decimal StockOut { get; set; }
        public decimal StockOutPrice { get; set; }
        public decimal StockOutAmount { get; set; }
        public decimal COGS { get; set; }

        public decimal BalanceStock { get; set; }
        public decimal BalancePrice { get; set; }
        public decimal BalanceAmount { get; set; }
    }
}
