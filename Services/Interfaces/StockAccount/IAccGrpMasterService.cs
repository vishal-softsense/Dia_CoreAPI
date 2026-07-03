using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.StockAccount
{
    public interface IAccGrpMasterService
    {
        Task<IEnumerable<AccGrpMasterListDto>> GetAccGrpMasterListAsync(string companyId, string whereClause = "");
        Task<IEnumerable<BasicDataDto>> GetAccGrpMasterDropdownAsync(string companyId);
        Task<AccGrpMasterDto?> GetAccGrpMasterByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> AddAccGrpMasterAsync(AccGrpMasterDto model, string companyId);
        Task<(bool Success, string Message)> UpdateAccGrpMasterAsync(AccGrpMasterDto model, string companyId);
        Task<(bool Success, string Message)> DeleteAccGrpMasterAsync(int id, string companyId);
        Task<bool> CheckAccGrpNameExistsAsync(string name, string companyId, int? excludeId = null);
        Task<float> GetMcurBaseRateAsync(string code, string companyId);
        Task<float> GetScurBaseRateAsync(string code, string companyId);
        Task<string> GetDefaultCurrencyCodeAsync(string companyId);
        Task<string> GetAccEffCodeAsync(int accType, string companyId);
        Task<string> GetAccEffSideAsync(int accType, string companyId);
    }
}
