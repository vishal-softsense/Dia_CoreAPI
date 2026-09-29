using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment
{
    public interface IIncomeExpenseService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId);
        Task<List<IncomeExpenseListItemDto>> GetListAsync(string? transDate, string companyId);
        Task<IncomeExpensePageDataDto> GetPageDataAsync(string companyId);
        Task<IncomeExpenseDetailDto?> GetDetailAsync(int id, string companyId);
        Task<(bool Success, string Message)> AddAsync(IncomeExpenseSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(IncomeExpenseSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
    }
}
