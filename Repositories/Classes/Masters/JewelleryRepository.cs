using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class JewelleryRepository : IJewelleryRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public JewelleryRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        #region Jewellery Category Master
        public async Task<List<JewCategoryMaster>> GetJewCategoryMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "JewCategory" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewCategoryMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveJewCategoryMasterAsync(JewCategoryMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@CategoryId", model.CategoryID },
                { "@CategoryName", model.CategoryName ?? (object)DBNull.Value },
                { "@categoryPriority", model.CategoryPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_JewCategoryMaster", param, companyId);
            return true;
        }
        #endregion

        #region Jewellery SubCategory Master
        public async Task<List<JewSubCategoryMaster>> GetJewSubCategoryMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "JewSubCategory" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewSubCategoryMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveJewSubCategoryMasterAsync(JewSubCategoryMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@CategoryId", model.CategoryID },
                { "@SubCategoryId", model.SubCategoryID },
                { "@SubCategoryName", model.SubCategoryName ?? (object)DBNull.Value },
                { "@SubCategoryPriority", model.SubCategoryPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_JewSubCategoryMaster", param, companyId);
            return true;
        }
        #endregion

        #region Jewellery Metal Master
        public async Task<List<JewMetalMaster>> GetJewMetalMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "JewMetal" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewMetalMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveJewMetalMasterAsync(JewMetalMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@MetalId", model.MetalID },
                { "@MetalName", model.MetalName ?? (object)DBNull.Value },
                { "@MetalPriority", model.MetalPriority },
                { "@MetalType", model.MetalType ?? (object)DBNull.Value },
                { "@MetalColor", model.MetalColor ?? (object)DBNull.Value },
                { "@MetalPurity", model.MetalPurity ?? (object)DBNull.Value },
                { "@Metal24KTRatio", model.Metal24KTRatio },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_JewMetalMaster", param, companyId);
            return true;
        }

        public async Task<List<GetJewPurity>> GetPurityDataAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "GetJewPurity" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<GetJewPurity>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }
        #endregion

        #region Jewellery Metal Purity Master
        public async Task<List<JewMetalPurityMaster>> GetJewMetalPurityMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "JewMetalPurity" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewMetalPurityMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveJewMetalPurityMasterAsync(JewMetalPurityMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@MetalId", model.Metal ?? (object)DBNull.Value },
                { "@MetalPurityID", model.MetalPurityID },
                { "@MetalPurityPriority", model.MetalPurityPriority },
                { "@MetalPurityName", model.MetalPurityName ?? (object)DBNull.Value },
                { "@MetalPurityToPureRatio", model.MetalPurityToPureRatio },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_JewMetalPurityMaster", param, companyId);
            return true;
        }
        #endregion

        #region Jewellery Color Master
        public async Task<List<JewColorMaster>> GetJewColorMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "JewColor" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<JewColorMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveJewColorMasterAsync(JewColorMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@JewColorID", model.JewColorID },
                { "@JewColorName", model.JewColorName ?? (object)DBNull.Value },
                { "@JewColorPriority", model.JewColorPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_JewColorMaster", param, companyId);
            return true;
        }
        #endregion
    }
}
