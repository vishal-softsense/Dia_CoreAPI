namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class ImportGradingDTO
    {
        public Microsoft.AspNetCore.Http.IFormFile? ImportFile { get; set; }
        public bool ChkReplace { get; set; }
    }

    public class GradingListDto
    {
        public string LotNo { get; set; } = string.Empty;
        public int ItemCode { get; set; }
        public string MfgSeriesID { get; set; } = string.Empty;
        public string MfgPacketID { get; set; } = string.Empty;
        public float Weight { get; set; }
        public float AskingRate { get; set; }
        public float ActualRate { get; set; }
        public int itemsrno { get; set; }
        public string SizeId { get; set; } = string.Empty;
        public float Back { get; set; }
        public float ActualBack { get; set; }
        public float MRP { get; set; }
        public string MilkyId { get; set; } = string.Empty;
        public string LusterId { get; set; } = string.Empty;
        public string NattsId { get; set; } = string.Empty;
        public string BrownId { get; set; } = string.Empty;
        public double GirdleMInMS { get; set; }
        public double GirdleMaxMS { get; set; }
        public string OIT { get; set; } = string.Empty;
        public string OIG { get; set; } = string.Empty;
        public string OIC { get; set; } = string.Empty;
        public string OIP { get; set; } = string.Empty;
        public string TableInclusion { get; set; } = string.Empty;
        public string CrownInclusion { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string HAID { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string IGICut { get; set; } = string.Empty;
        public string ButhId { get; set; } = string.Empty;
        public string HRDCut { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;
        public string PagePath { get; set; } = string.Empty;
    }

    public class ImportGradingResult
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public string? Message { get; set; }
        public string? Path { get; set; }
        public string? Mode { get; set; }

        public ImportGradingResult() { }

        public ImportGradingResult(bool success, string? data, string? message = null, string? path = null, string? mode = null)
        {
            Success = success;
            Data = data;
            Message = message;
            Path = path;
            Mode = mode;
        }
    }

    public class SettingMasterItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class GetParameterValueDto
    {
        public string ParameterValue { get; set; } = string.Empty;
    }
}
