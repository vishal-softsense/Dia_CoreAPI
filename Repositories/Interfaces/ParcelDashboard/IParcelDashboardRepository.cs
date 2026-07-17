using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;

namespace Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard
{
    public interface IParcelDashboardRepository
    {
        Task<List<ParcelBestSellerDto>> GetBestSellerAsync(string where, string companyId);
        Task<List<ParcelTopBrokerDto>> GetTopBrokersAsync(string where, string companyId);
        Task<List<ParcelTopBrokerDto>> GetAllBrokersAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopCustomersAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllCustomersAsync(string where, string companyId);
        Task<List<ParcelOverDueInvoiceCountDto>> GetOverDueSaleCountAsync(string companyId);
        Task<List<ParcelOverDueInvoiceCountDto>> GetOverDuePurCountAsync(string companyId);
        Task<List<ParcelSalesByCountryDto>> GetSalesByCountryAsync(string where, string companyId);
        Task<List<ParcelTopPayableDto>> GetTopPayableAsync(string where, string companyId);
        Task<List<ParcelTopPayableDto>> GetAllPayablesAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopOutstandingAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllOutstandingsAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetTopOpenMemoAsync(string where, string companyId);
        Task<List<ParcelTopCustomerDto>> GetAllOpenMemosAsync(string where, string companyId);
        Task<List<ParcelOverDueSaleDetailDto>> GetTopOverdueSalesAsync(string where, string companyId);
        Task<List<ParcelOverDueSaleDetailDto>> GetAllOverdueSalesAsync(string where, string companyId);
    }
}
