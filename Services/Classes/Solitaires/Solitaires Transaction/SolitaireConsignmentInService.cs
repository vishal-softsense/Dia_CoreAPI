using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireConsignmentInService : ISolitaireConsignmentInService
    {
        private readonly ISolitaireConsignmentInRepository _repository;

        public SolitaireConsignmentInService(ISolitaireConsignmentInRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<SolitaireConsignmentInInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<SolitaireConsignmentInPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<SolitaireConsignmentInDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<SolitaireConsignmentInLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<SolitaireConsignmentInGridViewDto> AddItemInGridAsync(SolitaireConsignmentInAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(SolitaireConsignmentInSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(SolitaireConsignmentInSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<SolitaireConsignmentInPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);
    }
}
