using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireSaleReturnService : ISolitaireSaleReturnService
    {
        private readonly ISolitaireSaleReturnRepository _repository;

        public SolitaireSaleReturnService(ISolitaireSaleReturnRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<SolitaireSaleReturnInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<SolitaireSaleReturnPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<SolitaireSaleReturnDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<SolitaireSaleReturnLotDetailDto>> GetLotDetailAsync(SolitaireSaleReturnLotRequest request, string companyId)
            => _repository.GetLotDetailAsync(request, companyId);

        public Task<SolitaireSaleReturnGridViewDto> AddItemInGridAsync(SolitaireSaleReturnAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<SolitaireSaleReturnGridViewDto> GetItemFromListAsync(SolitaireSaleReturnGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<SolitaireSaleReturnGridViewDto> RefreshBackAsync(SolitaireSaleReturnRefreshBackRequest request, string companyId)
            => _repository.RefreshBackAsync(request, companyId);

        public Task<List<SolitaireSaleReturnSaleRowDto>> GetSaleToSaleReturnAsync(string? partyCode, string? invNo, string companyId)
            => _repository.GetSaleToSaleReturnAsync(partyCode, invNo, companyId);

        public Task<SolitaireSaleReturnGridViewDto> FillSaleToSaleReturnAsync(SolitaireSaleReturnFillRequest request, string companyId)
            => _repository.FillSaleToSaleReturnAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(SolitaireSaleReturnSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(SolitaireSaleReturnSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<SolitaireSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);
    }
}
