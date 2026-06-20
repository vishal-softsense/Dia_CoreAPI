using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<IEnumerable<SideMenuItemDto>> GetUserSidebarMenuAsync(string userId, string companyId)
        {
            return await _menuRepository.GetUserSidebarMenuAsync(userId, companyId);
        }
    }
}
