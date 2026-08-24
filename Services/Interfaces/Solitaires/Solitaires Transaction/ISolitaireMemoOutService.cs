using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireMemoOutService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitaireMemoOutInvListDto>> GetListAsync(string companyId);
        Task<SolitaireMemoOutPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitaireMemoOutDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitaireMemoOutLotDetailDto>> GetLotDetailAsync(SolitaireMemoOutLotRequest request, string companyId);
        Task<SolitaireMemoOutGridViewDto> AddItemInGridAsync(SolitaireMemoOutAddLotRequest request, string companyId);
        Task<SolitaireMemoOutGridViewDto> GetItemFromListAsync(SolitaireMemoOutGetItemFromListRequest request, string companyId);
        Task<SolitaireMemoOutGridViewDto> RefreshBackAsync(SolitaireMemoOutRefreshBackRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitaireMemoOutSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitaireMemoOutSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitaireMemoOutPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<SolitaireMemoOutShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId);
    }
}
