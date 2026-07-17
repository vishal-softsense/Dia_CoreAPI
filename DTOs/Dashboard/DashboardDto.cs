namespace Dia_CoreAPI.DTOs.Dashboard
{
    public class BestSellerDto
    {
        public string Seller { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal PLPerOnPurAmount { get; set; }
    }

    public class TopBrokerDto
    {
        public string Broker { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
    }

    public class TopCustomerDto
    {
        public string Customer { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
    }

    public class OverDueSaleDto
    {
        public string OverDueInvoice { get; set; } = string.Empty;
    }

    public class OverDuePurDto
    {
        public string OverDueInvoice { get; set; } = string.Empty;
    }

    public class SalesByCountryDto
    {
        public string CountryName { get; set; } = string.Empty;
        public decimal PL { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
        public decimal Sales { get; set; }
    }

    public class SalePLPerLegendDto
    {
        public string SalePLPerLegend { get; set; } = string.Empty;
        public decimal TotPcs { get; set; }
    }

    public class StoneAgeDto
    {
        public string StoneAge { get; set; } = string.Empty;
        public decimal TotPcs { get; set; }
    }

    public class RevisedAgeDto
    {
        public string RevisedAge { get; set; } = string.Empty;
        public decimal TotPcs { get; set; }
    }
}
