using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IGradingService
    {
        #region Brown Master
        Task<List<BrownMaster>> GetBrownMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveBrownMasterAsync(BrownMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteBrownAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Green Master
        Task<List<GreenMaster>> GetGreenMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveGreenMasterAsync(GreenMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteGreenAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Milky Master
        Task<List<MilkyMaster>> GetMilkyMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveMilkyMasterAsync(MilkyMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteMilkyAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Luster Master
        Task<List<LusterMaster>> GetLusterMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveLusterMasterAsync(LusterMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteLusterAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Natts Master
        Task<List<NattsMaster>> GetNattsMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveNattsMasterAsync(NattsMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteNattsAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region HA Master
        Task<List<HAMaster>> GetHAMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveHAMasterAsync(HAMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteHAAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Shade Master
        Task<List<ShadeMaster>> GetShadeMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveShadeMasterAsync(ShadeMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteShadeAsync(int id, string userTypeId, string userId, string companyId);
        #endregion
    }
}
