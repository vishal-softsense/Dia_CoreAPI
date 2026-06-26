using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class DiamondOriginService : IDiamondOriginService
    {
        private readonly IDiamondOriginRepository _repository;

        public DiamondOriginService(IDiamondOriginRepository repository)
        {
            _repository = repository;
        }

        private async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        #region Mines Master
        public async Task<List<MinesMaster>> GetMinesMasterListAsync(string where, string companyId)
        {
            return await _repository.GetMinesMasterListAsync(where, companyId);
        }

        public async Task<MinesMaster> GetMinesByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetMinesMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitMinesDataAsync(MinesMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Mines Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetMinesMasterListAsync($" AND MinesId = ''{model.MinesID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Mines id - " + model.MinesID);

                    bool result = await _repository.SaveMinesMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Mines Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetMinesMasterListAsync($" AND MinesId = ''{model.MinesID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Mines id already exists.");

                    bool result = await _repository.SaveMinesMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteMinesAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Mines Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetMinesByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new MinesMaster
                {
                    ID = id,
                    MinesID = data.MinesID,
                    MinesName = null,
                    MinesPriority = 0
                };

                bool result = await _repository.SaveMinesMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return (false, "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint") || ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
                {
                    return (false, "Record is in use, You can not delete record.");
                }
                return (false, ex.Message);
            }
        }
        #endregion

        #region Rough Master
        public async Task<List<RoughMaster>> GetRoughMasterListAsync(string where, string companyId)
        {
            return await _repository.GetRoughMasterListAsync(where, companyId);
        }

        public async Task<RoughMaster> GetRoughByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetRoughMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitRoughDataAsync(RoughMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Rough Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetRoughMasterListAsync($" AND RoughId = ''{model.RoughID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Rough id - " + model.RoughID);

                    bool result = await _repository.SaveRoughMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Rough Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetRoughMasterListAsync($" AND RoughId = ''{model.RoughID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Rough id already exists.");

                    bool result = await _repository.SaveRoughMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteRoughAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Rough Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetRoughByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new RoughMaster
                {
                    ID = id,
                    RoughID = data.RoughID,
                    RoughName = null,
                    RoughPriority = 0
                };

                bool result = await _repository.SaveRoughMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return (false, "Record is in use, You can not delete record.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint") || ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
                {
                    return (false, "Record is in use, You can not delete record.");
                }
                return (false, ex.Message);
            }
        }
        #endregion
    }
}
