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
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;

        public InvoiceService(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        private async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        #region Term Master
        public async Task<List<TermMaster>> GetTermMasterListAsync(string where, string companyId)
        {
            return await _repository.GetTermMasterListAsync(where, companyId);
        }

        public async Task<TermMaster> GetTermByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetTermMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitTermDataAsync(TermMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Term Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetTermMasterListAsync($" AND TermId = '{model.TermID}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Term id - " + model.TermID);

                    bool result = await _repository.SaveTermMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Term Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetTermMasterListAsync($" AND TermId = '{model.TermID}' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Term id already exists.");

                    bool result = await _repository.SaveTermMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteTermAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Term Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetTermByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new TermMaster
                {
                    ID = id,
                    TermID = data.TermID,
                    TermName = null,
                    TermPriority = 0
                };

                bool result = await _repository.SaveTermMasterAsync(model, userId, companyId, "delete");
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

        #region Type Master
        public async Task<List<TypeMaster>> GetTypeMasterListAsync(string where, string companyId)
        {
            return await _repository.GetTypeMasterListAsync(where, companyId);
        }

        public async Task<TypeMaster> GetTypeByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetTypeMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitTypeDataAsync(TypeMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Type Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetTypeMasterListAsync($" AND TypeName = '{model.TypeName}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with Type Name - " + model.TypeName);

                    bool result = await _repository.SaveTypeMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Type Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetTypeMasterListAsync($" AND TypeId = '{model.TypeID}' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because Type id already exists.");

                    bool result = await _repository.SaveTypeMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteTypeAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Type Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetTypeByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new TypeMaster
                {
                    ID = id,
                    TypeID = data.TypeID,
                    TypeName = null,
                    TypePriority = 0
                };

                bool result = await _repository.SaveTypeMasterAsync(model, userId, companyId, "delete");
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

        #region SubType Master
        public async Task<List<SubTypeMaster>> GetSubTypeMasterListAsync(string where, string companyId)
        {
            return await _repository.GetSubTypeMasterListAsync(where, companyId);
        }

        public async Task<SubTypeMaster> GetSubTypeByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetSubTypeMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitSubTypeDataAsync(SubTypeMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "SubType Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetSubTypeMasterListAsync($" AND SubTypeName = '{model.SubTypeName}' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with SubType Name - " + model.SubTypeName);

                    bool result = await _repository.SaveSubTypeMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "SubType Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetSubTypeMasterListAsync($" AND SubTypeId = '{model.SubTypeID}' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because SubType id already exists.");

                    bool result = await _repository.SaveSubTypeMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteSubTypeAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "SubType Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetSubTypeByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new SubTypeMaster
                {
                    ID = id,
                    SubTypeID = data.SubTypeID,
                    SubTypeName = null,
                    SubTypePriority = 0
                };

                bool result = await _repository.SaveSubTypeMasterAsync(model, userId, companyId, "delete");
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

        #region Currency Master
        public async Task<List<CurrencyMaster>> GetCurrencyMasterListAsync(string where, string companyId)
        {
            return await _repository.GetCurrencyMasterListAsync(where, companyId);
        }

        public async Task<CurrencyMaster> GetCurrencyByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetCurrencyMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }
        #endregion
    }
}
