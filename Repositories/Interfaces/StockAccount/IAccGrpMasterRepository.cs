using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.StockAccount
{
    public interface IAccGrpMasterRepository
    {
        Task<IEnumerable<AccGrpMasterListDto>> GetAccGrpMasterListAsync(string companyId, string whereClause = "");
        Task<IEnumerable<BasicDataDto>> GetAccGrpMasterDropdownAsync(string companyId);
        Task<AccGrpMasterDto?> GetAccGrpMasterByIdAsync(int id, string companyId);
        Task<bool> SaveAccGrpMasterAsync(AccGrpMasterDto model, string actionType, string companyId);
        Task<string> GetAccCountAsync(int id, string companyId);
        Task<bool> DeleteAccGrpMasterAsync(int id, string companyId);
        Task<bool> CheckAccGrpNameExistsAsync(string name, string companyId, int? excludeId = null);
        Task<string> GetMCurOperatorAsync(int currencyId, string companyId);
        Task<string> GetSCurOperatorAsync(int currencyId, string companyId);
        Task<float> GetMcurBaseRateAsync(string code, string companyId);
        Task<float> GetScurBaseRateAsync(string code, string companyId);
        Task<string> GetMasterSymbolAsync(string companyId);
        Task<string> GetStockSymbolAsync(string companyId);
        Task<string> GetDefaultCurrencyCodeAsync(string companyId);
        Task<string> GetAccEffCodeAsync(int accType, string companyId);
        Task<string> GetAccEffSideAsync(int accType, string companyId);
    }
}
