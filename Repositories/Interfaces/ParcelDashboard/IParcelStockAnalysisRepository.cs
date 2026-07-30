using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard
{
    public interface IParcelStockAnalysisRepository
    {
        Task<List<ParcelChartDataDto>?> GetParcelChartDataAsync(ParcelStockAnalysisRequestDto filter, string companyId);
        Task<List<ParcelStockPivotDto>> GetParcelPivotDataAsync(ParcelStockAnalysisRequestDto filter, string companyId);
        Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId);
    }
}
