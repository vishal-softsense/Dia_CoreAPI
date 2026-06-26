using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface I4CMasterRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId,string formName,string actionType,string companyId);


        #region Shape Master
        Task<List<ShapeMaster>> GetShapeMasterListAsync(string where, string companyId);
        Task<bool> SaveShapeMasterAsync(ShapeMaster model,string userId, string companyId,string actionType);
        Task<bool> DeleteShapeAsync(int id,string userTypeId,string userId,string companyId);
        #endregion

        #region Color Master
        Task<List<ColorMaster>> GetColorMasterListAsync(string where, string companyId);
        Task<bool> SaveColorMasterAsync(ColorMaster model,string userId,string companyId,string actionType);
        Task<bool> DeleteColorAsync(int id,string userTypeId,string userId,string companyId);
        #endregion

        #region Size Master
        Task<List<SizeMaster>> GetSizeMasterListAsync(string where, string companyId);
        Task<bool> SaveSizeMasterAsync(SizeMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteSizeAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Clarity Master
        Task<List<ClarityMaster>> GetClarityMasterListAsync(string where, string companyId);
        Task<bool> SaveClarityMasterAsync(ClarityMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteClarityAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Sieve Master
        Task<List<SieveMaster>> GetSieveMasterListAsync(string where, string companyId);
        Task<bool> SaveSieveMasterAsync(SieveMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteSieveAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region LAB Master
        Task<List<LABMaster>> GetLABMasterListAsync(string where, string companyId);
        Task<bool> SaveLABMasterAsync(LABMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteLABAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Prop Master
        Task<List<PropMaster>> GetPropMasterListAsync(string where, string companyId);
        Task<bool> SavePropMasterAsync(PropMaster model, string userId, string companyId, string actionType);
        Task<bool> DeletePropAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Polish Master
        Task<List<PolishMaster>> GetPolishMasterListAsync(string where, string companyId);
        Task<bool> SavePolishMasterAsync(PolishMaster model, string userId, string companyId, string actionType);
        Task<bool> DeletePolishAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Sym Master
        Task<List<SymMaster>> GetSymMasterListAsync(string where, string companyId);
        Task<bool> SaveSymMasterAsync(SymMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteSymAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region FL Master
        Task<List<FLMaster>> GetFLMasterListAsync(string where, string companyId);
        Task<bool> SaveFLMasterAsync(FLMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteFLAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region FLColor Master
        Task<List<FLColorMaster>> GetFLColorMasterListAsync(string where, string companyId);
        Task<bool> SaveFLColorMasterAsync(FLColorMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteFLColorAsync(int id, string userTypeId, string userId, string companyId);
        #endregion
    }
}