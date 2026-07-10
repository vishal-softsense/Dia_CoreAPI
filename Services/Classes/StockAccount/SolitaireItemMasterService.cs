using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class SolitaireItemMasterService : ISolitaireItemMasterService
    {
        private readonly ISolitaireItemMasterRepository _repository;

        public SolitaireItemMasterService(ISolitaireItemMasterRepository repository)
        {
            _repository = repository;
        }

        #region Solitaire

        public async Task<IEnumerable<ItemMaster>> GetItemMasterListAsync(string companyId, string whereClause)
        {
            return await _repository.GetItemMasterListAsync(whereClause, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> ItemMasterDropDownAsync(string companyId)
        {
            return await _repository.ItemMasterDropDownAsync(companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> FillCityStateCountryItemMasterDropDownAsync(string companyId)
        {
            return await _repository.FillCityStateCountryItemMasterDropDownAsync(companyId);
        }

        public async Task<SolitaireItemMasterDto?> GetItemMasterFullListAsync(string id, string companyId)
        {
            return await _repository.GetItemMasterFullListAsync(id, companyId);
        }

        public async Task<string> GetLotExistAsync(string lotNo, string companyId)
        {
            return await _repository.GetLotExistAsync($" AND LotNo = '{lotNo}'", companyId);
        }

        public async Task<bool> AddItemAsync(SolitaireItemMasterDto item, string companyId)
        {
            string lotAlready = await _repository.GetLotExistAsync($" AND LotNo = '{item.LotNo}'", companyId);
            if (lotAlready != "0")
            {
                throw new Exception("LotNo Already Created.");
            }

            int singlePcs = 0;
            if (item.StoneTypes == "Single")
            {
                singlePcs += 1;
            }

            string itemName = "";
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                itemName = item.ItemName;
            }
            else
            {
                itemName = item.LotNo;
            }
            item.ItemName = itemName;

            return await _repository.SaveItemMasterDataAsync(item, "insert", companyId, singlePcs);
        }

        public async Task<bool> EditItemAsync(SolitaireItemMasterDto item, string companyId)
        {
            string itemName = "";
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                itemName = item.ItemName;
            }
            else
            {
                itemName = item.LotNo;
            }
            item.ItemName = itemName;

            int singlePcs = 0;
            if (item.StoneTypes == "Single")
            {
                singlePcs += 1;
            }

            string lotAlready = await _repository.GetLotExistAsync($" AND LotNo = '{item.LotNo}' AND ItemCode != {item.ItemCode}", companyId);
            if (lotAlready == "0")
            {
                return await _repository.SaveItemMasterDataAsync(item, "update", companyId, singlePcs);
            }
            else
            {
                throw new Exception("Sorry.! You Can not Update Record Because LotNo Already Created.");
            }
        }

        public async Task<bool> DeleteItemAsync(int id, string companyId)
        {
            string where = $"  (ItemCode = {id} OR FItemCode = {id} OR TItemCode = {id})";
            string where2 = $"  (ItemCode = {id})";

            string i = await _repository.GetItemCountAsync(where, where2, companyId);

            if (Convert.ToInt32(i) <= 0)
            {
                var data = await _repository.GetItemMasterListAsync($" AND ItemCode = {id}", companyId);
                
                bool hasData = false;
                foreach (var item in data)
                {
                    hasData = true;
                    break;
                }
                
                if (hasData)
                {
                    return await _repository.DeleteItemMasterDataAsync(id, companyId, "delete");
                }
                else
                {
                    throw new Exception("Record not found.");
                }
            }
            else
            {
                throw new Exception("Record is in use, You can not delete record.");
            }
        }

        #endregion
    }
}
