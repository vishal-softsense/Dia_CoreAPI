using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixMemoOutRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixMemoOutInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<MixMemoOutPageDataDto> GetPageDataAsync(string companyId);
        Task<MixMemoOutDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixMemoOutLotDetailDto>> GetLotDetailAsync(MixMemoOutLotRequest request, string companyId);
        Task<MixMemoOutGridViewDto> AddItemInGridAsync(MixMemoOutAddLotRequest request, string companyId);
        Task<MixMemoOutGridViewDto> GetItemFromListAsync(MixMemoOutGetItemFromListRequest request, string companyId);
        Task<MixMemoOutGridViewDto> RefreshBackAsync(MixMemoOutRefreshBackRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixMemoOutHeaderDto header, List<MixMemoOutItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixMemoOutHeaderDto header, List<MixMemoOutItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixMemoOutPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<MixMemoOutShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId);
        Task<List<MixMemoOutLotOptionDto>> GetLotsAsync(string? searchBy, string companyId);
    }
}
