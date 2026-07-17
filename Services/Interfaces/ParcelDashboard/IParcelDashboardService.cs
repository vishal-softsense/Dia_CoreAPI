using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;

namespace Dia_CoreAPI.Services.Interfaces.ParcelDashboard
{
    public interface IParcelDashboardService
    {
        Task<List<ParcelBestSellerDto>> GetBestSellerAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopBrokerDto>> GetTopBrokersAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopBrokerDto>> GetAllBrokersAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopCustomersAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllCustomersAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelOverDueInvoiceCountDto>> GetOverDueSaleCountAsync(string companyId);
        Task<List<ParcelOverDueInvoiceCountDto>> GetOverDuePurCountAsync(string companyId);
        Task<List<ParcelSalesByCountryDto>> GetSalesByCountryAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopPayableDto>> GetTopPayableAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopPayableDto>> GetAllPayablesAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopOutstandingAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllOutstandingsAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopOpenMemoAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllOpenMemosAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelOverDueSaleDetailDto>> GetTopOverdueSalesAsync(string month, string year, string tdate, string companyId);
        Task<List<ParcelOverDueSaleDetailDto>> GetAllOverdueSalesAsync(string month, string year, string tdate, string companyId);
    }
}
