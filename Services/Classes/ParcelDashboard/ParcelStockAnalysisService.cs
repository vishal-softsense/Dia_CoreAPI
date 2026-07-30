using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Services.Classes.ParcelDashboard
{
    public class ParcelStockAnalysisService : IParcelStockAnalysisService
    {
        private readonly IParcelStockAnalysisRepository _repository;

        public ParcelStockAnalysisService(IParcelStockAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ParcelChartDataDto>?> GetParcelChartDataAsync(ParcelStockAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetParcelChartDataAsync(filter, companyId);
        }

        public async Task<List<ParcelStockPivotDto>> GetParcelPivotDataAsync(ParcelStockAnalysisRequestDto filter, string companyId)
        {
            return await _repository.GetParcelPivotDataAsync(filter, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            return await _repository.GetStockSearchDropdownAsync(companyId);
        }
    }
}
