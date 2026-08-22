using System;
using System.Collections.Generic;

namespace Dia_CoreAPI.DTOs.Reports.Utility
{
    public class StockTransferListDto
    {
        public int Code { get; set; }
        public string SPdate { get; set; } = string.Empty;
        public string FLot { get; set; } = string.Empty;
        public string Fname { get; set; } = string.Empty;
        public double FItemRate { get; set; }
        public double carats { get; set; }
        public string TLot { get; set; } = string.Empty;
        public string Tname { get; set; } = string.Empty;
        public double TItemRate { get; set; }
    }

    public class StockTransferSaveDto
    {
        public DateTime TrnDate { get; set; }
        public string? Fromlotno { get; set; }
        public int CompanyID { get; set; }
        public string? FItemName { get; set; }
        public double FromPrice { get; set; }
        public double Carats { get; set; }
        public string? Tolotno { get; set; }
        public string? TItemName { get; set; }
        public double ToPrice { get; set; }
    }

    public class StockTranDataDto
    {
        public string LotNo { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public float BalanceCarats3Dec { get; set; }
        public float PurchaseRate { get; set; }
    }

    public class StockTransferLotRequestDto
    {
        public string? LotNo { get; set; }
    }

    /// <summary>Kept for file naming consistency with other Utility DTOs.</summary>
    public class StockTransferDTO
    {
        public string? Message { get; set; }
    }
}
