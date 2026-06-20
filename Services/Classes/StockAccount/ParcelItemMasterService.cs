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

        public async Task<bool> CheckLotNoExistsAsync(string lotNo, string companyId)
        {
            return await _repository.CheckLotNoExistsAsync(lotNo, companyId);
        }

        public async Task<int> GetMaxItemSrNoAsync(string companyId)
        {
            return await _repository.GetMaxItemSrNoAsync(companyId);
        }

        public async Task<bool> SaveParcelItemMasterAsync(ParcelItemMasterDto model, string actionType, string companyId)
        {
            return await _repository.SaveParcelItemMasterAsync(model, actionType, companyId);
        }

        public async Task<string> GetAccandItemCountAsync(int id, string companyId)
        {
            return await _repository.GetAccandItemCountAsync(id, companyId);
        }

        public async Task<bool> DeleteParcelItemMasterAsync(int id, string companyId)
        {
            return await _repository.DeleteParcelItemMasterAsync(id, companyId);
        }
    }
}
