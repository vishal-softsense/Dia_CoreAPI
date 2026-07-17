using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;

namespace Dia_CoreAPI.Services.Classes.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        private string BuildWhereClause(string month, string year, string tdate)
        {
            string where = "";
            if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(tdate))
            {
                where = $" and Month(tspt.SPDate)={month} and Year(tspt.SPDate)={year} and Day(tspt.SPDate)={tdate}";
            }
            else if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year) && string.IsNullOrEmpty(tdate))
            {
                where = $" and Month(tspt.SPDate)={month} and Year(tspt.SPDate)={year}";
            }
            else if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(tdate) && string.IsNullOrEmpty(year))
            {
                where = $" and Month(tspt.SPDate)={month} and Day(tspt.SPDate)={tdate}";
            }
            else if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(tdate) && string.IsNullOrEmpty(month))
            {
                where = $" and Year(tspt.SPDate)={year} and Day(tspt.SPDate)={tdate}";
            }
            else if (!string.IsNullOrEmpty(year))
            {
                where = $" and Year(tspt.SPDate)={year}";
            }
            return where;
        }

        public async Task<List<BestSellerDto>> GetBestSellerAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _dashboardRepository.GetBestSellerAsync(where, companyId);
        }

        public async Task<List<TopBrokerDto>> GetTopBrokersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _dashboardRepository.GetTopBrokersAsync(where, companyId);
        }

        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _dashboardRepository.GetTopCustomersAsync(where, companyId);
        }

        public async Task<List<OverDueSaleDto>> GetOverDueSaleAsync(string companyId)
        {
            return await _dashboardRepository.GetOverDueSaleAsync(companyId);
        }

        public async Task<List<OverDuePurDto>> GetOverDuePurAsync(string companyId)
        {
            return await _dashboardRepository.GetOverDuePurAsync(companyId);
        }

        public async Task<List<SalesByCountryDto>> GetSalesByCountryAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _dashboardRepository.GetSalesByCountryAsync(where, companyId);
        }

        public async Task<List<SalePLPerLegendDto>> GetSalePLPerLegendAsync(string companyId)
        {
            return await _dashboardRepository.GetSalePLPerLegendAsync(companyId);
        }

        public async Task<List<StoneAgeDto>> GetStoneAgeAsync(string companyId)
        {
            return await _dashboardRepository.GetStoneAgeAsync(companyId);
        }

        public async Task<List<RevisedAgeDto>> GetRevisedAgeAsync(string companyId)
        {
            return await _dashboardRepository.GetRevisedAgeAsync(companyId);
        }
    }
}
