using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.ParcelDashboard
{
    public interface IParcelPurchaseAnalysisService
    {
        Task<List<ParcelPurchaseAnalysisResultDto>?> GetParcelPurchaseAnalysisDataAsync(ParcelPurchaseAnalysisRequestDto filter, string companyId);
        Task<IEnumerable<BasicDataDto>> GetPurchaseSearchDropdownAsync(string companyId);
    }
}
