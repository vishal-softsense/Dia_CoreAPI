using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Repositories.Interfaces
{
    public interface IMenuSearchRepository
    {
        Task<List<BasicDataforform>> GetMenuForDropdownAsync(string where, string companyId);
    }
}
