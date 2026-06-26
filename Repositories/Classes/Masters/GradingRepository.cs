using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class GradingRepository : IGradingRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public GradingRepository(ISqlDataAccess sqlDataAccess)
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

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>("GetUserPermission", parameters, companyId);
            return result.Count();
        }

        #region Brown Master
        public async Task<List<BrownMaster>> GetBrownMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Brown" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<BrownMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveBrownMasterAsync(
          BrownMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@BrownId", model.BrownID },
                { "@BrownName", model.BrownName },
                { "@BrownPriority", model.BrownPriority },
                { "@BrownGroupName", model.BrownGroupName },
                { "@BrownGroupPriority", model.BrownGroupPriority },
                { "@PricingBrownID", model.PricingBrownID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_BrownMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteBrownAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Brown Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetBrownMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveBrownMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Green Master
        public async Task<List<GreenMaster>> GetGreenMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Green" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<GreenMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveGreenMasterAsync(
          GreenMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@GreenId", model.GreenID },
                { "@GreenName", model.GreenName },
                { "@GreenPriority", model.GreenPriority },
                { "@GreenGroupName", model.GreenGroupName },
                { "@GreenGroupPriority", model.GreenGroupPriority },
                { "@PricingGreenID", model.PricingGreenID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_GreenMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteGreenAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Green Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetGreenMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveGreenMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Milky Master
        public async Task<List<MilkyMaster>> GetMilkyMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Milky" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<MilkyMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveMilkyMasterAsync(
          MilkyMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@MilkyId", model.MilkyID },
                { "@MilkyName", model.MilkyName },
                { "@MilkyPriority", model.MilkyPriority },
                { "@MilkyGroupName", model.MilkyGroupName },
                { "@MilkyGroupPriority", model.MilkyGroupPriority },
                { "@PricingMilkyID", model.PricingMilkyID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_MilkyMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteMilkyAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Milky Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetMilkyMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveMilkyMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Luster Master
        public async Task<List<LusterMaster>> GetLusterMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Luster" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<LusterMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveLusterMasterAsync(
          LusterMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@LusterId", model.LusterID },
                { "@LusterName", model.LusterName },
                { "@LusterPriority", model.LusterPriority },
                { "@LusterGroupName", model.LusterGroupName },
                { "@LusterGroupPriority", model.LusterGroupPriority },
                { "@PricingLusterID", model.PricingLusterID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_LusterMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteLusterAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Luster Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetLusterMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveLusterMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Natts Master
        public async Task<List<NattsMaster>> GetNattsMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Natts" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<NattsMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveNattsMasterAsync(
          NattsMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@NattsId", model.NattsID },
                { "@NattsName", model.NattsName },
                { "@NattsPriority", model.NattsPriority },
                { "@NattsGroupName", model.NattsGroupName },
                { "@NattsGroupPriority", model.NattsGroupPriority },
                { "@PricingNattsID", model.PricingNattsID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_NattsMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteNattsAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Natts Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetNattsMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveNattsMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region HA Master
        public async Task<List<HAMaster>> GetHAMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "HA" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<HAMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveHAMasterAsync(
          HAMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@HAId", model.HAID },
                { "@HAName", model.HAName },
                { "@HAPriority", model.HAPriority },
                { "@HAGroupName", model.HAGroupName },
                { "@HAGroupPriority", model.HAGroupPriority },
                { "@PricingHAID", model.PricingHAID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_HAMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteHAAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "H&A Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetHAMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveHAMasterAsync(data, userId, companyId, "delete");
        }
        #endregion

        #region Shade Master
        public async Task<List<ShadeMaster>> GetShadeMasterListAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Shade" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<ShadeMaster>("GetALLMasterList", param, companyId);

            return result.ToList();
        }

        public async Task<bool> SaveShadeMasterAsync(
          ShadeMaster model,
          string userId,
          string companyId,
          string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@ShadeId", model.ShadeID },
                { "@ShadeName", model.ShadeName },
                { "@ShadePriority", model.ShadePriority },
                { "@ShadeGroupName", model.ShadeGroupName },
                { "@ShadeGroupPriority", model.ShadeGroupPriority },
                { "@PricingShadeID", model.PricingShadeID },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_ShadeMaster", param, companyId);

            return result > 0;
        }

        public async Task<bool> DeleteShadeAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Shade Master", "delete", companyId);

            if (permission != 1)
                throw new Exception("You are not authorized to perform this task.");

            var data = (await GetShadeMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();

            if (data == null)
                throw new Exception("Record not found.");

            return await SaveShadeMasterAsync(data, userId, companyId, "delete");
        }
        #endregion
    }
}
