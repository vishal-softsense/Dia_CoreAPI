using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports
{
    public class SolitaireHoldUnholdReportFilterDto
    {
        public string? Date { get; set; }
        public string? Show { get; set; }
        public string? HoldUnHold { get; set; }
        public string? StoneTypes { get; set; }
        public string? Status { get; set; }
        public string? LotNo { get; set; }
        public float BackPer { get; set; }
        public float BackPlusMinus { get; set; }
        public float RatePer { get; set; }
        public float RatePlusMinus { get; set; }
    }

    public class SolitaireHoldUnholdActionDto
    {
        /// <summary>Quoted lot list as sent by MVC, e.g. "'LOT1','LOT2'".</summary>
        public string? LotNo { get; set; }
        /// <summary>"hold" or "unhold".</summary>
        public string? Type { get; set; }
    }

    public class SolitaireHoldUnholdReportRowDto
    {
        public string? CurStatus { get; set; }
        public string? LotNo { get; set; }
        public string? Shape { get; set; }
        public string? Color { get; set; }
        public string? Clarity { get; set; }
        public decimal Carats { get; set; }
        public decimal Rapa { get; set; }
        public decimal Rate { get; set; }
        public decimal Back { get; set; }
        public decimal Amount { get; set; }
        public string? Prop { get; set; }
        public string? Pol { get; set; }
        public string? Sym { get; set; }
        public string? FL { get; set; }
        public string? Diam { get; set; }
        public decimal MRPAmount { get; set; }
        public decimal AskingBack { get; set; }
        public decimal AskingRate { get; set; }
        public decimal AskingAmount { get; set; }
        public string? LAB { get; set; }
        public string? CNo { get; set; }
        public string? HA { get; set; }
        public string? Nts { get; set; }
        public decimal Partyholdcts { get; set; }
        public string? ML { get; set; }
        public string? KeyToSymbols { get; set; }
        public string? Comments { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public string? PagePath { get; set; }
        /// <summary>MVC holdres.js media eye link uses row.view (maps from PagePath).</summary>
        public string? view { get; set; }
        public string? NotShow { get; set; }
        public string? CertPath { get; set; }
        public string? ItemName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
