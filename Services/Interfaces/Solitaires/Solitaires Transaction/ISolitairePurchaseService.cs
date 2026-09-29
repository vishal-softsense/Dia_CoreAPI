using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePurchaseService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePurchaseInvListDto>> GetListAsync(string companyId);
        Task<SolitairePurchasePageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePurchaseDetailDto?> GetDetailAsync(int id, string companyId, string? mode = null);
        Task<List<SolitairePurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<List<SolitaireConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId);
        Task<SolitairePurchaseGridViewDto> AddItemInGridAsync(SolitairePurchaseAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePurchaseSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePurchaseSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitairePurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<SolitairePurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
        Task<float> GetRapRateAsync(string shape, string color, string clarity, double carat, string companyId);
    }
}
