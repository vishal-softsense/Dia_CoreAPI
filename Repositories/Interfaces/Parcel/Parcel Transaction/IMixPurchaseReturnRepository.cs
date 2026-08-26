using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixPurchaseReturnRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixPurchaseReturnInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<MixPurchaseReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<MixPurchaseReturnDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixPurchaseReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<MixPurchaseReturnGridViewDto> AddItemInGridAsync(MixPurchaseReturnAddLotRequest request, string companyId);
        Task<MixPurchaseReturnGridViewDto> GetItemFromListAsync(MixPurchaseReturnGetItemFromListRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixPurchaseReturnHeaderDto header, List<MixPurchaseReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixPurchaseReturnHeaderDto header, List<MixPurchaseReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixPurchaseReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<object>> GetLotsAsync(string? searchBy, string companyId);
    }
}
