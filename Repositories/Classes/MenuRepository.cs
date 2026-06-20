using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.DataAccess;

namespace Dia_CoreAPI.Repositories.Implementations
{
    public class MenuRepository : IMenuRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MenuRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<SideMenuItemDto>> GetUserSidebarMenuAsync(string userId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@UserID", userId);

            // Fetch the side menu items using the stored procedure from the original MVC codebase
            return await _sqlDataAccess.SelectDataFromSP<SideMenuItemDto>(
                "GetPermissionWiseMnuList",
                param,
                companyId);
        }
    }
}
