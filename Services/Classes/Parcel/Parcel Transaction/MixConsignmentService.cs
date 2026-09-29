using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class MixConsignmentService : IMixConsignmentService
    {
        private readonly IMixConsignmentRepository _repository;

        public MixConsignmentService(IMixConsignmentRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<MixConsignmentInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<MixConsignmentPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<MixConsignmentDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<MixConsignmentLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<MixConsignmentGridViewDto> AddItemInGridAsync(MixConsignmentAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(MixConsignmentSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(MixConsignmentSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<MixConsignmentPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<MixConsignmentStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
            => _repository.GetCurrentStatusAsync(invNo, companyId);

        public Task<List<object>> GetLotsAsync(string? searchBy, string companyId)
            => _repository.GetLotsAsync(searchBy, companyId);

        public Task<string> GetMixLotNoLMAsync(string companyId)
            => _repository.GetMixLotNoLMAsync(companyId);
    }
}
