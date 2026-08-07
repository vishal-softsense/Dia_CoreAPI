using Dia_CoreAPI.DTOs.DiaAnalytics;
using Dia_CoreAPI.Repositories.Interfaces.DiaAnalytics;
using Dia_CoreAPI.Services.Interfaces.DiaAnalytics;

namespace Dia_CoreAPI.Services.Classes.DiaAnalytics
{
    public class StockAnalyzeService : IStockAnalyzeService
    {
        private readonly IStockAnalyzeRepository _repository;

        public StockAnalyzeService(IStockAnalyzeRepository repository)
        {
            _repository = repository;
        }

        public StockAnalyzeConfigDto GetStockAnalyzeConfig()
        {
            return _repository.GetStockAnalyzeConfig();
        }
    }
}
