using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IAllMasterRepository
    {
        Task<List<AllMasterDto>> GetAllMasterListAsync(string UserTypeId, string companyId);
    }
}