using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment
{
    public interface IPaymentService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId);
        Task<List<PaymentListItemDto>> GetListAsync(string companyId);
        Task<PaymentDetailDto?> GetDetailAsync(int id, string companyId);
        Task<PaymentJvListItemDto?> GetJvDetailAsync(int id, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<PaymentPageDataDto> GetPageDataAsync(string companyId);
        Task<List<PaymentJvListItemDto>> GetJvEntriesAsync(string? transDate, string companyId);
        Task<List<PaymentPendingInvDto>> GetPendingInvoicesAsync(PaymentPendingInvRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(PaymentSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(PaymentSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditJvAsync(PaymentSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteJvAsync(int id, string userId, string companyId);
        Task<List<PaymentPendingInvDto>> GetEditDetailsAsync(string gcode, string adjustmentType, string companyId);
    }
}
