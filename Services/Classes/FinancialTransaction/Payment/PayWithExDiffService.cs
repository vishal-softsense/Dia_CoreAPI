using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Classes.FinancialTransaction.Payment
{
    public class PayWithExDiffService : IPayWithExDiffService
    {
        private readonly IPayWithExDiffRepository _repository;

        public PayWithExDiffService(IPayWithExDiffRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, actionType, companyId);

        public Task<List<PayWithExDiffListItemDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(companyId);

        public Task<PayWithExDiffDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<PayWithExDiffPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<List<PayWithExDiffPendingInvDto>> GetPendingInvoicesAsync(PayWithExDiffPendingInvRequest request, string companyId)
            => _repository.GetPendingInvoicesAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(PayWithExDiffSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(PayWithExDiffSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request, userId, companyId);

        public Task<List<PayWithExDiffPendingInvDto>> GetEditDetailsAsync(string gcode, string adjustmentType, string companyId)
            => _repository.GetEditDetailsAsync(gcode, adjustmentType, companyId);
    }
}
