using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface ISyncSettingRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<ApiResponse> SaveSyncAsync(
            ProviderSettings model,
            string companyId,
            string companyName,
            string sqlConnection);

        Task<ApiResponse> GetSyncAsync(int companyId);
    }
}
