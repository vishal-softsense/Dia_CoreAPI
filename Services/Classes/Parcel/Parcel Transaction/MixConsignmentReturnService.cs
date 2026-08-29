using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class MixConsignmentReturnService : IMixConsignmentReturnService
    {
        private readonly IMixConsignmentReturnRepository _repository;

        public MixConsignmentReturnService(IMixConsignmentReturnRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<MixConsignmentReturnPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<List<MixConsignmentReturnRowDto>> GetListAsync(MixConsignmentReturnFilterRequest filter, string companyId)
            => _repository.GetListAsync(filter ?? new(), companyId);

        public Task<(bool Success, string Message)> ReturnAsync(MixConsignmentReturnSaveRequest request, string userId, string companyId)
            => _repository.ReturnAsync(request ?? new(), userId, companyId);
    }
}
