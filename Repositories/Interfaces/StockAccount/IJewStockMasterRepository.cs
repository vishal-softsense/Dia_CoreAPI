using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.StockAccount
{
    public interface IJewStockMasterRepository
    {
        Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(
        string companyId,
        string whereClause = "");

        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "");
        Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "");
        Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId, string metalType);

        Task<double> GetPureRatioAsync(string companyId,string metalType,string metalPurity);

        Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId);
        Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "");
        Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId,string whereClause = "");

        Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId);

        // Mounting-specific methods
        Task<JewStockMasterDto?> GetMountingStockByIdAsync(int id, string companyId);
        Task<bool> CheckStockIdExistsAsync(string stockId, string companyId);
        Task<bool> CheckMountingUsedAsync(string mountingId, string companyId);
        Task<bool> SaveMountingStockAsync(JewStockMasterDto model, List<DiamondDetails> diamond, List<SettingDetails> setting, string actionType, string companyId);
        Task<bool> DeleteMountingStockMasterAsync(int id, string companyId);
        Task<string> GetNextMountingStockIdAsync(string designId, string companyId);
        Task<IEnumerable<BasicDataDto>> GetMountingStockDropdownAsync(string category, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetAllLotNoListAllAsync(string searchby, string stoneType, string companyId);
        Task<(JewStockMasterDto? data, List<DiamondDetails> diamondList)> GetAllMountingDetailsAsync(int id, string companyId);
        Task<int> GetMaxStockCodeAsync(string companyId);
        Task<string> GetMaxItemCodeAsync(string whereClause, string companyId);
        Task<string> GetDesignNameAsync(string whereClause, string companyId);
        Task<int> GetMaxStockSerialNoAsync(string whereClause, string companyId);
        Task<bool> SaveStockDiamondDetailsDataAsync(string designId, int jewCode, int itemCode, string stockId, string type, string shape, string size, string sieve, string color, string clarity, int pcs, double carat, float price, double amount, string lab, string actionType, string companyId);
        Task<bool> SaveStockSettingDetailsDataAsync(string designId, int jewCode, string type, string shape, string size, int pcs, string settingType, float price, double amount, string actionType, string companyId);
        Task<bool> DeleteBothAsync(int jewCode, string actionType, string companyId);
        Task<List<LotNoDataJew>> GetJewLotNoDataAsync(string lotNo, string companyId);
    }
}
