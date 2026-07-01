using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces;

namespace Dia_CoreAPI.Repositories
{
    public class MenuSearchRepository : IMenuSearchRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public MenuSearchRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<List<BasicDataforform>> GetMenuForDropdownAsync(
            string where,
            string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@strWhere", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<BasicDataforform>(
                "GetmenuForDropdown",
                param,
                companyId);

            return result.ToList();
        }
    }
}