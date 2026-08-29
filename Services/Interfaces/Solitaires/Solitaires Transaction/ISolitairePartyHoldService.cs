using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePartyHoldService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePartyHoldInvListDto>> GetListAsync(string companyId);
        Task<SolitairePartyHoldPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePartyHoldDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitairePartyHoldLotDetailDto>> GetLotDetailAsync(SolitairePartyHoldLotRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> AddItemInGridAsync(SolitairePartyHoldAddLotRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> GetItemFromListAsync(SolitairePartyHoldGetItemFromListRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> RefreshBackAsync(SolitairePartyHoldRefreshBackRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePartyHoldSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePartyHoldSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitairePartyHoldPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<SolitairePartyHoldShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId);
    }
}
