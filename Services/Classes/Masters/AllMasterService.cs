using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class AllMasterService : IAllMasterService
    {
        private readonly IAllMasterRepository _repository;

        public AllMasterService(IAllMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AllMasterDto>> GetAllMasterListAsync(string UserTypeId, string companyId)
        {
            //return await _repository.GetAllMasterListAsync(UserTypeId);
            return await _repository.GetAllMasterListAsync(UserTypeId, companyId);
        }
    }
}