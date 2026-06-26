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
    public class JewelleryService : IJewelleryService
    {
        private readonly IJewelleryRepository _repository;

        public JewelleryService(IJewelleryRepository repository)
        {
            _repository = repository;
        }

        private async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        #region Jewellery Category Master
        public async Task<List<JewCategoryMaster>> GetJewCategoryMasterListAsync(string where, string companyId)
        {
            return await _repository.GetJewCategoryMasterListAsync(where, companyId);
        }

        public async Task<JewCategoryMaster> GetJewCategoryByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetJewCategoryMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitJewCategoryDataAsync(JewCategoryMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Category Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewCategoryMasterListAsync($" AND CategoryID = ''{model.CategoryID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with category id - " + model.CategoryID);

                    bool result = await _repository.SaveJewCategoryMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Category Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewCategoryMasterListAsync($" AND CategoryID = ''{model.CategoryID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because category id already exists.");

                    bool result = await _repository.SaveJewCategoryMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJewCategoryAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetJewCategoryByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new JewCategoryMaster
                {
                    ID = id,
                    CategoryID = data.CategoryID,
                    CategoryName = "",
                    CategoryPriority = 0
                };

                bool result = await _repository.SaveJewCategoryMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Message.Contains("REFERENCE constraint"))
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

        #region Jewellery SubCategory Master
        public async Task<List<JewSubCategoryMaster>> GetJewSubCategoryMasterListAsync(string where, string companyId)
        {
            return await _repository.GetJewSubCategoryMasterListAsync(where, companyId);
        }

        public async Task<JewSubCategoryMaster> GetJewSubCategoryByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetJewSubCategoryMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitJewSubCategoryDataAsync(JewSubCategoryMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew SubCategory Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewSubCategoryMasterListAsync($" AND SubCategoryID = ''{model.SubCategoryID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with subcategory id - " + model.SubCategoryID);

                    bool result = await _repository.SaveJewSubCategoryMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew SubCategory Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewSubCategoryMasterListAsync($" AND SubCategoryID = ''{model.SubCategoryID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because subcategory id already exists.");

                    bool result = await _repository.SaveJewSubCategoryMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJewSubCategoryAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetJewSubCategoryByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new JewSubCategoryMaster
                {
                    ID = id,
                    SubCategoryID = data.SubCategoryID,
                    SubCategoryName = "",
                    SubCategoryPriority = 0,
                    CategoryID = 0
                };

                bool result = await _repository.SaveJewSubCategoryMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Message.Contains("REFERENCE constraint"))
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

        #region Jewellery Metal Master
        public async Task<List<JewMetalMaster>> GetJewMetalMasterListAsync(string where, string companyId)
        {
            return await _repository.GetJewMetalMasterListAsync(where, companyId);
        }

        public async Task<JewMetalMaster> GetJewMetalByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetJewMetalMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitJewMetalDataAsync(JewMetalMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewMetalMasterListAsync($" AND MetalID = ''{model.MetalID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with metal id - " + model.MetalID);

                    bool result = await _repository.SaveJewMetalMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewMetalMasterListAsync($" AND MetalID = ''{model.MetalID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because metal id already exists.");

                    bool result = await _repository.SaveJewMetalMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJewMetalAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetJewMetalByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new JewMetalMaster
                {
                    ID = id,
                    MetalID = data.MetalID,
                    MetalName = "",
                    MetalPriority = 0,
                    MetalType = null,
                    MetalColor = null,
                    MetalPurity = null,
                    Metal24KTRatio = 0
                };

                bool result = await _repository.SaveJewMetalMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Message.Contains("REFERENCE constraint"))
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

        public async Task<List<GetJewPurity>> GetPurityDataAsync(string where, string companyId)
        {
            return await _repository.GetPurityDataAsync(where, companyId);
        }
        #endregion

        #region Jewellery Metal Purity Master
        public async Task<List<JewMetalPurityMaster>> GetJewMetalPurityMasterListAsync(string where, string companyId)
        {
            return await _repository.GetJewMetalPurityMasterListAsync(where, companyId);
        }

        public async Task<JewMetalPurityMaster> GetJewMetalPurityByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetJewMetalPurityMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitJewMetalPurityDataAsync(JewMetalPurityMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Purity Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewMetalPurityMasterListAsync($" AND MetalPurityID = ''{model.MetalPurityID}'' AND Metal = ''{model.Metal}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with metal purity id - " + model.MetalPurityID);

                    bool result = await _repository.SaveJewMetalPurityMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Purity Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewMetalPurityMasterListAsync($" AND MetalPurityID = ''{model.MetalPurityID}'' AND Metal = ''{model.Metal}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because metal purity id already exists.");

                    bool result = await _repository.SaveJewMetalPurityMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJewMetalPurityAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Jew Metal Purity Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetJewMetalPurityByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new JewMetalPurityMaster
                {
                    ID = id,
                    MetalPurityID = data.MetalPurityID,
                    MetalPurityName = "",
                    MetalPurityPriority = 0,
                    MetalPurityToPureRatio = 0,
                    Metal = null
                };

                bool result = await _repository.SaveJewMetalPurityMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Message.Contains("REFERENCE constraint"))
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

        #region Jewellery Color Master
        public async Task<List<JewColorMaster>> GetJewColorMasterListAsync(string where, string companyId)
        {
            return await _repository.GetJewColorMasterListAsync(where, companyId);
        }

        public async Task<JewColorMaster> GetJewColorByIdAsync(int id, string companyId)
        {
            var list = await _repository.GetJewColorMasterListAsync($" AND ID = {id} ", companyId);
            return list.SingleOrDefault();
        }

        public async Task<(bool Success, string Message)> SubmitJewColorDataAsync(JewColorMaster model, string userTypeId, string userId, string companyId)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Color Master", "insert", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewColorMasterListAsync($" AND JewColorID = ''{model.JewColorID}'' ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Record already available with jewcolor id - " + model.JewColorID);

                    bool result = await _repository.SaveJewColorMasterAsync(model, userId, companyId, "insert");
                    return (result, result ? "Record saved successfully." : "Failed to save record.");
                }
                else
                {
                    int permission = await CheckUserPermissionAsync(userTypeId, "Jew Color Master", "edit", companyId);
                    if (permission != 1)
                        return (false, "You are not authorized to perform this task.");

                    var duplicate = await _repository.GetJewColorMasterListAsync($" AND JewColorID = ''{model.JewColorID}'' AND ID != {model.ID} ", companyId);
                    if (duplicate.Count > 0)
                        return (false, "Sorry.! You can not update record because jewcolor id already exists.");

                    bool result = await _repository.SaveJewColorMasterAsync(model, userId, companyId, "update");
                    return (result, result ? "Record updated successfully." : "Failed to update record.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJewColorAsync(int id, string userTypeId, string userId, string companyId)
        {
            try
            {
                int permission = await CheckUserPermissionAsync(userTypeId, "Shape Master", "delete", companyId);
                if (permission != 1)
                    return (false, "You are not authorized to perform this task.");

                var data = await GetJewColorByIdAsync(id, companyId);
                if (data == null)
                    return (false, "Record not found.");

                var model = new JewColorMaster
                {
                    ID = id,
                    JewColorID = data.JewColorID,
                    JewColorName = "",
                    JewColorPriority = 0
                };

                bool result = await _repository.SaveJewColorMasterAsync(model, userId, companyId, "delete");
                return (result, result ? "Records Deleted." : "Record is in use, You can not delete record.");
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Message.Contains("REFERENCE constraint"))
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
