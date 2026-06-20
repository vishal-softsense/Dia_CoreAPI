using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;

namespace Dia_CoreAPI.Services.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<SideMenuItemDto>> GetUserSidebarMenuAsync(string userId, string companyId);
    }
}
