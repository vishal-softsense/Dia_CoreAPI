using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixConsignmentReturnRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixConsignmentReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<List<MixConsignmentReturnRowDto>> GetListAsync(MixConsignmentReturnFilterRequest filter, string companyId);
        Task<(bool Success, string Message)> ReturnAsync(MixConsignmentReturnSaveRequest request, string userId, string companyId);
    }
}
