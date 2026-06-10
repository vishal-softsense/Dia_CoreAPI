using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IUserMasterService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync(string companyId);
        Task<UserMasterDto?> GetUserByIdAsync(int userId, string companyId);
        Task<(bool Success, string Message)> CreateUserAsync(UserMasterDto user, string createdBy);
        Task<(bool Success, string Message)> UpdateUserAsync(UserMasterDto user, string updatedBy);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId, string companyId);
        Task<IEnumerable<DropdownDto>> GetUserDropdownAsync(string companyId);
    }
}
