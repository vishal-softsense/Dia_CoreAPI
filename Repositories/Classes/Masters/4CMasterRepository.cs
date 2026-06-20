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
            var param = new Dictionary<string, object>
            {
                { "@Type", "Shape" },
                { "@Where", where }
            };

            var result =
                await _sqlDataAccess.SelectDataFromSP<ShapeMaster>(
                    "GetALLMasterList",
                    param,
                    companyId);

            return result.ToList();
        }

        /*
        public async Task<List<ColorMaster>> GetColorMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Color" },
                { "@Where", where }
            };

            var result =
                await _sqlDataAccess.SelectDataFromSP<ColorMaster>(
                    "GetALLMasterList",
                    param,
                    companyId);

            return result.ToList();
        }
        */

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

        /*
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

            var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            param.Add("@ReturnValue", returnValueParam);

            await _sqlDataAccess.ExecuteSP("SP_ColorMaster", param, companyId);

            int returnValue = (int)(returnValueParam.Value ?? 0);

            return returnValue == 1;
        }
        */

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