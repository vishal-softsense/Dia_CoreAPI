using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRoleViewDto>> GetAllRolesAsync(string companyId);
        Task<UserRoleViewDto> GetRoleByIdAsync(int id, string companyId);
        Task<bool> SaveRoleAsync(UserRoleDto role, string userId, string companyId);
        Task<bool> UpdateRoleAsync(UserRoleDto role, string userId, string companyId);
        Task<bool> DeleteRoleAsync(int id, string companyId);
        Task<IEnumerable<UserPermissionListDto>> GetRolePermissionsAsync(int roleId, string companyId);
        Task<bool> SaveRolePermissionsAsync(int roleId, IEnumerable<UserPermissionSaveDto> permissions, string userId, string companyId);
    }
}
