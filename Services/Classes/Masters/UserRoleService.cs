using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<IEnumerable<UserRoleViewDto>> GetAllRolesAsync(string companyId)
        {
            return await _userRoleRepository.GetAllRolesAsync(companyId);
        }

        public async Task<UserRoleViewDto> GetRoleByIdAsync(int id, string companyId)
        {
            var role = await _userRoleRepository.GetRoleByIdAsync(id, companyId);
            if (role == null)
                throw new Exception("Role not found");

            return role;
        }

        public async Task<bool> SaveRoleAsync(UserRoleDto role, string userId, string companyId)
        {
            bool exists = await _userRoleRepository.CheckRoleNameExistsAsync(role.UserType, companyId);
            if (exists)
                throw new Exception($"Record already available with User Name - {role.UserType}");

            return await _userRoleRepository.SaveRoleAsync(role, userId, companyId);
        }

        public async Task<bool> UpdateRoleAsync(UserRoleDto role, string userId, string companyId)
        {
            bool exists = await _userRoleRepository.CheckRoleNameExistsAsync(role.UserType, companyId, role.UserTypeID);
            if (exists)
                throw new Exception($"Record already available with User Name - {role.UserType}");

            return await _userRoleRepository.UpdateRoleAsync(role, userId, companyId);
        }

        public async Task<bool> DeleteRoleAsync(int id, string companyId)
        {
            var result = await _userRoleRepository.DeleteRoleAsync(id, companyId);
            if (!result)
                throw new Exception("Record is in use, You can not delete record.");
            return true;
        }

        public async Task<IEnumerable<UserPermissionListDto>> GetRolePermissionsAsync(int roleId, string companyId)
        {
            return await _userRoleRepository.GetRolePermissionsAsync(roleId, companyId);
        }

        public async Task<bool> SaveRolePermissionsAsync(int roleId, IEnumerable<UserPermissionSaveDto> permissions, string userId, string companyId)
        {
            return await _userRoleRepository.SaveRolePermissionsAsync(roleId, permissions, userId, companyId);
        }
    }
}
