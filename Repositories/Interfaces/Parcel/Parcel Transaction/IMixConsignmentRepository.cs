using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixConsignmentRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixConsignmentInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<MixConsignmentPageDataDto> GetPageDataAsync(string companyId);
        Task<MixConsignmentDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixConsignmentLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<MixConsignmentGridViewDto> AddItemInGridAsync(MixConsignmentAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixConsignmentHeaderDto header, List<MixConsignmentItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixConsignmentHeaderDto header, List<MixConsignmentItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixConsignmentPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<MixConsignmentStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
        Task<List<object>> GetLotsAsync(string? searchBy, string companyId);
        Task<string> GetMixLotNoLMAsync(string companyId);
    }
}
