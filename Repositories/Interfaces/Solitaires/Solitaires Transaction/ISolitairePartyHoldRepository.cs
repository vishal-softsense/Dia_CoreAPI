using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePartyHoldRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitairePartyHoldInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<SolitairePartyHoldPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitairePartyHoldDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitairePartyHoldLotDetailDto>> GetLotDetailAsync(SolitairePartyHoldLotRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> AddItemInGridAsync(SolitairePartyHoldAddLotRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> GetItemFromListAsync(SolitairePartyHoldGetItemFromListRequest request, string companyId);
        Task<SolitairePartyHoldGridViewDto> RefreshBackAsync(SolitairePartyHoldRefreshBackRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitairePartyHoldHeaderDto header, List<SolitairePartyHoldItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitairePartyHoldHeaderDto header, List<SolitairePartyHoldItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitairePartyHoldPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<SolitairePartyHoldShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId);
    }
}
