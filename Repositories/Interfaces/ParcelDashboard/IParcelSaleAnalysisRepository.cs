using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard
{
    public interface IParcelSaleAnalysisRepository
    {
        Task<List<ParcelSaleAnalysisResultDto>?> GetParcelSaleAnalysisDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId);
        Task<List<ParcelSalePivotDto>> GetParcelSalePivotDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId);
        Task<IEnumerable<BasicDataDto>> GetSaleSearchDropdownAsync(string companyId);
    }
}
