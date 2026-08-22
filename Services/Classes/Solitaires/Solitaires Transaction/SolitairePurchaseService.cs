using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitairePurchaseService : ISolitairePurchaseService
    {
        private readonly ISolitairePurchaseRepository _repository;

        public SolitairePurchaseService(ISolitairePurchaseRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<SolitairePurchaseInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<SolitairePurchasePageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<SolitairePurchaseDetailDto?> GetDetailAsync(int id, string companyId, string? mode = null)
            => _repository.GetDetailAsync(id, companyId, mode);

        public Task<List<SolitairePurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<List<SolitaireConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId)
            => _repository.GetConsignmentAsync(partyCode, memoNo, companyId);

        public Task<SolitairePurchaseGridViewDto> AddItemInGridAsync(SolitairePurchaseAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(SolitairePurchaseSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(SolitairePurchaseSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<SolitairePurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<SolitairePurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
            => _repository.GetCurrentStatusAsync(invNo, companyId);
    }
}
