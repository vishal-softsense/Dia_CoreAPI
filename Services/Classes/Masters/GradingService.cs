using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Classes.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class GradingService : IGradingService
    {
        private readonly IGradingRepository _repository;

        public GradingService(IGradingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            if (_repository is GradingRepository repo)
            {
                return await repo.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
            }
            return 0;
        }

        #region Brown Master
        public async Task<List<BrownMaster>> GetBrownMasterListAsync(string where, string companyId)
        {
            return await _repository.GetBrownMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveBrownMasterAsync(BrownMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Brown Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetBrownMasterListAsync($" AND BrownId = '{model.BrownID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Brown id - " + model.BrownID);
                }

                bool result = await _repository.SaveBrownMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Brown Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetBrownMasterListAsync($" AND BrownId = '{model.BrownID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Brown id already exists.");
                }

                bool result = await _repository.SaveBrownMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteBrownAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteBrownAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Green Master
        public async Task<List<GreenMaster>> GetGreenMasterListAsync(string where, string companyId)
        {
            return await _repository.GetGreenMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveGreenMasterAsync(GreenMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Green Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetGreenMasterListAsync($" AND GreenId = '{model.GreenID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Green id - " + model.GreenID);
                }

                bool result = await _repository.SaveGreenMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Green Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetGreenMasterListAsync($" AND GreenId = '{model.GreenID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Green id already exists.");
                }

                bool result = await _repository.SaveGreenMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteGreenAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteGreenAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Milky Master
        public async Task<List<MilkyMaster>> GetMilkyMasterListAsync(string where, string companyId)
        {
            return await _repository.GetMilkyMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveMilkyMasterAsync(MilkyMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Milky Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetMilkyMasterListAsync($" AND MilkyId = '{model.MilkyID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Milky id - " + model.MilkyID);
                }

                bool result = await _repository.SaveMilkyMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Milky Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetMilkyMasterListAsync($" AND MilkyId = '{model.MilkyID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Milky id already exists.");
                }

                bool result = await _repository.SaveMilkyMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteMilkyAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteMilkyAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Luster Master
        public async Task<List<LusterMaster>> GetLusterMasterListAsync(string where, string companyId)
        {
            return await _repository.GetLusterMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveLusterMasterAsync(LusterMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Luster Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetLusterMasterListAsync($" AND LusterId = '{model.LusterID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Luster id - " + model.LusterID);
                }

                bool result = await _repository.SaveLusterMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Luster Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetLusterMasterListAsync($" AND LusterId = '{model.LusterID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Luster id already exists.");
                }

                bool result = await _repository.SaveLusterMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteLusterAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteLusterAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Natts Master
        public async Task<List<NattsMaster>> GetNattsMasterListAsync(string where, string companyId)
        {
            return await _repository.GetNattsMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveNattsMasterAsync(NattsMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Natts Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetNattsMasterListAsync($" AND NattsId = '{model.NattsID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Natts id - " + model.NattsID);
                }

                bool result = await _repository.SaveNattsMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Natts Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetNattsMasterListAsync($" AND NattsId = '{model.NattsID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Natts id already exists.");
                }

                bool result = await _repository.SaveNattsMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteNattsAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteNattsAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region HA Master
        public async Task<List<HAMaster>> GetHAMasterListAsync(string where, string companyId)
        {
            return await _repository.GetHAMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveHAMasterAsync(HAMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "H&A Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetHAMasterListAsync($" AND HAId = '{model.HAID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with HA id - " + model.HAID);
                }

                bool result = await _repository.SaveHAMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "H&A Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetHAMasterListAsync($" AND HAId = '{model.HAID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because HA id already exists.");
                }

                bool result = await _repository.SaveHAMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteHAAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteHAAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Shade Master
        public async Task<List<ShadeMaster>> GetShadeMasterListAsync(string where, string companyId)
        {
            return await _repository.GetShadeMasterListAsync(where, companyId);
        }

        public async Task<(bool Success, string Message)> SaveShadeMasterAsync(ShadeMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shade Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetShadeMasterListAsync($" AND ShadeId = '{model.ShadeID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Shade id - " + model.ShadeID);
                }

                bool result = await _repository.SaveShadeMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shade Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetShadeMasterListAsync($" AND ShadeId = '{model.ShadeID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Shade id already exists.");
                }

                bool result = await _repository.SaveShadeMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }

        public async Task<bool> DeleteShadeAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteShadeAsync(id, userTypeId, userId, companyId);
        }
        #endregion
    }
}
