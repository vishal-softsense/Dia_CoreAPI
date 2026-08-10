using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireLotAssortmentReportFilterDto
    {
        public List<string>? FLotList { get; set; }
    }

    public class SolitaireLotAssortmentReportRowDto
    {
        public string? LotNo { get; set; }
        public string? FromLotCode { get; set; }
        public string? Shape { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Clarity { get; set; }
        public string? TransferDate { get; set; }
        public string? FromLotNo { get; set; }
        public string? ToLotNo { get; set; }
        public double Carats { get; set; }
        public double Rate { get; set; }
        public double AmountCl { get; set; }
    }

    public class SolitaireLotAssortmentLotGroupDto
    {
        public string LotNo { get; set; } = "Unknown";
        public List<SolitaireLotAssortmentReportRowDto> Data { get; set; } = new();
    }
}
