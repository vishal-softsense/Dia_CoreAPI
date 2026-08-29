using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitairePartyHoldService : ISolitairePartyHoldService
    {
        private readonly ISolitairePartyHoldRepository _repository;

        public SolitairePartyHoldService(ISolitairePartyHoldRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<SolitairePartyHoldInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<SolitairePartyHoldPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<SolitairePartyHoldDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<SolitairePartyHoldLotDetailDto>> GetLotDetailAsync(SolitairePartyHoldLotRequest request, string companyId)
            => _repository.GetLotDetailAsync(request, companyId);

        public Task<SolitairePartyHoldGridViewDto> AddItemInGridAsync(SolitairePartyHoldAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<SolitairePartyHoldGridViewDto> GetItemFromListAsync(SolitairePartyHoldGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<SolitairePartyHoldGridViewDto> RefreshBackAsync(SolitairePartyHoldRefreshBackRequest request, string companyId)
            => _repository.RefreshBackAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(SolitairePartyHoldSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(SolitairePartyHoldSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<SolitairePartyHoldPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<List<SolitairePartyHoldShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId)
            => _repository.GetShipViaAsync(searchBy, companyId);
    }
}
