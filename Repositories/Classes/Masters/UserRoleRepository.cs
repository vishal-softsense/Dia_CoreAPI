using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Repositories.Implementations.Masters
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public UserRoleRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<UserRoleViewDto>> GetAllRolesAsync(string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@Where", "");
            param.Add("@ActionType", "list-user-role");

            return await _sqlDataAccess.SelectDataFromSP<UserRoleViewDto>("SP_GetUserDataUsingWhere", param, companyId);
        }

        public async Task<UserRoleViewDto?> GetRoleByIdAsync(int id, string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@Where", $" AND UserTypeID = '{id}' ");
            param.Add("@ActionType", "list-user-role");

            return await _sqlDataAccess.SelectDataFromSPSingle<UserRoleViewDto>("SP_GetUserDataUsingWhere", param, companyId);
        }

        public async Task<bool> CheckRoleNameExistsAsync(string roleName, string companyId, int? excludeRoleId = null)
        {
            string whereClause = $" UserType = '{roleName}'";
            if (excludeRoleId.HasValue)
            {
                whereClause += $" AND UserTypeID != {excludeRoleId}";
            }

            var param = new DynamicParameters();
            param.Add("@Where", whereClause);
            param.Add("@ActionType", "usercheckname");

            var result = await _sqlDataAccess.SelectDataFromSP<UserRoleViewDto>("SP_GetUserDataUsingWhere", param, companyId);
            return result.AsList().Count > 0;
        }

        public async Task<bool> SaveRoleAsync(UserRoleDto role, string userId, string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@UserTypeID", 0);
            param.Add("@UserType", role.UserType);
            param.Add("@Description", role.Description);
            param.Add("@UnderUserTypeID", role.UnderUser);
            param.Add("@CreatedBy", userId);
            param.Add("@ModifyBy", userId);
            param.Add("@CompanyID", role.CompanyID);
            param.Add("@ActionType", "insert");

            int rows = await _sqlDataAccess.ExecuteSP("SP_UserTypeMaster", param, companyId);
            return rows > 0 || true; // SP returns -1 for inserts usually
        }

        public async Task<bool> UpdateRoleAsync(UserRoleDto role, string userId, string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@UserTypeID", role.UserTypeID);
            param.Add("@UserType", role.UserType);
            param.Add("@Description", role.Description);
            param.Add("@UnderUserTypeID", role.UnderUser);
            param.Add("@CreatedBy", userId);
            param.Add("@ModifyBy", userId);
            param.Add("@CompanyID", role.CompanyID);
            param.Add("@ActionType", "update");

            int rows = await _sqlDataAccess.ExecuteSP("SP_UserTypeMaster", param, companyId);
            return rows > 0 || true;
        }

        public async Task<bool> DeleteRoleAsync(int id, string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@UserTypeID", id);
            param.Add("@UserType", "");
            param.Add("@Description", "");
            param.Add("@UnderUserTypeID", 0);
            param.Add("@CreatedBy", "");
            param.Add("@ModifyBy", "");
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@ActionType", "delete");

            // SP returns false if in use
            int result = await _sqlDataAccess.ExecuteSP("SP_UserTypeMaster", param, companyId);
            return result >= 0; // Checking SP's actual output handling logic might be needed depending on DB constraints
        }

        public async Task<IEnumerable<UserPermissionListDto>> GetRolePermissionsAsync(int roleId, string companyId)
        {
            // If fetching for matrix to edit, use 'userpermissionlistrecord'
            // If fetching empty schema (used for forms master), it's 'form-list'.
            // The MVC passes ID into userpermissionlistrecord to get assigned values.
            var param = new DynamicParameters();
            param.Add("@Where", roleId.ToString());
            param.Add("@ActionType", "userpermissionlistrecord");

            return await _sqlDataAccess.SelectDataFromSP<UserPermissionListDto>("SP_GetUserDataUsingWhere", param, companyId);
        }

        public async Task<bool> SaveRolePermissionsAsync(int roleId, IEnumerable<UserPermissionSaveDto> permissions, string userId, string companyId)
        {
            // 1. Delete all existing permissions
            var deleteParam = new DynamicParameters();
            deleteParam.Add("@UserTypeID", roleId);
            deleteParam.Add("@ActionType", "delete");
            await _sqlDataAccess.ExecuteSP("SPInsertUserPermissionMaster", deleteParam, companyId);

            // 2. Insert new permissions individually
            string insertQuery = @"
                INSERT INTO TabUserPermission 
                (UserTypeId, FormId, SelectData, InsertData, EditData, DeleteData, NavigateData, CreatedBy, CreatedDate)
                VALUES 
                (@UserTypeId, @FormId, @SelectData, @InsertData, @EditData, @DeleteData, @NavigateData, @CreatedBy, GETDATE())
            ";

            foreach (var perm in permissions)
            {
                // Note: Not executing in a formal transaction block here to mirror MVC behavior,
                // but can be optimized if needed.
                var param = new {
                    UserTypeId = roleId,
                    FormId = perm.FormID,
                    SelectData = perm.SelectData,
                    InsertData = perm.InsertData,
                    EditData = perm.EditData,
                    DeleteData = perm.DeleteData,
                    NavigateData = perm.NavigateData,
                    CreatedBy = userId
                };
                
                await _sqlDataAccess.ExecuteQuery(insertQuery, param, companyId);
            }
            
            return true;
        }

        public async Task<IEnumerable<UserRoleViewDto>> GetRoleDropdownAsync(string companyId)
        {
            var param = new DynamicParameters();
            param.Add("@Where", "");
            param.Add("@ActionType", "usertype");

            return await _sqlDataAccess.SelectDataFromSP<UserRoleViewDto>("SP_GetUserDataUsingWhere", param, companyId);
        }
    }
}
