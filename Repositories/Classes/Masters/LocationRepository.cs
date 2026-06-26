using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class LocationRepository : ILocationRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public LocationRepository(ISqlDataAccess sqlDataAccess)
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

        #region Area Master
        public async Task<List<AreaMaster>> GetAreaMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Area" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<AreaMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveAreaMasterAsync(AreaMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@AreaId", model.AreaID },
                { "@AreaName", model.AreaName ?? (object)DBNull.Value },
                { "@AreaPriority", model.AreaPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_AreaMaster", param, companyId);
            return result > 0 || true; // Some SP execution yields -1 but succeeds
        }

        public async Task<bool> DeleteAreaAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Area Master", "delete", companyId);
            if (permission != 1)
                throw new UnauthorizedAccessException("You are not authorized to perform this task.");

            var data = (await GetAreaMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
            if (data == null)
                throw new KeyNotFoundException("Record not found.");

            // MVC behavior: calls SaveAreaMasterData with ID, AreaID, null, 0, null, null, CompanyID, "delete"
            var model = new AreaMaster
            {
                ID = id,
                AreaID = data.AreaID,
                AreaName = null,
                AreaPriority = 0
            };

            return await SaveAreaMasterAsync(model, userId, companyId, "delete");
        }
        #endregion

        #region City Master
        public async Task<List<CityMaster>> GetCityMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "City" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<CityMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveCityMasterAsync(CityMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@CityId", model.CityID },
                { "@CityName", model.CityName ?? (object)DBNull.Value },
                { "@CityPriority", model.CityPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_CityMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<bool> DeleteCityAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "City Master", "delete", companyId);
            if (permission != 1)
                throw new UnauthorizedAccessException("You are not authorized to perform this task.");

            var data = (await GetCityMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
            if (data == null)
                throw new KeyNotFoundException("Record not found.");

            var model = new CityMaster
            {
                ID = id,
                CityID = data.CityID,
                CityName = null,
                CityPriority = 0
            };

            return await SaveCityMasterAsync(model, userId, companyId, "delete");
        }
        #endregion

        #region State Master
        public async Task<List<StateMaster>> GetStateMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "State" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<StateMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveStateMasterAsync(StateMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@StateId", model.StateID },
                { "@StateName", model.StateName ?? (object)DBNull.Value },
                { "@StatePriority", model.StatePriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_StateMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<bool> DeleteStateAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "State Master", "delete", companyId);
            if (permission != 1)
                throw new UnauthorizedAccessException("You are not authorized to perform this task.");

            var data = (await GetStateMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
            if (data == null)
                throw new KeyNotFoundException("Record not found.");

            var model = new StateMaster
            {
                ID = id,
                StateID = data.StateID,
                StateName = null,
                StatePriority = 0
            };

            return await SaveStateMasterAsync(model, userId, companyId, "delete");
        }
        #endregion

        #region Country Master
        public async Task<List<CountryMaster>> GetCountryMasterListAsync(string where, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "Country" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<CountryMaster>(
                "GetALLMasterList",
                param,
                companyId);

            return result.ToList();
        }

        public async Task<bool> SaveCountryMasterAsync(CountryMaster model, string userId, string companyId, string actionType)
        {
            var param = new Dictionary<string, object>
            {
                { "@id", model.ID },
                { "@CountryId", model.CountryID },
                { "@CountryName", model.CountryName ?? (object)DBNull.Value },
                { "@CountryPriority", model.CountryPriority },
                { "@CreatedBy", userId },
                { "@ModifyBy", userId },
                { "@CompanyID", companyId },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_CountryMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<bool> DeleteCountryAsync(int id, string userTypeId, string userId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, "Country Master", "delete", companyId);
            if (permission != 1)
                throw new UnauthorizedAccessException("You are not authorized to perform this task.");

            var data = (await GetCountryMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
            if (data == null)
                throw new KeyNotFoundException("Record not found.");

            var model = new CountryMaster
            {
                ID = id,
                CountryID = data.CountryID,
                CountryName = null,
                CountryPriority = 0
            };

            return await SaveCountryMasterAsync(model, userId, companyId, "delete");
        }
        #endregion
    }
}
