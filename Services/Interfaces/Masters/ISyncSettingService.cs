using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface ISyncSettingService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        Task<ApiResponse> SaveSyncAsync(
            ProviderSettings model,
            string companyId,
            string companyName,
            string sqlConnection
           );

        Task<ApiResponse> GetSyncAsync(int companyId);
    }
}
