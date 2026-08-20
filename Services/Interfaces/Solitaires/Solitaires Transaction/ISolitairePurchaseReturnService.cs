using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePurchaseReturnService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePurchaseReturnInvListDto>> GetListAsync(string companyId);
        Task<SolitairePurchaseReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePurchaseReturnDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitairePurchaseReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<SolitairePurchaseReturnGridViewDto> AddItemInGridAsync(SolitairePurchaseReturnAddLotRequest request, string companyId);
        Task<SolitairePurchaseReturnGridViewDto> GetItemFromListAsync(SolitairePurchaseReturnGetItemFromListRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePurchaseReturnSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePurchaseReturnSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
    }
}
