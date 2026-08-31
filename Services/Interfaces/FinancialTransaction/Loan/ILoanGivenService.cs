using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Loan;

namespace Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Loan
{
    public interface ILoanGivenService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId);
        Task<List<LoanListItemDto>> GetListAsync(string companyId);
        Task<LoanPageDataDto> GetPageDataAsync(string companyId);
        Task<LoanDetailDto?> GetDetailAsync(int id, string companyId);
        Task<(bool Success, string Message)> AddAsync(LoanSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(LoanSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
    }
}
