using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class GradingComparisonReportFilterDto
    {
        public List<string>? Shape { get; set; }
        public List<string>? Color { get; set; }
        public List<string>? Clarity { get; set; }
        public List<string>? Size { get; set; }
        public string? LotNo { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public List<string>? Proportion { get; set; }
        public List<string>? Symmetry { get; set; }
        public List<string>? LAB { get; set; }
        public List<string>? Fluorescence { get; set; }
        public List<string>? Polish { get; set; }
        /// <summary>When true, search returns Lab Weight Difference rows derived from grading comparison pairs.</summary>
        public bool LabWeightDifference { get; set; }
    }

    public class LabWeightDiffRowDto
    {
        public string? LotNo { get; set; }
        public decimal OldWeight { get; set; }
        public decimal NewWeight { get; set; }
        public decimal Difference { get; set; }
    }

    public class LabWeightDiffEntryRowDto
    {
        public string? LotNo { get; set; }
        public decimal OldWeight { get; set; }
        public decimal NewWeight { get; set; }
    }

    public class MakeLabWeightDiffEntryRequestDto
    {
        public bool UseLabWeightDate { get; set; }
        public DateTime? LabWeightDate { get; set; }
        public List<LabWeightDiffEntryRowDto>? Rows { get; set; }
    }

    public class MakeLabWeightDiffEntryResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InsertedCount { get; set; }
    }

    public class GradingComparisonReportRowDto
    {
        public string? Ref { get; set; }
        public int sr { get; set; }
        public string? LotNo { get; set; }
        public string? Kapan { get; set; }
        public string? PacketNo { get; set; }
        public string? ReportDate { get; set; }
        public string? GDate { get; set; }
        public string? Grader { get; set; }
        public string? carats { get; set; }
        public string? rate { get; set; }
        public string? amount { get; set; }
        public string? MSP { get; set; }
        public string? Back { get; set; }
        public string? Shape { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public string? Proportion { get; set; }
        public string? Polish { get; set; }
        public string? Symm { get; set; }
        public string? FLCode { get; set; }
        public string? HACode { get; set; }
        public string? Diameter { get; set; }
        public string? Depth { get; set; }
        public string? Table { get; set; }
        public string? CH { get; set; }
        public string? CA { get; set; }
        public string? GerdleMin { get; set; }
        public string? Girdle { get; set; }
        public string? PA { get; set; }
        public string? PH { get; set; }
        public string? Culet { get; set; }
        public string? MilkyCode { get; set; }
        public string? Brown { get; set; }
        public string? LusterCode { get; set; }
        public string? Natts { get; set; }
        public string? TableInclusion { get; set; }
        public string? CrownInclusion { get; set; }
        public string? IP { get; set; }
        public string? CertificateNo { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? ReportComments { get; set; }
        public string? LaserInscriptionText { get; set; }
        public string? ControlNo { get; set; }
        public string? SuratRemarks { get; set; }
        public string? IGICut { get; set; }
        public string? Buth { get; set; }
        public string? ClarityStatus { get; set; }
        public string? HRDcut { get; set; }
        public string? Fees { get; set; }

        public string? ColorCarats { get; set; }
        public string? ColorColor { get; set; }
        public string? ClarityColor { get; set; }
        public string? PropColor { get; set; }
        public string? PolishColor { get; set; }
        public string? SymColor { get; set; }
        public string? FLCodeColor { get; set; }
        public string? DepthColor { get; set; }
        public string? TableColor { get; set; }
        public string? CAColor { get; set; }
        public string? CHColor { get; set; }
        public string? PAColor { get; set; }
        public string? PHColor { get; set; }
    }
}
