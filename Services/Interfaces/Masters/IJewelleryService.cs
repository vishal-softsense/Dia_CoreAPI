using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IJewelleryService
    {
        // Category
        Task<List<JewCategoryMaster>> GetJewCategoryMasterListAsync(string where, string companyId);
        Task<JewCategoryMaster> GetJewCategoryByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitJewCategoryDataAsync(JewCategoryMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJewCategoryAsync(int id, string userTypeId, string userId, string companyId);

        // SubCategory
        Task<List<JewSubCategoryMaster>> GetJewSubCategoryMasterListAsync(string where, string companyId);
        Task<JewSubCategoryMaster> GetJewSubCategoryByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitJewSubCategoryDataAsync(JewSubCategoryMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJewSubCategoryAsync(int id, string userTypeId, string userId, string companyId);

        // Metal
        Task<List<JewMetalMaster>> GetJewMetalMasterListAsync(string where, string companyId);
        Task<JewMetalMaster> GetJewMetalByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitJewMetalDataAsync(JewMetalMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJewMetalAsync(int id, string userTypeId, string userId, string companyId);
        Task<List<GetJewPurity>> GetPurityDataAsync(string where, string companyId);

        // Metal Purity
        Task<List<JewMetalPurityMaster>> GetJewMetalPurityMasterListAsync(string where, string companyId);
        Task<JewMetalPurityMaster> GetJewMetalPurityByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitJewMetalPurityDataAsync(JewMetalPurityMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJewMetalPurityAsync(int id, string userTypeId, string userId, string companyId);

        // Color
        Task<List<JewColorMaster>> GetJewColorMasterListAsync(string where, string companyId);
        Task<JewColorMaster> GetJewColorByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitJewColorDataAsync(JewColorMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJewColorAsync(int id, string userTypeId, string userId, string companyId);
    }
}
