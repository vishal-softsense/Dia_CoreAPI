using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePurchaseRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePurchaseInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<SolitairePurchasePageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePurchaseDetailDto?> GetDetailAsync(int id, string companyId, string? mode = null);
        Task<List<SolitairePurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<List<SolitaireConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId);
        Task<SolitairePurchaseGridViewDto> AddItemInGridAsync(SolitairePurchaseAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePurchaseHeaderDto header, List<SolitairePurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePurchaseHeaderDto header, List<SolitairePurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitairePurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<SolitairePurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
    }
}
