using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireMemoReturnService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<SolitaireMemoReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<List<SolitaireMemoReturnRowDto>> GetListAsync(SolitaireMemoReturnFilterRequest filter, string companyId);
        Task<(bool Success, string Message)> ReturnAsync(SolitaireMemoReturnSaveRequest request, string companyId);
    }
}
