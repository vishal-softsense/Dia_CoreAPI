using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class MasterPriceComparisonFilterDto
    {
        public List<string>? LAB { get; set; }
        public List<string>? Shape { get; set; }
        public List<string>? Size { get; set; }
        public List<string>? CPS { get; set; }
        public List<string>? Fluorescence { get; set; }
    }

    public class MasterPriceComparisonPageDto
    {
        public List<MasterPriceComparisonDropdownOptionDto> LAB { get; set; } = new();
        public List<MasterPriceComparisonDropdownOptionDto> Shape { get; set; } = new();
        public List<MasterPriceComparisonDropdownOptionDto> Size { get; set; } = new();
        public List<MasterPriceComparisonDropdownOptionDto> Fluorescence { get; set; } = new();
        public List<MasterPriceComparisonDropdownOptionDto> CPS { get; set; } = new();
    }

    public class MasterPriceComparisonDropdownOptionDto
    {
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Matches MVC MasterPriceComparisonReport (SP_MasterPriceComparison Type=Report).</summary>
    public class MasterPriceComparisonReportDto
    {
        public string CertId { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string CPSID { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Flou { get; set; } = string.Empty;

        public decimal IF { get; set; }
        public decimal VVS1 { get; set; }
        public decimal VVS2 { get; set; }
        public decimal VS1 { get; set; }
        public decimal VS2 { get; set; }
        public decimal SI1 { get; set; }
        public decimal SI2 { get; set; }
        public decimal SI3 { get; set; }
        public decimal I1 { get; set; }
        public decimal I2 { get; set; }
        public decimal I3 { get; set; }

        public decimal DMRP_IF { get; set; }
        public decimal DMRP_VVS1 { get; set; }
        public decimal DMRP_VVS2 { get; set; }
        public decimal DMRP_VS1 { get; set; }
        public decimal DMRP_VS2 { get; set; }
        public decimal DMRP_SI1 { get; set; }
        public decimal DMRP_SI2 { get; set; }
        public decimal DMRP_SI3 { get; set; }
        public decimal DMRP_I1 { get; set; }
        public decimal DMRP_I2 { get; set; }
        public decimal DMRP_I3 { get; set; }

        public int ColorPriority { get; set; }
        public int ClarityFlag { get; set; }
        public int ColorFlag { get; set; }
    }

    /// <summary>Kept for file naming consistency with other Utility DTOs.</summary>
    public class MasterPriceComparisonDTO
    {
        public string? Message { get; set; }
    }
}
