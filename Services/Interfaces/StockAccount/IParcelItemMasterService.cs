using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.StockAccount
{
    public interface IParcelItemMasterService
    {
        Task<IEnumerable<PItemMasterDto>> GetParcelItemMasterListAsync(string companyId, string whereClause = "");
        Task<IEnumerable<BasicDataDto>> GetItemMasterDropdownAsync(string companyId);
        Task<ParcelItemMasterDto?> GetParcelItemMasterByIdAsync(int id, string companyId);
        Task<bool> CheckLotNoExistsAsync(string lotNo, string companyId, int? excludeId = null);
        Task<int> GetMaxItemSrNoAsync(string companyId);
        Task<(bool success, string message)> SaveParcelItemMasterAsync(ParcelItemMasterDto model, string actionType, string companyId);
        Task<string> GetAccandItemCountAsync(int id, string companyId);
        Task<(bool success, string message)> DeleteParcelItemMasterAsync(int id, string companyId);
    }
}
