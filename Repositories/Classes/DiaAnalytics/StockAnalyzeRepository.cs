using Dia_CoreAPI.DTOs.DiaAnalytics;
using Dia_CoreAPI.Repositories.Interfaces.DiaAnalytics;
using Microsoft.Extensions.Configuration;

namespace Dia_CoreAPI.Repositories.Classes.DiaAnalytics
{
    public class StockAnalyzeRepository : IStockAnalyzeRepository
    {
        private readonly IConfiguration _configuration;

        public StockAnalyzeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public StockAnalyzeConfigDto GetStockAnalyzeConfig()
        {
            var section = _configuration.GetSection("Diasense:StockAnalyze");

            var user = section["User"] ?? "";
            var apiKey = section["StockUploadAPIKey"] ?? "";
            var baseUrl = section["BaseUrl"]
                ?? "https://www.diasense.com/DiasenseAI/StockAnalyzeAI.aspx";

            var iframeUrl = string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(apiKey)
                ? ""
                : $"{baseUrl.TrimEnd('?', '&')}?user={user.Trim()}&StockUploadAPIKey={apiKey.Trim()}";

            return new StockAnalyzeConfigDto
            {
                User = user,
                StockUploadAPIKey = apiKey,
                BaseUrl = baseUrl,
                IframeUrl = iframeUrl
            };
        }
    }
}
