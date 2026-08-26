using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireMemoReturnService : ISolitaireMemoReturnService
    {
        private readonly ISolitaireMemoReturnRepository _repository;

        public SolitaireMemoReturnService(ISolitaireMemoReturnRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<SolitaireMemoReturnPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<List<SolitaireMemoReturnRowDto>> GetListAsync(SolitaireMemoReturnFilterRequest filter, string companyId)
            => _repository.GetListAsync(filter ?? new(), companyId);

        public Task<(bool Success, string Message)> ReturnAsync(SolitaireMemoReturnSaveRequest request, string companyId)
            => _repository.ReturnAsync(request ?? new(), companyId);
    }
}
