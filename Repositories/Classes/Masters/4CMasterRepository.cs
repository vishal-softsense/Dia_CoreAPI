using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class _4CMasterRepository : I4CMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public _4CMasterRepository(ISqlDataAccess sqlDataAccess)
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

            var result =
                await _sqlDataAccess.SelectDataFromSP<PermissionCheck>("GetUserPermission",parameters,companyId);
            return result.Count();
        }

        #region Shape Master
        public async Task<List<ShapeMaster>> GetShapeMasterListAsync(string where,string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Shape" },
                { "@Where", where }
            };

            var result =await _sqlDataAccess.SelectDataFromSP<ShapeMaster>("GetALLMasterList",param,companyId);
            return result.ToList();
        }
        public async Task<bool> SaveShapeMasterAsync(
         ShapeMaster model,
         string userId,
         string companyId,
         string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@ShapeId", model.ShapeID },
                { "@ShapeName", model.ShapeName },
                { "@ShapePriority", model.ShapePriority },
                { "@ShapeGroupName", model.ShapeGroupName },
                { "@ShapeGroupPriority", model.ShapeGroupPriority },
                { "@PricingShapeID", model.PricingShapeID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_ShapeMaster", param, companyId);
            return result > 0;
        }

        public async Task<bool> DeleteShapeAsync(int id,string userTypeId,string userId,string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data =(await GetShapeMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveShapeMasterAsync(data, userId, companyId, "delete");
        }
        #endregion


        #region Color Master
        public async Task<List<ColorMaster>> GetColorMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Color" },
                { "@Where", where }
            };

            var result =await _sqlDataAccess.SelectDataFromSP<ColorMaster>("GetALLMasterList",param,companyId);

            return result.ToList();
        }

        public async Task<bool> SaveColorMasterAsync(
          ColorMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@ColorId", model.ColorID },
                { "@ColorName", model.ColorName },
                { "@ColorPriority", model.ColorPriority },
                { "@ColorGroupName", model.ColorGroupName },
                { "@ColorGroupPriority", model.ColorGroupPriority },
                { "@PricingColorID", model.PricingColorID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_ColorMaster", param, companyId);

            return result > 0;
        }


        public async Task<bool> DeleteColorAsync(int id,string userTypeId,string userId,string companyId)
        {
            int permission =await CheckUserPermissionAsync(userTypeId, "Color Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data =
                (await GetColorMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveColorMasterAsync(data, userId, companyId, "delete");
        }
        #endregion


        #region Size Master
        public async Task<List<SizeMaster>> GetSizeMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Size" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SizeMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveSizeMasterAsync(
          SizeMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@SizeId", model.SizeID },
                { "@SizeName", model.SizeName },
                { "@FCarat", model.FCarat },
                { "@TCarat", model.TCarat },
                { "@SizePriority", model.SizePriority },
                { "@SizeGroupName", model.SizeGroupName },
                { "@SizeGroupPriority", model.SizeGroupPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_SizeMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteSizeAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Size Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetSizeMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveSizeMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Clarity Master
        public async Task<List<ClarityMaster>> GetClarityMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Clarity" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<ClarityMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveClarityMasterAsync(
          ClarityMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@ClarityId", model.ClarityID },
                { "@ClarityName", model.ClarityName },
                { "@ClarityPriority", model.ClarityPriority },
                { "@ClarityGroupName", model.ClarityGroupName },
                { "@ClarityGroupPriority", model.ClarityGroupPriority },
                { "@PricingClarityID", model.PricingClarityID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_ClarityMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteClarityAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Clarity Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetClarityMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveClarityMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Sieve Master
        public async Task<List<SieveMaster>> GetSieveMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Sieve" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SieveMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveSieveMasterAsync(
          SieveMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@SieveId", model.SieveID },
                { "@SieveName", model.SieveName },
                { "@SievePriority", model.SievePriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_SieveMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteSieveAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Sieve Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetSieveMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveSieveMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region LAB Master
        public async Task<List<LABMaster>> GetLABMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Cert" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<LABMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveLABMasterAsync(
          LABMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@CertId", model.CertID },
                { "@CertName", model.CertName },
                { "@CertPriority", model.CertPriority },
                { "@CertGroupName", model.CertGroupName },
                { "@CertGroupPriority", model.CertGroupPriority },
                { "@PricingCertID", model.PricingCertID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_CertMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteLABAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "LAB Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetLABMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveLABMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Prop Master
        public async Task<List<PropMaster>> GetPropMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Prop" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PropMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SavePropMasterAsync(
          PropMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@PropId", model.PropID },
                { "@PropName", model.PropName },
                { "@PropPriority", model.PropPriority },
                { "@PropGroupName", model.PropGroupName },
                { "@PropGroupPriority", model.PropGroupPriority },
                { "@PricingPropID", model.PricingPropID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_PropMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeletePropAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Proportion Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetPropMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SavePropMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Polish Master
        public async Task<List<PolishMaster>> GetPolishMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Polish" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PolishMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SavePolishMasterAsync(
          PolishMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@PolishId", model.PolishID },
                { "@PolishName", model.PolishName },
                { "@PolishPriority", model.PolishPriority },
                { "@PolishGroupName", model.PolishGroupName },
                { "@PolishGroupPriority", model.PolishGroupPriority },
                { "@PricingPolishID", model.PricingPolishID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_PolishMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeletePolishAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Polish Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetPolishMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SavePolishMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Sym Master
        public async Task<List<SymMaster>> GetSymMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Sym" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SymMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveSymMasterAsync(
          SymMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@SymId", model.SymID },
                { "@SymName", model.SymName },
                { "@SymPriority", model.SymPriority },
                { "@SymGroupName", model.SymGroupName },
                { "@SymGroupPriority", model.SymGroupPriority },
                { "@PricingSymID", model.PricingSymID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_SymMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteSymAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Symmetry Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetSymMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveSymMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region FL Master
        public async Task<List<FLMaster>> GetFLMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "FL" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<FLMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveFLMasterAsync(
          FLMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@FLId", model.FLID },
                { "@FLName", model.FLName },
                { "@FLPriority", model.FLPriority },
                { "@FLGroupName", model.FLGroupName },
                { "@FLGroupPriority", model.FLGroupPriority },
                { "@PricingFLID", model.PricingFLID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_FLMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteFLAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetFLMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveFLMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region FLColor Master
        public async Task<List<FLColorMaster>> GetFLColorMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "FlColor" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<FLColorMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveFLColorMasterAsync(
          FLColorMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@FLColorId", model.FLColorID },
                { "@FLColorName", model.FLColorName },
                { "@FLColorPriority", model.FLColorPriority },
                { "@FLColorGroupName", model.FLColorGroupName },
                { "@FLColorGroupPriority", model.FLColorGroupPriority },
                { "@PricingFLColorID", model.PricingFLColorID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_FLColorMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteFLColorAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Color Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetFLColorMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveFLColorMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

       
        
    }
}