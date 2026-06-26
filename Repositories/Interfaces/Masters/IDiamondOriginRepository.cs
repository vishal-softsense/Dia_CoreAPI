using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IDiamondOriginRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        // Mines Master
        Task<List<MinesMaster>> GetMinesMasterListAsync(string where, string companyId);
        Task<bool> SaveMinesMasterAsync(MinesMaster model, string userId, string companyId, string actionType);

        // Rough Master
        Task<List<RoughMaster>> GetRoughMasterListAsync(string where, string companyId);
        Task<bool> SaveRoughMasterAsync(RoughMaster model, string userId, string companyId, string actionType);
    }
}
