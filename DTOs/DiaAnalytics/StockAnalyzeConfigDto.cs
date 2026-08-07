namespace Dia_CoreAPI.DTOs.DiaAnalytics
{
    /// <summary>
    /// Config used to open Diasense Stock Analyze AI inside SoftOnCloud iframe (same as MVC StockAnalyze.cshtml).
    /// </summary>
    public class StockAnalyzeConfigDto
    {
        public string User { get; set; } = string.Empty;
        public string StockUploadAPIKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Full iframe src: BaseUrl?user=...&amp;StockUploadAPIKey=...</summary>
        public string IframeUrl { get; set; } = string.Empty;
    }
}
