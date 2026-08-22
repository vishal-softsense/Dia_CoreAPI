using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class ConvertExcelResultDto
    {
        public bool Success { get; set; } = true;
        public List<Dictionary<string, object>> OriginalData { get; set; } = new();
        public List<Dictionary<string, object>> MappedData { get; set; } = new();
        public string File { get; set; } = string.Empty;
        public string FileName { get; set; } = "MappedExcel.xlsx";
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ConvertExcelUtilityDTO
    {
        public Microsoft.AspNetCore.Http.IFormFile? File { get; set; }
    }
}
