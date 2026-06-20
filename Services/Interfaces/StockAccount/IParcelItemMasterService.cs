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
        Task<bool> CheckLotNoExistsAsync(string lotNo, string companyId);
        Task<int> GetMaxItemSrNoAsync(string companyId);
        Task<bool> SaveParcelItemMasterAsync(ParcelItemMasterDto model, string actionType, string companyId);
        Task<string> GetAccandItemCountAsync(int id, string companyId);
        Task<bool> DeleteParcelItemMasterAsync(int id, string companyId);
    }
}
