using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Services.Interfaces
{
    public interface IMenuSearchService
    {
        Task<List<BasicDataforform>> GetMenuForDropdownAsync(string where, string companyId);
    }
}