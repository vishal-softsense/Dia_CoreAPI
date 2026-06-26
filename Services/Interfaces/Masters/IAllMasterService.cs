using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IAllMasterService
    {
        Task<List<AllMasterDto>> 
            GetAllMasterListAsync(string UserTypeId, string companyId);
    }
}