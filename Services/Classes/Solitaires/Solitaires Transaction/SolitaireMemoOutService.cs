using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireMemoOutService : ISolitaireMemoOutService
    {
        private readonly ISolitaireMemoOutRepository _repository;

        public SolitaireMemoOutService(ISolitaireMemoOutRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<SolitaireMemoOutInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<SolitaireMemoOutPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<SolitaireMemoOutDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<SolitaireMemoOutLotDetailDto>> GetLotDetailAsync(SolitaireMemoOutLotRequest request, string companyId)
            => _repository.GetLotDetailAsync(request, companyId);

        public Task<SolitaireMemoOutGridViewDto> AddItemInGridAsync(SolitaireMemoOutAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<SolitaireMemoOutGridViewDto> GetItemFromListAsync(SolitaireMemoOutGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<SolitaireMemoOutGridViewDto> RefreshBackAsync(SolitaireMemoOutRefreshBackRequest request, string companyId)
            => _repository.RefreshBackAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(SolitaireMemoOutSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(SolitaireMemoOutSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<SolitaireMemoOutPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<List<SolitaireMemoOutShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId)
            => _repository.GetShipViaAsync(searchBy, companyId);
    }
}
