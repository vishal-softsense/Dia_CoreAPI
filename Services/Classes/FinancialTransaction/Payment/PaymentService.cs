using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Classes.FinancialTransaction.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, actionType, companyId);

        public Task<List<PaymentListItemDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(companyId);

        public Task<PaymentDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<PaymentJvListItemDto?> GetJvDetailAsync(int id, string companyId)
            => _repository.GetJvDetailAsync(id, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<PaymentPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<List<PaymentJvListItemDto>> GetJvEntriesAsync(string? transDate, string companyId)
            => _repository.GetJvEntriesAsync(transDate, companyId);

        public Task<List<PaymentPendingInvDto>> GetPendingInvoicesAsync(PaymentPendingInvRequest request, string companyId)
            => _repository.GetPendingInvoicesAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(PaymentSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(PaymentSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditJvAsync(PaymentSaveRequest request, string userId, string companyId)
            => _repository.EditJvAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> DeleteJvAsync(int id, string userId, string companyId)
            => _repository.DeleteJvAsync(id, userId, companyId);

        public Task<List<PaymentPendingInvDto>> GetEditDetailsAsync(string gcode, string adjustmentType, string companyId)
            => _repository.GetEditDetailsAsync(gcode, adjustmentType, companyId);
    }
}
