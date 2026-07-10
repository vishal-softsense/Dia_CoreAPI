using Dia_CoreAPI.DTOs.StockAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.StockAccount
{
    public interface ISolitaireItemMasterService
    {
        Task<IEnumerable<ItemMaster>> GetItemMasterListAsync(string companyId, string whereClause);
        Task<IEnumerable<BasicDataDto>> ItemMasterDropDownAsync(string companyId);
        Task<IEnumerable<BasicDataDto>> FillCityStateCountryItemMasterDropDownAsync(string companyId);
        Task<SolitaireItemMasterDto?> GetItemMasterFullListAsync(string id, string companyId);
        Task<string> GetLotExistAsync(string lotNo, string companyId);
        Task<bool> AddItemAsync(SolitaireItemMasterDto item, string companyId);
        Task<bool> EditItemAsync(SolitaireItemMasterDto item, string companyId);
        Task<bool> DeleteItemAsync(int id, string companyId);
    }
}
