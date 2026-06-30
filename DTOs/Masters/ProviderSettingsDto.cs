namespace Dia_CoreAPI.DTOs.Masters
{
    public class ProviderSettings
    {
        public string provider { get; set; }
        public bool enabled { get; set; }
        public string mode { get; set; }

        // API
        public string apiUser { get; set; }
        public string apiPass { get; set; }
        public string apiLink { get; set; }

        // FTP
        public string ftpUser { get; set; }
        public string ftpPass { get; set; }
        public string ftpHost { get; set; }
        public string ftpPort { get; set; }
        public float ftpAmt { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public object? Data { get; set; }
    }
}
