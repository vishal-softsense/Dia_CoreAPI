using Dia_CoreAPI.DTOs.StockAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.StockAccount
{
    public interface ISolitaireItemMasterRepository
    {
        Task<IEnumerable<ItemMaster>> GetItemMasterListAsync(string whereClause, string companyId);
        Task<IEnumerable<BasicDataDto>> ItemMasterDropDownAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> FillCityStateCountryItemMasterDropDownAsync(string companyId);
        Task<SolitaireItemMasterDto?> GetItemMasterFullListAsync(string id, string companyId);
        Task<string> GetLotExistAsync(string whereClause, string companyId);
        Task<int> GetMaxItemSrNoAsync(string companyId);
        Task<bool> SaveItemMasterDataAsync(SolitaireItemMasterDto model, string actionType, string companyId, int singlePcs);
        Task<string> GetItemCountAsync(string where1, string where2, string companyId);
        Task<bool> DeleteItemMasterDataAsync(int id, string companyId, string actionType);
        Task<string?> GetParameterIdAsync(string parameter, string parameterValue, string usedFor, string companyId);
    }
}
