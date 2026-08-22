using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IStockTransferService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<StockTransferListDto>> GetListAsync(string companyId);
        Task<List<StockTranDataDto>> GetLotDetailsAsync(string lotNo, string companyId);
        Task<IEnumerable<BasicDataDto>> GetAllLotNoListAsync(string searchby, string companyId);
        Task<(bool Success, string Message)> SaveAsync(
            StockTransferSaveDto model,
            int userId,
            string userName,
            int companyId,
            string companyIdStr);
        Task<(bool Success, string Message)> DeleteAsync(int id, string companyIdStr);
    }
}
