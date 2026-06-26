using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Classes.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class _4CMasterService : I4CMasterService
    {
        private readonly I4CMasterRepository _repository;

        public _4CMasterService(I4CMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId,string formName,string actionType,string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId,formName,actionType,companyId);
        }

        #region Shape Master
        public async Task<List<ShapeMaster>> GetShapeMasterListAsync(string where,string companyId)
        {
            return await _repository.GetShapeMasterListAsync(where,companyId);
        }
        public async Task<(bool Success, string Message)> SaveShapeMasterAsync(ShapeMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with shape id - " + model.ShapeID);
                }

                bool result = await _repository.SaveShapeMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because shape id already exists.");
                }

                bool result = await _repository.SaveShapeMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteShapeAsync(int id,string userTypeId,string userId,string companyId)
        {
            return await _repository.DeleteShapeAsync(id,userTypeId,userId,companyId);
        }

        #endregion

        #region Color Master
        public async Task<List<ColorMaster>> GetColorMasterListAsync(string where,string companyId)
        {
            return await _repository.GetColorMasterListAsync(where,companyId);
        }
        public async Task<(bool Success, string Message)> SaveColorMasterAsync(ColorMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Color Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetColorMasterListAsync($" AND ColorId = '{model.ColorID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Color id - " + model.ColorID);
                }

                bool result = await _repository.SaveColorMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Color Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetColorMasterListAsync($" AND ColorId = '{model.ColorID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Color id already exists.");
                }

                bool result = await _repository.SaveColorMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteColorAsync(int id,string userTypeId,string userId,string companyId)
        {
            return await _repository.DeleteColorAsync(id,userTypeId,userId,companyId);
        }

        #endregion

        #region Size Master
        public async Task<List<SizeMaster>> GetSizeMasterListAsync(string where, string companyId)
        {
            return await _repository.GetSizeMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveSizeMasterAsync(SizeMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Size Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSizeMasterListAsync($" AND SizeId = '{model.SizeID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Size id - " + model.SizeID);
                }

                bool result = await _repository.SaveSizeMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Size Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSizeMasterListAsync($" AND SizeId = '{model.SizeID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Size id already exists.");
                }

                bool result = await _repository.SaveSizeMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteSizeAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteSizeAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Clarity Master
        public async Task<List<ClarityMaster>> GetClarityMasterListAsync(string where, string companyId)
        {
            return await _repository.GetClarityMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveClarityMasterAsync(ClarityMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Clarity Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetClarityMasterListAsync($" AND ClarityId = '{model.ClarityID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Clarity id - " + model.ClarityID);
                }

                bool result = await _repository.SaveClarityMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Clarity Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetClarityMasterListAsync($" AND ClarityId = '{model.ClarityID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Clarity id already exists.");
                }

                bool result = await _repository.SaveClarityMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteClarityAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteClarityAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Sieve Master
        public async Task<List<SieveMaster>> GetSieveMasterListAsync(string where, string companyId)
        {
            return await _repository.GetSieveMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveSieveMasterAsync(SieveMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Sieve Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSieveMasterListAsync($" AND SieveId = '{model.SieveID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Sieve id - " + model.SieveID);
                }

                bool result = await _repository.SaveSieveMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Sieve Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSieveMasterListAsync($" AND SieveId = '{model.SieveID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Sieve id already exists.");
                }

                bool result = await _repository.SaveSieveMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteSieveAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteSieveAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region LAB Master
        public async Task<List<LABMaster>> GetLABMasterListAsync(string where, string companyId)
        {
            return await _repository.GetLABMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveLABMasterAsync(LABMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "LAB Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetLABMasterListAsync($" AND CertId = '{model.CertID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Cert id - " + model.CertID);
                }

                bool result = await _repository.SaveLABMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "LAB Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetLABMasterListAsync($" AND CertId = '{model.CertID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Cert id already exists.");
                }

                bool result = await _repository.SaveLABMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteLABAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteLABAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Prop Master
        public async Task<List<PropMaster>> GetPropMasterListAsync(string where, string companyId)
        {
            return await _repository.GetPropMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SavePropMasterAsync(PropMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Proportion Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetPropMasterListAsync($" AND PropId = '{model.PropID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Prop id - " + model.PropID);
                }

                bool result = await _repository.SavePropMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Proportion Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetPropMasterListAsync($" AND PropId = '{model.PropID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Prop id already exists.");
                }

                bool result = await _repository.SavePropMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeletePropAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeletePropAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Polish Master
        public async Task<List<PolishMaster>> GetPolishMasterListAsync(string where, string companyId)
        {
            return await _repository.GetPolishMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SavePolishMasterAsync(PolishMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Polish Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetPolishMasterListAsync($" AND PolishId = '{model.PolishID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Polish id - " + model.PolishID);
                }

                bool result = await _repository.SavePolishMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Polish Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetPolishMasterListAsync($" AND PolishId = '{model.PolishID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Polish id already exists.");
                }

                bool result = await _repository.SavePolishMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeletePolishAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeletePolishAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region Sym Master
        public async Task<List<SymMaster>> GetSymMasterListAsync(string where, string companyId)
        {
            return await _repository.GetSymMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveSymMasterAsync(SymMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Symmetry Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSymMasterListAsync($" AND SymId = '{model.SymID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with Sym id - " + model.SymID);
                }

                bool result = await _repository.SaveSymMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Symmetry Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetSymMasterListAsync($" AND SymId = '{model.SymID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because Sym id already exists.");
                }

                bool result = await _repository.SaveSymMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteSymAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteSymAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region FL Master
        public async Task<List<FLMaster>> GetFLMasterListAsync(string where, string companyId)
        {
            return await _repository.GetFLMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveFLMasterAsync(FLMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetFLMasterListAsync($" AND FLId = '{model.FLID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with FL id - " + model.FLID);
                }

                bool result = await _repository.SaveFLMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetFLMasterListAsync($" AND FLId = '{model.FLID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because FL id already exists.");
                }

                bool result = await _repository.SaveFLMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteFLAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteFLAsync(id, userTypeId, userId, companyId);
        }
        #endregion

        #region FLColor Master
        public async Task<List<FLColorMaster>> GetFLColorMasterListAsync(string where, string companyId)
        {
            return await _repository.GetFLColorMasterListAsync(where, companyId);
        }
        public async Task<(bool Success, string Message)> SaveFLColorMasterAsync(FLColorMaster model, string userTypeId, string userId, string companyId)
        {
            if (model.ID == 0)
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Color Master", "insert", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetFLColorMasterListAsync($" AND FLColorId = '{model.FLColorID}' ", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Record already available with FLColor id - " + model.FLColorID);
                }

                bool result = await _repository.SaveFLColorMasterAsync(model, userId, companyId, "insert");
                return (result, "Record saved successfully.");
            }
            else
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Fluorescence Color Master", "edit", companyId);
                if (permission == 0)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var duplicateData = await GetFLColorMasterListAsync($" AND FLColorId = '{model.FLColorID}' AND ID <> {model.ID}", companyId);
                if (duplicateData.Count > 0)
                {
                    return (false, "Sorry.! You can not update record because FLColor id already exists.");
                }

                bool result = await _repository.SaveFLColorMasterAsync(model, userId, companyId, "update");
                return (result, "Record updated successfully.");
            }
        }
        public async Task<bool> DeleteFLColorAsync(int id, string userTypeId, string userId, string companyId)
        {
            return await _repository.DeleteFLColorAsync(id, userTypeId, userId, companyId);
        }
        #endregion

    }
}