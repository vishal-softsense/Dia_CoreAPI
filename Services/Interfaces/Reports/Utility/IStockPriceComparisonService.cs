using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IStockPriceComparisonService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<StockPriceComparisonPageDto> LoadPageDataAsync(string companyId);
        Task<List<StockPriceComparisonReportDto>> GetReportAsync(StockPriceComparisonFilterDto filter, string companyId);
    }
}
