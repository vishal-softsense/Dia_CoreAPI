using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IDiamondOriginService
    {
        // Mines Master
        Task<List<MinesMaster>> GetMinesMasterListAsync(string where, string companyId);
        Task<MinesMaster> GetMinesByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitMinesDataAsync(MinesMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteMinesAsync(int id, string userTypeId, string userId, string companyId);

        // Rough Master
        Task<List<RoughMaster>> GetRoughMasterListAsync(string where, string companyId);
        Task<RoughMaster> GetRoughByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitRoughDataAsync(RoughMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteRoughAsync(int id, string userTypeId, string userId, string companyId);
    }
}
