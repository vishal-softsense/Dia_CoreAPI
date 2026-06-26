using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IGradingRepository
    {
        #region Brown Master
        Task<List<BrownMaster>> GetBrownMasterListAsync(string where, string companyId);
        Task<bool> SaveBrownMasterAsync(BrownMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteBrownAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Green Master
        Task<List<GreenMaster>> GetGreenMasterListAsync(string where, string companyId);
        Task<bool> SaveGreenMasterAsync(GreenMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteGreenAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Milky Master
        Task<List<MilkyMaster>> GetMilkyMasterListAsync(string where, string companyId);
        Task<bool> SaveMilkyMasterAsync(MilkyMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteMilkyAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Luster Master
        Task<List<LusterMaster>> GetLusterMasterListAsync(string where, string companyId);
        Task<bool> SaveLusterMasterAsync(LusterMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteLusterAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Natts Master
        Task<List<NattsMaster>> GetNattsMasterListAsync(string where, string companyId);
        Task<bool> SaveNattsMasterAsync(NattsMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteNattsAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region HA Master
        Task<List<HAMaster>> GetHAMasterListAsync(string where, string companyId);
        Task<bool> SaveHAMasterAsync(HAMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteHAAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Shade Master
        Task<List<ShadeMaster>> GetShadeMasterListAsync(string where, string companyId);
        Task<bool> SaveShadeMasterAsync(ShadeMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteShadeAsync(int id, string userTypeId, string userId, string companyId);
        #endregion
    }
}
