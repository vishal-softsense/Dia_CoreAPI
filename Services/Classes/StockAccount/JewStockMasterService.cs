using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class JewStockMasterService : IJewStockMasterService
    {
        private readonly IJewStockMasterRepository _repository;

        public JewStockMasterService(IJewStockMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetMountingStockAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewMetalMasterAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewColorMasterAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId,string metalType)
        {
            return await _repository.GetJewPurityAsync(companyId, metalType);
        }
        public async Task<double> GetPureRatioAsync(string companyId,string metalType,string metalPurity)
        {
            return await _repository.GetPureRatioAsync(companyId, metalType, metalPurity);
        }

        public async Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId)
        {
            return await _repository.GetAddMountingStockDataAsync(companyId);
        }

        public async Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "")
        {
            return await _repository.GetJewCategoryMasterAsync(companyId, whereClause);
        }

        public async Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewSubCategoryMasterAsync(companyId, whereClause);
        }

        public async Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId)
        {
            return await _repository.GetJewSettingValueAsync(companyId);
        }
    }
}
