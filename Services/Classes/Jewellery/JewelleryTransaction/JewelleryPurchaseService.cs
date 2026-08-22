using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryTransaction;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryTransaction;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryTransaction;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryTransaction
{
    public class JewelleryPurchaseService : IJewelleryPurchaseService
    {
        private readonly IJewelleryPurchaseRepository _repository;

        public JewelleryPurchaseService(IJewelleryPurchaseRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<JewelleryPurchaseInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<JewelleryPurchasePageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<JewelleryPurchaseDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<JewelleryPurchaseStockOptionDto>> SearchStockAsync(string searchBy, string companyId)
            => _repository.SearchStockAsync(searchBy, companyId);

        public Task<List<JewelleryPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<JewelleryPurchaseGridViewDto> AddItemInGridAsync(JewelleryPurchaseAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<JewelleryPurchaseGridViewDto> GetItemFromListAsync(JewelleryPurchaseGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<List<JewelleryPurchaseConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId)
            => _repository.GetConsignmentAsync(partyCode, memoNo, companyId);

        public Task<(bool Success, string Message)> AddAsync(JewelleryPurchaseSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(JewelleryPurchaseSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<JewelleryPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<JewelleryPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
            => _repository.GetCurrentStatusAsync(invNo, companyId);
    }
}
