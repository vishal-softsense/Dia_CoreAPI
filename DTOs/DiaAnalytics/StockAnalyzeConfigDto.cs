namespace Dia_CoreAPI.DTOs.DiaAnalytics
{
    public class StockAnalyzeConfigDto
    {
        public string User { get; set; } = string.Empty;
        public string StockUploadAPIKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;

        public string IframeUrl { get; set; } = string.Empty;
    }
}
