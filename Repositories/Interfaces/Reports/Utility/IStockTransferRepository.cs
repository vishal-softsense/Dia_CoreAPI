using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.DTOs.StockAccount;


namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IStockTransferRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<StockTransferListDto>> GetStockTransferListAsync(string companyId);
        Task<List<StockTranDataDto>> GetLotDetailsAsync(string lotNo, string companyId);
        /// <summary>Matches MVC common/get-alllotno-list → GetLotDataForDropdown.</summary>
        Task<IEnumerable<BasicDataDto>> GetAllLotNoListAsync(string searchby, string companyId);
        Task<(bool Success, string Message)> SaveStockTransferAsync(
            StockTransferSaveDto model,
            int userId,
            string userName,
            int companyId,
            string companyIdStr);
        Task<(bool Success, string Message)> DeleteStockTransferAsync(int id, string companyIdStr);
    }
}
