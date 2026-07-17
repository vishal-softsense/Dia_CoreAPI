namespace Dia_CoreAPI.DTOs.ParcelDashboard
{
    public class ParcelBestSellerDto
    {
        public decimal YearNumber { get; set; }
        public decimal MonthNumber { get; set; }
        public string Seller { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
    }

    public class ParcelTopBrokerDto
    {
        public string Broker { get; set; } = string.Empty;
        public decimal YearNumber { get; set; }
        public decimal MonthNumber { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal PL { get; set; }
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
        public decimal Cts { get; set; }
        public decimal RecievableUsd { get; set; }
        public decimal RecievableInr { get; set; }
    }

    public class ParcelOverDueInvoiceCountDto
    {
        public decimal OverDueInvoice { get; set; }
    }

    public class ParcelSalesByCountryDto
    {
        public string CountryID { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public decimal YearNumber { get; set; }
        public decimal MonthNumber { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal PurchaseCost { get; set; }
        public decimal PL { get; set; }
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
    }

    public class ParcelTopCustomerDto
    {
        public string Customer { get; set; } = string.Empty;
        public decimal YearNumber { get; set; }
        public decimal MonthNumber { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal PurchaseCost { get; set; }
        public decimal PL { get; set; }
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
        public decimal Cts { get; set; }
        public decimal RecievableUSD { get; set; }
        public decimal RecievableINR { get; set; }
    }

    public class ParcelTopPayableDto
    {
        public string Customer { get; set; } = string.Empty;
        public decimal YearNumber { get; set; }
        public decimal MonthNumber { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal PurchaseCost { get; set; }
        public decimal PL { get; set; }
        public decimal Sales { get; set; }
        public decimal PLPerOnPurAmount { get; set; }
        public decimal Cts { get; set; }
        public decimal RecievableUSD { get; set; }
        public decimal RecievableINR { get; set; }
    }

    public class ParcelOverDueSaleDetailDto
    {
        public int Code { get; set; }
        public int InvNo { get; set; }
        public string InvDate { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public decimal InvoiceAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int OverdueDays { get; set; }
        public decimal OverDueInvoice { get; set; }
    }
}
