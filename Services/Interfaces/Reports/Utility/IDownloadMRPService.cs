using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IDownloadMRPService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<(bool Success, string Data)> DownloadMRPAsync(string companyId);
        Task<(bool Success, string Data)> UpdateMRPAsync(string companyId);
    }
}
