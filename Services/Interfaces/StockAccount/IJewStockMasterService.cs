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

        // Mounting-specific methods
        Task<JewStockMasterDto?> GetMountingStockByIdAsync(int id, string companyId);
        Task<(bool success, string message)> SaveMountingStockAsync(JewStockMasterDto model, List<DiamondDetails> diamond, List<SettingDetails> setting, string actionType, string userTypeId, string companyId);
        Task<(bool success, string message)> DeleteMountingStockMasterAsync(int id, string companyId);
        Task<(bool success, string message, JewGridData? data)> AddMountingItemInGridAsync(AddJewLotNo item, List<JewDiamondDetails> existingGrid);
        Task<(bool success, string message, JewGridData? data)> DeleteMountingItemFromGridAsync(int index, List<JewDiamondDetails> existingGrid);
        Task<string> GetNextMountingStockIdAsync(string designId, string companyId);
        Task<IEnumerable<BasicDataDto>> GetMountingStockDropdownAsync(string category, string companyId);
        Task<IEnumerable<BasicDataDto>> GetAllLotNoListAllAsync(string searchby, string stoneType, string companyId);
        Task<(JewStockMasterDto? data, List<JewDiamondDetails> diamondList)> GetAllMountingDetailsAsync(int id, string companyId);
        Task<List<LotNoDataJew>> GetJewLotNoDataAsync(string lotNo, string companyId);
    }
}
