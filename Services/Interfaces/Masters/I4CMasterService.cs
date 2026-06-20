using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface I4CMasterService
    {
        Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);
        Task<List<ShapeMaster>> GetShapeMasterListAsync(
            string where,string companyId);
        Task<List<ColorMaster>> GetColorMasterListAsync(
           string where, string companyId);

        Task<bool> SaveShapeMasterAsync(
            ShapeMaster model,
            string userId,
            string companyId,
            string actionType);
        Task<bool> SaveColorMasterAsync(
            ColorMaster model,
            string userId,
            string companyId,
            string actionType);

        Task<bool> DeleteShapeAsync(
    int id,
    string userTypeId,
    string userId,
    string companyId);
    }
}