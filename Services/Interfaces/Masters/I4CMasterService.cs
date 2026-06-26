using Dia_CoreAPI.DTOs.Masters;


namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface I4CMasterService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId,string formName,string actionType,string companyId);

        #region Shape Master
        Task<List<ShapeMaster>> GetShapeMasterListAsync(string where,string companyId);
        Task<(bool Success, string Message)> SaveShapeMasterAsync(ShapeMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteShapeAsync(int id, string userTypeId, string userId, string companyId); Task<bool> DeleteColorAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Color Master
        Task<List<ColorMaster>> GetColorMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveColorMasterAsync(ColorMaster model, string userTypeId, string userId, string companyId);
        #endregion

        #region Size Master
        Task<List<SizeMaster>> GetSizeMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveSizeMasterAsync(SizeMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteSizeAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Clarity Master
        Task<List<ClarityMaster>> GetClarityMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveClarityMasterAsync(ClarityMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteClarityAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Sieve Master
        Task<List<SieveMaster>> GetSieveMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveSieveMasterAsync(SieveMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteSieveAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region LAB Master
        Task<List<LABMaster>> GetLABMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveLABMasterAsync(LABMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteLABAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Prop Master
        Task<List<PropMaster>> GetPropMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SavePropMasterAsync(PropMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeletePropAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Polish Master
        Task<List<PolishMaster>> GetPolishMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SavePolishMasterAsync(PolishMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeletePolishAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Sym Master
        Task<List<SymMaster>> GetSymMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveSymMasterAsync(SymMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteSymAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region FL Master
        Task<List<FLMaster>> GetFLMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveFLMasterAsync(FLMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteFLAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region FLColor Master
        Task<List<FLColorMaster>> GetFLColorMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveFLColorMasterAsync(FLColorMaster model, string userTypeId, string userId, string companyId);
        Task<bool> DeleteFLColorAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

       

        
    }


}