using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class ParcelItemMasterService : IParcelItemMasterService
    {
        private readonly IParcelItemMasterRepository _repository;

        public ParcelItemMasterService(IParcelItemMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PItemMasterDto>> GetParcelItemMasterListAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetParcelItemMasterListAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<BasicDataDto>> GetItemMasterDropdownAsync(string companyId)
        {
            return await _repository.GetItemMasterDropdownAsync(companyId);
        }

        public async Task<ParcelItemMasterDto?> GetParcelItemMasterByIdAsync(int id, string companyId)
        {
            return await _repository.GetParcelItemMasterByIdAsync(id, companyId);
        }

        public async Task<bool> CheckLotNoExistsAsync(string lotNo, string companyId, int? excludeId = null)
        {
            return await _repository.CheckLotNoExistsAsync(lotNo, companyId, excludeId);
        }

        public async Task<int> GetMaxItemSrNoAsync(string companyId)
        {
            return await _repository.GetMaxItemSrNoAsync(companyId);
        }

        public async Task<(bool success, string message)> SaveParcelItemMasterAsync(ParcelItemMasterDto model, string actionType, string companyId)
        {
            int? excludeId = actionType == "update" ? model.ItemCode : null;
            bool exists = await _repository.CheckLotNoExistsAsync(model.LotNo, companyId, excludeId);
            if (exists)
            {
                string existsMsg = actionType == "update" ? "LotNo Already Exists under another item." : "LotNo Already Exists";
                return (false, existsMsg);
            }

            bool saveSuccess = await _repository.SaveParcelItemMasterAsync(model, actionType, companyId);
            if (saveSuccess)
            {
                string successMsg = actionType == "update" ? "Parcel Item updated successfully." : "Parcel Item saved successfully.";
                return (true, successMsg);
            }
            return (false, $"Failed to {actionType} Parcel Item.");
        }

        public async Task<string> GetAccandItemCountAsync(int id, string companyId)
        {
            return await _repository.GetAccandItemCountAsync(id, companyId);
        }

        public async Task<(bool success, string message)> DeleteParcelItemMasterAsync(int id, string companyId)
        {
            string countStr = await _repository.GetAccandItemCountAsync(id, companyId);
            if (countStr != "0")
            {
                return (false, "Record is in use. You can not delete record");
            }

            bool deleteSuccess = await _repository.DeleteParcelItemMasterAsync(id, companyId);
            if (deleteSuccess)
            {
                return (true, "Parcel Item deleted successfully.");
            }
            return (false, "Failed to delete Parcel Item.");
        }
    }
}
