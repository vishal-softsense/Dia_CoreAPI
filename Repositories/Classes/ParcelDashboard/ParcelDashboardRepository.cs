using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Repositories.Classes.ParcelDashboard
{
    public class ParcelDashboardRepository : IParcelDashboardRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ParcelDashboardRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        private async Task<List<T>> GetParcelDashboardDataAsync<T>(string type, string @where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Where", @where ?? string.Empty },
                { "@Type", type }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<T>(
                "GetParcelDashboarddata",
                param,
                companyId);

            return result.ToList();
        }

        public Task<List<ParcelBestSellerDto>> GetBestSellerAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelBestSellerDto>("BestPerformingSeller", where, companyId);
        }

        public Task<List<ParcelTopBrokerDto>> GetTopBrokersAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopBrokerDto>("Top5Broker", where, companyId);
        }

        public Task<List<ParcelTopBrokerDto>> GetAllBrokersAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopBrokerDto>("AllBroker", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetTopCustomersAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("Top10Customer", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetAllCustomersAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("AllCustomer", where, companyId);
        }

        public Task<List<ParcelOverDueInvoiceCountDto>> GetOverDueSaleCountAsync(string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelOverDueInvoiceCountDto>("OverdueSaleInvoice", "and 1=1", companyId);
        }

        public Task<List<ParcelOverDueInvoiceCountDto>> GetOverDuePurCountAsync(string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelOverDueInvoiceCountDto>("OverduePurInvoice", "and 1=1", companyId);
        }

        public Task<List<ParcelSalesByCountryDto>> GetSalesByCountryAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelSalesByCountryDto>("SalesByCountry", where, companyId);
        }

        public Task<List<ParcelTopPayableDto>> GetTopPayableAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopPayableDto>("Top10Payable", where, companyId);
        }

        public Task<List<ParcelTopPayableDto>> GetAllPayablesAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopPayableDto>("AllPayable", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetTopOutstandingAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("Top10Outstanding", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetAllOutstandingsAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("AllOutstanding", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetTopOpenMemoAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("Top10OpenMemo", where, companyId);
        }

        public Task<List<ParcelTopCustomerDto>> GetAllOpenMemosAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelTopCustomerDto>("AllOpenMemo", where, companyId);
        }

        public Task<List<ParcelOverDueSaleDetailDto>> GetTopOverdueSalesAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelOverDueSaleDetailDto>("Top10Overduesale", where, companyId);
        }

        public Task<List<ParcelOverDueSaleDetailDto>> GetAllOverdueSalesAsync(string where, string companyId)
        {
            return GetParcelDashboardDataAsync<ParcelOverDueSaleDetailDto>("AllOverduesale", where, companyId);
        }
    }
}
