using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;

namespace Dia_CoreAPI.Repositories.Interfaces.Dashboard
{
    public interface IDashboardRepository
    {
        Task<List<BestSellerDto>> GetBestSellerAsync(string where, string companyId);
        Task<List<TopBrokerDto>> GetTopBrokersAsync(string where, string companyId);
        Task<List<TopCustomerDto>> GetTopCustomersAsync(string where, string companyId);
        Task<List<OverDueSaleDto>> GetOverDueSaleAsync(string companyId);
        Task<List<OverDuePurDto>> GetOverDuePurAsync(string companyId);
        Task<List<SalesByCountryDto>> GetSalesByCountryAsync(string where, string companyId);
        Task<List<SalePLPerLegendDto>> GetSalePLPerLegendAsync(string companyId);
        Task<List<StoneAgeDto>> GetStoneAgeAsync(string companyId);
        Task<List<RevisedAgeDto>> GetRevisedAgeAsync(string companyId);
    }
}
