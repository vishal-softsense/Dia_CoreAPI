using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;

namespace Dia_CoreAPI.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        Task<IEnumerable<SideMenuItemDto>> GetUserSidebarMenuAsync(string userId, string companyId);
    }
}
