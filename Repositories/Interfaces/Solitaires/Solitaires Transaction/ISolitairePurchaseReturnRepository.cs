using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePurchaseReturnRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePurchaseReturnInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<SolitairePurchaseReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePurchaseReturnDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitairePurchaseReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<SolitairePurchaseReturnGridViewDto> AddItemInGridAsync(SolitairePurchaseReturnAddLotRequest request, string companyId);
        Task<SolitairePurchaseReturnGridViewDto> GetItemFromListAsync(SolitairePurchaseReturnGetItemFromListRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePurchaseReturnHeaderDto header, List<SolitairePurchaseReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePurchaseReturnHeaderDto header, List<SolitairePurchaseReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
    }
}
