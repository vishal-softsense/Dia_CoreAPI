using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs
{
    public class StockRegisterReportFilterDto
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        /// <summary>Legacy price mode string (e.g. marketprice, sale-pur-avg-price).</summary>
        public string RadioValue { get; set; }

        public string LotNo { get; set; }

        /// <summary>When false, only StockEffected = 1 rows are included.</summary>
        public bool StkEff { get; set; }

        /// <summary>UI checkbox: Show Previous Year (reserved / passed through).</summary>
        public bool ShowPrevYear { get; set; }

        // Price mode flags from React UI (preferred over RadioValue when set)
        public bool SalpurAvg { get; set; }
        public bool PurAvg { get; set; }
        public bool MarketAvg { get; set; }
        public bool COGS { get; set; }

        public List<string> SelectedTypes { get; set; } = new();
        public List<string> SelectedTypesTrn { get; set; } = new();

        public List<string> Shape { get; set; } = new();
        public List<string> Color { get; set; } = new();
        public List<string> Size { get; set; } = new();
        public List<string> Clarity { get; set; } = new();
    }

    public class ParcelStockDropdownDto
    {
        public List<BasicDataDto> Shape { get; set; } = new();
        public List<BasicDataDto> Color { get; set; } = new();
        public List<BasicDataDto> Size { get; set; } = new();
        public List<BasicDataDto> Clarity { get; set; } = new();
    }

    public class BasicDataDto
    {
        public string ID { get; set; }

        public string Type { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string TextName { get; set; }
        public object CityName { get; internal set; }
    }

    public class PermissionCheck
    {
        public int Id { get; set; }
        public bool InsertData { get; set; }
        public bool EditData { get; set; }
        public bool DeleteData { get; set; }
        public bool NavigateData { get; set; }
        public bool SelectData { get; set; }
    }

    /// <summary>Raw SP row shape for SP_StockRegisterReport.</summary>
    public class ParcelStockRegisterReportDatatable
    {
        public long trncode { get; set; }
        public long invcode { get; set; }
        public int Itemcode { get; set; }
        public string StockID { get; set; }
        public string TrnDate { get; set; }
        public string TrnType { get; set; }
        public string Particulars { get; set; }

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

        /// <summary>MVC Invno — invoice/memo number from SP_StockRegisterReport.</summary>
        public int Invno { get; set; }
    }

    /// <summary>Response shape expected by ParcelStockRegisterReport.js (mirrors Solitaire Stock History).</summary>
    public class ParcelStockRegisterReportResponseDto
    {
        public string TrnDate { get; set; }
        public string Trntype { get; set; }
        /// <summary>MVC Invno — displayed as InvNo column.</summary>
        public int Invno { get; set; }
        public string Perticulors { get; set; }
        public string LotNo { get; set; }

        public decimal? InCts { get; set; }
        public decimal? InRate { get; set; }
        public decimal? InAmount { get; set; }

        public decimal? OutCts { get; set; }
        public decimal? OutRate { get; set; }
        public decimal? OutAmount { get; set; }

        public decimal? COGS { get; set; }

        public decimal? BalCts { get; set; }
        public decimal? BalRate { get; set; }
        public decimal? BalAmount { get; set; }

        public string invcode { get; set; }
    }
}
