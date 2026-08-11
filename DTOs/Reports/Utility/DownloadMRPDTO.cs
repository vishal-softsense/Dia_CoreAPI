namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class DownloadMRPDTO
    {
        public string? Message { get; set; }
    }

    public class RapaportCredentialsDTO
    {
        public string? RapUsername { get; set; }
        public string? RapPassword { get; set; }
    }

    public class RapaportPriceRowDTO
    {
        public string Color { get; set; } = string.Empty;
        public string Clarity { get; set; } = string.Empty;
        public string CaratsFrom { get; set; } = string.Empty;
        public string CaratsTo { get; set; } = string.Empty;
        public string Rate { get; set; } = string.Empty;
    }
}
