using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Services.Classes.ParcelDashboard
{
    public class ParcelSaleAnalysisService : IParcelSaleAnalysisService
    {
        private readonly IParcelSaleAnalysisRepository _repository;

        public ParcelSaleAnalysisService(IParcelSaleAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ParcelSaleAnalysisResultDto>?> GetParcelSaleAnalysisDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetParcelSaleAnalysisDataAsync(filter, companyId);
        }

        public async Task<List<ParcelSalePivotDto>> GetParcelSalePivotDataAsync(ParcelSaleAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetParcelSalePivotDataAsync(filter, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetSaleSearchDropdownAsync(string companyId)
        {
            return await _repository.GetSaleSearchDropdownAsync(companyId);
        }
    }
}
