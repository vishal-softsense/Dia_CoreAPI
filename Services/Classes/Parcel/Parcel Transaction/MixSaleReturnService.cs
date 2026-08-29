using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class MixSaleReturnService : IMixSaleReturnService
    {
        private readonly IMixSaleReturnRepository _repository;

        public MixSaleReturnService(IMixSaleReturnRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<MixSaleReturnInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<MixSaleReturnPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<MixSaleReturnDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<MixSaleReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<MixSaleReturnGridViewDto> AddItemInGridAsync(MixSaleReturnAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<MixSaleReturnGridViewDto> GetItemFromListAsync(MixSaleReturnGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(MixSaleReturnSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(MixSaleReturnSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<MixSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<List<object>> GetLotsAsync(string? searchBy, string companyId)
            => _repository.GetLotsAsync(searchBy, companyId);

        public Task<List<MixSaleReturnCSaleLineDto>> GetCSaleForCSaleRtnAsync(string? partyCode, string? memoNo, string companyId)
            => _repository.GetCSaleForCSaleRtnAsync(partyCode, memoNo, companyId);

        public Task<MixSaleReturnGridViewDto> FillFromCSaleAsync(List<MixSaleReturnCSaleFillLineRequest> lines, List<MixSaleReturnItemDto> existingItems, string companyId)
            => _repository.FillFromCSaleAsync(lines, existingItems, companyId);
    }
}
