using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment
{
    public interface IPayWithExDiffRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId);
        Task<List<PayWithExDiffListItemDto>> GetListAsync(string companyId);
        Task<PayWithExDiffDetailDto?> GetDetailAsync(int id, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<PayWithExDiffPageDataDto> GetPageDataAsync(string companyId);
        Task<List<PayWithExDiffPendingInvDto>> GetPendingInvoicesAsync(PayWithExDiffPendingInvRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(PayWithExDiffSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(PayWithExDiffSaveRequest request, string userId, string companyId);
        Task<List<PayWithExDiffPendingInvDto>> GetEditDetailsAsync(string gcode, string adjustmentType, string companyId);
    }
}
