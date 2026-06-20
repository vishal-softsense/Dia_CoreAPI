using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.StockAccount
{
    public class ParcelItemMasterDto
    {
        [Key]
        public int ItemCode { get; set; }
        public string? SrNo { get; set; }

        [Required(ErrorMessage = "Enter Item ID/LotNo")]
        public string LotNo { get; set; } = string.Empty;
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? SizeId { get; set; }
        public string? SieveId { get; set; }
        public string? ClarityId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemGroupID { get; set; }
        public string? Category { get; set; }
        public string? Instruction { get; set; } // Matches StockSubType
        public string? ProductType { get; set; } // Matches Grown/Process Type
        public float Weight { get; set; }
        public int Pcs { get; set; }
        public float PDollorRate { get; set; } // BaseRate
        public float PurchaseRate { get; set; } // Pcostusdcarats
        public float PurchaseAmount { get; set; } // Pcostusdamt
        public float MasterCurrencyRate { get; set; } // Pcostinrcarats
        public float MasterCurrencyAmount { get; set; } // Pcostinramt
        public float AskingRate { get; set; } // askingusdcarats
        public float AskingAmount { get; set; } // askingusdamt
        public float AskingINR { get; set; } // askinginrcarats
        public float AskingINRAmt { get; set; } // askinginramt
        public string? PublicCommnets { get; set; } // Description
        public string? StoneTypes { get; set; }
    }

    public class PItemMasterDto
    {
        public int ItemCode { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public string? LAB { get; set; }
        public string? CertificateNo { get; set; }
        public string? ShapeId { get; set; }
        public string? ColorId { get; set; }
        public string? SizeId { get; set; }
        public string? ClarityId { get; set; }
        public string? SieveId { get; set; }
        public float Weight { get; set; }
        public float MRP { get; set; }
        public float PurchaseAmount { get; set; }
    }
}
