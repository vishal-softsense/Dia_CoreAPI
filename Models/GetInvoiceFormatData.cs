namespace Dia_CoreAPI.Models
{
    public class GetInvoiceFormatData
    {
        public int Id { get; set; }
        public string FormateName { get; set; } = string.Empty;
        public string CrReportName { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
