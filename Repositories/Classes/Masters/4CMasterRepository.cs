using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;

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

            var parameters = new DynamicParameters();

            parameters.Add("@UserTypeId", "'" + userTypeId + "'");
            parameters.Add("@FormName", "'" + formName + "'");
            parameters.Add("@Where", where);

            var result =
                await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                    "GetUserPermission",
                    parameters,
                    companyId);

            return result.Count();
        }
        
        public async Task<List<ShapeMaster>> GetShapeMasterListAsync(
            string where,
            string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@Type", "Shape");
            param.Add("@Where", where);

            var result =
                await _sqlDataAccess.SelectDataFromSP<ShapeMaster>(
                    "GetALLMasterList",
                    param,
                    companyId);

            return result.ToList();
        }
        public async Task<List<ColorMaster>> GetColorMasterListAsync(
            string where,
            string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@Type", "Color");
            param.Add("@Where", where);

            var result =
                await _sqlDataAccess.SelectDataFromSP<ColorMaster>(
                    "GetALLMasterList",
                    param,
                    companyId);

            return result.ToList();
        }

        public async Task<bool> SaveShapeMasterAsync(
           ShapeMaster model,
           string userId,
           string companyId,
           string actionType)
        {
            var param = new DynamicParameters();

            param.Add("@id", model.ID);
            param.Add("@ShapeId", model.ShapeID);
            param.Add("@ShapeName", model.ShapeName);
            param.Add("@ShapePriority", model.ShapePriority);
            param.Add("@ShapeGroupName", model.ShapeGroupName);
            param.Add("@ShapeGroupPriority", model.ShapeGroupPriority);
            param.Add("@PricingShapeID", model.PricingShapeID);
            param.Add("@CreatedBy", userId);
            param.Add("@ModifyBy", userId);
            param.Add("@CompanyID", companyId);
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_ShapeMaster", param,companyId);
            return result > 0;
            //return true;
        }

        public async Task<bool> SaveColorMasterAsync(
           ColorMaster model,
           string userId,
           string companyId,
           string actionType)
        {
            var param = new DynamicParameters();

            param.Add("@id", model.ID);
            param.Add("@ColorId", model.ColorID);
            param.Add("@ColorName", model.ColorName);
            param.Add("@ColorPriority", model.ColorPriority);
            param.Add("@ColorGroupName", model.ColorGroupName);
            param.Add("@ColorGroupPriority", model.ColorGroupPriority);
            param.Add("@PricingColorID", model.PricingColorID);
            param.Add("@CreatedBy", userId);
            param.Add("@ModifyBy", userId);
            param.Add("@CompanyID", companyId);
            param.Add("@ActionType", actionType);

            param.Add("@ReturnValue",dbType: DbType.Int32,direction: ParameterDirection.Output);

            await _sqlDataAccess.ExecuteSP("SP_ColorMaster", param, companyId);

            int returnValue = param.Get<int>("@ReturnValue");

            return returnValue == 1;
        }

        public async Task<bool> DeleteShapeAsync(
    int id,
    string userTypeId,
    string userId,
    string companyId)
        {
            int permission =
                await CheckUserPermissionAsync(userTypeId,"Shape Master", "delete",companyId);

            if (permission != 1)
                throw new Exception(
                    "You are not authorized to perform this task.");

            var data =
                (await GetShapeMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveShapeMasterAsync(data, userId, companyId, "delete");
        }
    }
}