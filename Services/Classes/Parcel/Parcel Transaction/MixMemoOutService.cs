using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class MixMemoOutService : IMixMemoOutService
    {
        private readonly IMixMemoOutRepository _repository;

        public MixMemoOutService(IMixMemoOutRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<MixMemoOutInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<MixMemoOutPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<MixMemoOutDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<MixMemoOutLotDetailDto>> GetLotDetailAsync(MixMemoOutLotRequest request, string companyId)
            => _repository.GetLotDetailAsync(request, companyId);

        public Task<MixMemoOutGridViewDto> AddItemInGridAsync(MixMemoOutAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<MixMemoOutGridViewDto> GetItemFromListAsync(MixMemoOutGetItemFromListRequest request, string companyId)
            => _repository.GetItemFromListAsync(request, companyId);

        public Task<MixMemoOutGridViewDto> RefreshBackAsync(MixMemoOutRefreshBackRequest request, string companyId)
            => _repository.RefreshBackAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(MixMemoOutSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(MixMemoOutSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<MixMemoOutPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<List<MixMemoOutShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId)
            => _repository.GetShipViaAsync(searchBy, companyId);

        public Task<List<MixMemoOutLotOptionDto>> GetLotsAsync(string? searchBy, string companyId)
            => _repository.GetLotsAsync(searchBy, companyId);
    }
}
