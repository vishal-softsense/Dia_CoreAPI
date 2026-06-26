using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IJewelleryRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        // Category
        Task<List<JewCategoryMaster>> GetJewCategoryMasterListAsync(string where, string companyId);
        Task<bool> SaveJewCategoryMasterAsync(JewCategoryMaster model, string userId, string companyId, string actionType);

        // SubCategory
        Task<List<JewSubCategoryMaster>> GetJewSubCategoryMasterListAsync(string where, string companyId);
        Task<bool> SaveJewSubCategoryMasterAsync(JewSubCategoryMaster model, string userId, string companyId, string actionType);

        // Metal
        Task<List<JewMetalMaster>> GetJewMetalMasterListAsync(string where, string companyId);
        Task<bool> SaveJewMetalMasterAsync(JewMetalMaster model, string userId, string companyId, string actionType);
        Task<List<GetJewPurity>> GetPurityDataAsync(string where, string companyId);

        // Metal Purity
        Task<List<JewMetalPurityMaster>> GetJewMetalPurityMasterListAsync(string where, string companyId);
        Task<bool> SaveJewMetalPurityMasterAsync(JewMetalPurityMaster model, string userId, string companyId, string actionType);

        // Color
        Task<List<JewColorMaster>> GetJewColorMasterListAsync(string where, string companyId);
        Task<bool> SaveJewColorMasterAsync(JewColorMaster model, string userId, string companyId, string actionType);
    }
}
