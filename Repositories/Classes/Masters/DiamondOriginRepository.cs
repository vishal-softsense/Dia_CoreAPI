using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class DiamondOriginRepository : IDiamondOriginRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public DiamondOriginRepository(ISqlDataAccess sqlDataAccess)
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

        #region Mines Master
        public async Task<List<MinesMaster>> GetMinesMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Mines" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<MinesMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveMinesMasterAsync(MinesMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@MinesId", model.MinesID },
                { "@MinesName", model.MinesName ?? (object)DBNull.Value },
                { "@MinesPriority", model.MinesPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_MinesMaster", param, companyId);
            return true;
        }
        #endregion

        #region Rough Master
        public async Task<List<RoughMaster>> GetRoughMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Rough" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<RoughMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveRoughMasterAsync(RoughMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@RoughId", model.RoughID },
                { "@RoughName", model.RoughName ?? (object)DBNull.Value },
                { "@RoughPriority", model.RoughPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            await _sqlDataAccess.ExecuteSP("SP_RoughMaster", param, companyId);
            return true;
        }
        #endregion
    }
}
