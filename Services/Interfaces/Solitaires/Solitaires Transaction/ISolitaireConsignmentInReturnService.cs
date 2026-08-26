using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireConsignmentInReturnService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<SolitaireConsignmentInReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<List<SolitaireConsignmentInReturnRowDto>> GetListAsync(SolitaireConsignmentInReturnFilterRequest filter, string companyId);
        Task<(bool Success, string Message)> ReturnAsync(SolitaireConsignmentInReturnSaveRequest request, string companyId);
    }
}
