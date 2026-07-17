using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;

namespace Dia_CoreAPI.Services.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<List<BestSellerDto>> GetBestSellerAsync(string month, string year, string tdate, string companyId);
        Task<List<TopBrokerDto>> GetTopBrokersAsync(string month, string year, string tdate, string companyId);
        Task<List<TopCustomerDto>> GetTopCustomersAsync(string month, string year, string tdate, string companyId);
        Task<List<OverDueSaleDto>> GetOverDueSaleAsync(string companyId);
        Task<List<OverDuePurDto>> GetOverDuePurAsync(string companyId);
        Task<List<SalesByCountryDto>> GetSalesByCountryAsync(string month, string year, string tdate, string companyId);
        Task<List<SalePLPerLegendDto>> GetSalePLPerLegendAsync(string companyId);
        Task<List<StoneAgeDto>> GetStoneAgeAsync(string companyId);
        Task<List<RevisedAgeDto>> GetRevisedAgeAsync(string companyId);
    }
}
