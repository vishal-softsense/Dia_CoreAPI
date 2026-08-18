using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class GetLabDataDTO
    {
        public IFormFile? ImportFile { get; set; }
    }

    public class GetLabLotRequestDto
    {
        public string? LotNo { get; set; }
    }

    public class GetLabLotDto
    {
        public string LotNo { get; set; } = string.Empty;
        public string CertificateNo { get; set; } = string.Empty;
        public float Carats { get; set; }
    }

    public class GetCertItemDto
    {
        public string? GetCertNo { get; set; }
        public float Carats { get; set; }
        public string? Lotno { get; set; }
    }

    public class GetCertRequestDto
    {
        public List<GetCertItemDto>? Item { get; set; }
    }

    public class LabParameterDto
    {
        public string ParameterID { get; set; } = string.Empty;
    }

    public class GiaDataDto
    {
        public int SrNo { get; set; }
        public string depth_pct { get; set; } = string.Empty;
        public string table_pct { get; set; } = string.Empty;
        public string crown_angle { get; set; } = string.Empty;
        public string crown_height { get; set; } = string.Empty;
        public string pavilion_angle { get; set; } = string.Empty;
        public string pavilion_depth { get; set; } = string.Empty;
        public string star_length { get; set; } = string.Empty;
        public string lower_half { get; set; } = string.Empty;
        public string girdle { get; set; } = string.Empty;
        public string culet { get; set; } = string.Empty;
        public string characteristic { get; set; } = string.Empty;
        public string shape_and_cutting_style { get; set; } = string.Empty;
        public string measurements { get; set; } = string.Empty;
        public string L { get; set; } = string.Empty;
        public string W { get; set; } = string.Empty;
        public string D { get; set; } = string.Empty;
        public string carat_weight { get; set; } = string.Empty;
        public string color_grade { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string clarity_grade { get; set; } = string.Empty;
        public string cut_grade { get; set; } = string.Empty;
        public string polish { get; set; } = string.Empty;
        public string symmetry { get; set; } = string.Empty;
        public string fluorescence { get; set; } = string.Empty;
        public string clarity_characteristics { get; set; } = string.Empty;
        public string inscriptions { get; set; } = string.Empty;
        public string report_number { get; set; } = string.Empty;
        public string report_date { get; set; } = string.Empty;
        public string report_type { get; set; } = string.Empty;
        public string color_origin { get; set; } = string.Empty;
        public string color_distribution { get; set; } = string.Empty;
        public string country_of_origin { get; set; } = string.Empty;
        public string diamond_type { get; set; } = string.Empty;
        public string report_comments { get; set; } = string.Empty;
        public string key_to_symbols { get; set; } = string.Empty;
        public string proportions { get; set; } = string.Empty;
        public string lotno { get; set; } = string.Empty;
        public string Cert { get; set; } = string.Empty;
    }

    public class IgiDataDto
    {
        public int SrNo { get; set; }
        public string ReportNumber { get; set; } = string.Empty;
        public string ReportDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Clarity { get; set; } = string.Empty;
        public string CutProp { get; set; } = string.Empty;
        public string Sym { get; set; } = string.Empty;
        public string Pol { get; set; } = string.Empty;
        public string TableDiameter { get; set; } = string.Empty;
        public string CrownHeight { get; set; } = string.Empty;
        public string CrownAngel { get; set; } = string.Empty;
        public string PavilionDepth { get; set; } = string.Empty;
        public string PavilionAngel { get; set; } = string.Empty;
        public string GirdleName { get; set; } = string.Empty;
        public string CuletSize { get; set; } = string.Empty;
        public string TotalDepth { get; set; } = string.Empty;
        public string Fluorescence { get; set; } = string.Empty;
        public string Inscription { get; set; } = string.Empty;
        public string Measurement1 { get; set; } = string.Empty;
        public string Measurement2 { get; set; } = string.Empty;
        public string Measurement3 { get; set; } = string.Empty;
        public string Cert { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
    }

    public class GetLabDataExcelRequestDto
    {
        public List<GiaDataDto>? spte { get; set; }
        public List<IgiDataDto>? ptse { get; set; }
    }

    public class GetLabDataResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
        public bool Error { get; set; }
        public List<string>? Headers { get; set; }
        public List<List<object?>>? Rows { get; set; }

        public GetLabDataResult() { }

        public GetLabDataResult(bool success, object? data, string? message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }
    }
}
