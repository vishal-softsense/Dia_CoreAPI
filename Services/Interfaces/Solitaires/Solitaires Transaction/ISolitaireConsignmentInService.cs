using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireConsignmentInService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitaireConsignmentInInvListDto>> GetListAsync(string companyId);
        Task<SolitaireConsignmentInPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitaireConsignmentInDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitaireConsignmentInLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<SolitaireConsignmentInGridViewDto> AddItemInGridAsync(SolitaireConsignmentInAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitaireConsignmentInSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitaireConsignmentInSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitaireConsignmentInPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
    }
}
