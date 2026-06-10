using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IUserMasterRepository
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync(string companyId);
        Task<UserMasterDto?> GetUserByIdAsync(int userId, string companyId);
        Task<bool> CheckUsernameExistsAsync(string username, string companyId, int? excludeUserId = null);
        Task<int> CreateUserAsync(UserMasterDto user, string createdBy);
        Task<bool> UpdateUserAsync(UserMasterDto user, string updatedBy);
        Task<bool> DeleteUserAsync(int userId, string companyId);
        Task<IEnumerable<DropdownDto>> GetUserDropdownAsync(string companyId);
    }
}
