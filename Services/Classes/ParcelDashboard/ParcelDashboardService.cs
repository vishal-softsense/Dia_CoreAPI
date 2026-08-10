using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Services.Classes.ParcelDashboard
{
    public class ParcelDashboardService : IParcelDashboardService
    {
        private readonly IParcelDashboardRepository _repository;

        public ParcelDashboardService(IParcelDashboardRepository repository)
        {
            _repository = repository;
        }

        private string BuildWhereClause(string month, string year, string tdate)
        {
            string where = "";
            if (!string.IsNullOrEmpty(tdate) && tdate.Contains("start") && tdate.Contains("end"))
            {
                try
                {
                    var dateRange = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(tdate);
                    if (dateRange != null && dateRange.TryGetValue("start", out var startDate) && dateRange.TryGetValue("end", out var endDate))
                    {
                        where = $" AND tspt.SPDate >= ''{startDate}'' AND tspt.SPDate <= ''{endDate}'' ";
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(where))
            {
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
            }
            return where;
        }

        public async Task<List<ParcelBestSellerDto>> GetBestSellerAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetBestSellerAsync(where, companyId);
        }

        public async Task<List<ParcelTopBrokerDto>> GetTopBrokersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetTopBrokersAsync(where, companyId);
        }

        public async Task<List<ParcelTopBrokerDto>> GetAllBrokersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetAllBrokersAsync(where, companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetTopCustomersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetTopCustomersAsync(where, companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetAllCustomersAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetAllCustomersAsync(where, companyId);
        }

        public async Task<List<ParcelOverDueInvoiceCountDto>> GetOverDueSaleCountAsync(string companyId)
        {
            return await _repository.GetOverDueSaleCountAsync(companyId);
        }

        public async Task<List<ParcelOverDueInvoiceCountDto>> GetOverDuePurCountAsync(string companyId)
        {
            return await _repository.GetOverDuePurCountAsync(companyId);
        }

        public async Task<List<ParcelSalesByCountryDto>> GetSalesByCountryAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetSalesByCountryAsync(where, companyId);
        }

        public async Task<List<ParcelTopPayableDto>> GetTopPayableAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetTopPayableAsync("", companyId);
        }

        public async Task<List<ParcelTopPayableDto>> GetAllPayablesAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetAllPayablesAsync("", companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetTopOutstandingAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetTopOutstandingAsync("", companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetAllOutstandingsAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetAllOutstandingsAsync("", companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetTopOpenMemoAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetTopOpenMemoAsync(where, companyId);
        }

        public async Task<List<ParcelTopCustomerDto>> GetAllOpenMemosAsync(string month, string year, string tdate, string companyId)
        {
            string where = BuildWhereClause(month, year, tdate);
            return await _repository.GetAllOpenMemosAsync(where, companyId);
        }

        public async Task<List<ParcelOverDueSaleDetailDto>> GetTopOverdueSalesAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetTopOverdueSalesAsync("", companyId);
        }

        public async Task<List<ParcelOverDueSaleDetailDto>> GetAllOverdueSalesAsync(string month, string year, string tdate, string companyId)
        {
            return await _repository.GetAllOverdueSalesAsync("", companyId);
        }
    }
}
