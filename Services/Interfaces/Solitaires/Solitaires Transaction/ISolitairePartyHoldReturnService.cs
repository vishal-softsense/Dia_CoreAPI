using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitairePartyHoldReturnService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<SolitairePartyHoldReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<List<SolitairePartyHoldReturnRowDto>> GetListAsync(SolitairePartyHoldReturnFilterRequest filter, string companyId);
        Task<(bool Success, string Message)> ReturnAsync(SolitairePartyHoldReturnSaveRequest request, string companyId);
    }
}
