using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services
{
    public class MenuSearchService : IMenuSearchService
    {
        private readonly IMenuSearchRepository _repository;

        public MenuSearchService(IMenuSearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BasicDataforform>> GetMenuForDropdownAsync(string where, string companyId)
        {
            return await _repository.GetMenuForDropdownAsync(where,companyId);
        }
    }
}