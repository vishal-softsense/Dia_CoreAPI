using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixMemoOutReturnRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixMemoOutReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<List<MixMemoOutReturnRowDto>> GetListAsync(MixMemoOutReturnFilterRequest filter, string companyId);
        Task<(bool Success, string Message)> ReturnAsync(MixMemoOutReturnSaveRequest request, string userId, string companyId);
    }
}
