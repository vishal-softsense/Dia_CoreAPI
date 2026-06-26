using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _repository;

        public LocationService(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        #region Area Master
        public async Task<List<AreaMaster>> GetAreaMasterListAsync(string where, string companyId)
        {
            return await _repository.GetAreaMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveAreaMasterAsync(
            AreaMaster model,
            string userId,
            string userTypeId,
            string companyId,
            string actionType)
        {
            try
            {
                if (actionType.ToLower() == "insert" || model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Area Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetAreaMasterListAsync($" AND AreaId = '{model.AreaID}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Area id - " + model.AreaID);

                    bool result = await _repository.SaveAreaMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Area Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetAreaMasterListAsync($" AND AreaId = '{model.AreaID}' AND ID <> {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Area id already exists.");

                    bool result = await _repository.SaveAreaMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAreaAsync(
            int id,
            string userTypeId,
            string userId,
            string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Area Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = (await _repository.GetAreaMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
                if (data == null)
                    return (false, "Record not found.");

                var model = new AreaMaster
                {
                    ID = id,
                    AreaID = data.AreaID,
                    AreaName = null,
                    AreaPriority = 0
                };

                bool result = await _repository.SaveAreaMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region City Master
        public async Task<List<CityMaster>> GetCityMasterListAsync(string where, string companyId)
        {
            return await _repository.GetCityMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveCityMasterAsync(
            CityMaster model,
            string userId,
            string userTypeId,
            string companyId,
            string actionType)
        {
            try
            {
                if (actionType.ToLower() == "insert" || model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "City Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetCityMasterListAsync($" AND CityId = '{model.CityID}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with City id - " + model.CityID);

                    bool result = await _repository.SaveCityMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "City Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetCityMasterListAsync($" AND CityId = '{model.CityID}' AND ID <> {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because City id already exists.");

                    bool result = await _repository.SaveCityMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteCityAsync(
            int id,
            string userTypeId,
            string userId,
            string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "City Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = (await _repository.GetCityMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
                if (data == null)
                    return (false, "Record not found.");

                var model = new CityMaster
                {
                    ID = id,
                    CityID = data.CityID,
                    CityName = null,
                    CityPriority = 0
                };

                bool result = await _repository.SaveCityMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region State Master
        public async Task<List<StateMaster>> GetStateMasterListAsync(string where, string companyId)
        {
            return await _repository.GetStateMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveStateMasterAsync(
            StateMaster model,
            string userId,
            string userTypeId,
            string companyId,
            string actionType)
        {
            try
            {
                if (actionType.ToLower() == "insert" || model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "State Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetStateMasterListAsync($" AND StateId = '{model.StateID}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with State id - " + model.StateID);

                    bool result = await _repository.SaveStateMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "State Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetStateMasterListAsync($" AND StateId = '{model.StateID}' AND ID <> {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because State id already exists.");

                    bool result = await _repository.SaveStateMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteStateAsync(
            int id,
            string userTypeId,
            string userId,
            string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "State Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = (await _repository.GetStateMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
                if (data == null)
                    return (false, "Record not found.");

                var model = new StateMaster
                {
                    ID = id,
                    StateID = data.StateID,
                    StateName = null,
                    StatePriority = 0
                };

                bool result = await _repository.SaveStateMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Country Master
        public async Task<List<CountryMaster>> GetCountryMasterListAsync(string where, string companyId)
        {
            return await _repository.GetCountryMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveCountryMasterAsync(
            CountryMaster model,
            string userId,
            string userTypeId,
            string companyId,
            string actionType)
        {
            try
            {
                if (actionType.ToLower() == "insert" || model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Country Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetCountryMasterListAsync($" AND CountryId = '{model.CountryID}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Country id - " + model.CountryID);

                    bool result = await _repository.SaveCountryMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Country Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetCountryMasterListAsync($" AND CountryId = '{model.CountryID}' AND ID <> {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Country id already exists.");

                    bool result = await _repository.SaveCountryMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteCountryAsync(
            int id,
            string userTypeId,
            string userId,
            string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Country Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = (await _repository.GetCountryMasterListAsync(" AND ID = " + id, companyId)).SingleOrDefault();
                if (data == null)
                    return (false, "Record not found.");

                var model = new CountryMaster
                {
                    ID = id,
                    CountryID = data.CountryID,
                    CountryName = null,
                    CountryPriority = 0
                };

                bool result = await _repository.SaveCountryMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion
    }
}
