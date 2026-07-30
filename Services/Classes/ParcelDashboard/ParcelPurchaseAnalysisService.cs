using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Services.Classes.ParcelDashboard
{
    public class ParcelPurchaseAnalysisService : IParcelPurchaseAnalysisService
    {
        private readonly IParcelPurchaseAnalysisRepository _repository;

        public ParcelPurchaseAnalysisService(IParcelPurchaseAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ParcelPurchaseAnalysisResultDto>?> GetParcelPurchaseAnalysisDataAsync(ParcelPurchaseAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetParcelPurchaseAnalysisDataAsync(filter, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetPurchaseSearchDropdownAsync(string companyId)
        {
            return await _repository.GetPurchaseSearchDropdownAsync(companyId);
        }
    }
}
