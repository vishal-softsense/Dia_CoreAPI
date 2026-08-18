using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class ImportPricingDTO
    {
        public IFormFile? ImportFile { get; set; }
    }

    public class ImportPricingExcelDto
    {
        [JsonPropertyName("LotNo")]
        public string LotNo { get; set; } = string.Empty;

        [JsonPropertyName("rate")]
        public double rate { get; set; }
    }

    public class PricingTypeDto
    {
        [JsonPropertyName("Pricingtype")]
        public string Pricingtype { get; set; } = string.Empty;
    }

    public class ImportPricingRowDto
    {
        public string LotNo { get; set; } = string.Empty;
        public double rate { get; set; }
    }

    public class SavePricingRequestDto
    {
        [JsonPropertyName("Pr")]
        public PricingTypeDto? Pr { get; set; }

        [JsonPropertyName("Imp")]
        public List<ImportPricingRowDto>? Imp { get; set; }
    }

    public class ImportPricingResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }

        public ImportPricingResult() { }

        public ImportPricingResult(bool success, object? data, string? message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }
    }
}
