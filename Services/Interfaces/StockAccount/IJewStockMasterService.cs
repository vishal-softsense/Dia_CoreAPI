using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.StockAccount
{
    public interface IJewStockMasterService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(string companyId, string whereClause = "");

        Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "");
        Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "");
        Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId,string metalType);
        Task<double> GetPureRatioAsync(string companyId, string metalType, string metalPurity);

        Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId);

        Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "");

        Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId,string whereClause = "");
        Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId);
    }
}
