using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;

namespace Dia_CoreAPI.Repositories.Classes.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public DashboardRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<List<BestSellerDto>> GetBestSellerAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", where },
                { "@Type", "BestPerformingSeller" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<BestSellerDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<TopBrokerDto>> GetTopBrokersAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", where },
                { "@Type", "Top5Broker" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<TopBrokerDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", where },
                { "@Type", "Top10Customer" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<TopCustomerDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<OverDueSaleDto>> GetOverDueSaleAsync(string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", "and 1=1" },
                { "@Type", "OverdueSaleInvoice" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<OverDueSaleDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<OverDuePurDto>> GetOverDuePurAsync(string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", "and 1=1" },
                { "@Type", "OverduePurInvoice" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<OverDuePurDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<SalesByCountryDto>> GetSalesByCountryAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", where },
                { "@Type", "SalesByCountry" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SalesByCountryDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<SalePLPerLegendDto>> GetSalePLPerLegendAsync(string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", "and 1=1" },
                { "@Type", "SalePLPerLegend" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SalePLPerLegendDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<StoneAgeDto>> GetStoneAgeAsync(string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", "and 1=1" },
                { "@Type", "StoneAge" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<StoneAgeDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<List<RevisedAgeDto>> GetRevisedAgeAsync(string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", "and 1=1" },
                { "@Type", "RevisedAge" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<RevisedAgeDto>(
                "GetDashboarddata",
                param,
                companyId);

            return result.ToList();
        }
    }
}
