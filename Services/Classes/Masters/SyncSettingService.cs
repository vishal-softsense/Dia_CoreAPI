using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class SyncSettingService : ISyncSettingService
    {
        private readonly ISyncSettingRepository _syncSettingRepository;

        public SyncSettingService(ISyncSettingRepository syncSettingRepository)
        {
            _syncSettingRepository = syncSettingRepository;
        }
        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _syncSettingRepository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }
        public async Task<ApiResponse> SaveSyncAsync(
            ProviderSettings model,
            string companyId,
            string companyName,
            string sqlConnection)
        {
            return await _syncSettingRepository.SaveSyncAsync(
                model,
                companyId,
                companyName,
                sqlConnection);
        }

        public async Task<ApiResponse> GetSyncAsync(int companyId)
        {
            return await _syncSettingRepository.GetSyncAsync(companyId);
        }
    }
}
