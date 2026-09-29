using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Classes.FinancialTransaction.Payment
{
    public class IncomeExpenseService : IIncomeExpenseService
    {
        private readonly IIncomeExpenseRepository _repository;

        public IncomeExpenseService(IIncomeExpenseRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, actionType, companyId);

        public Task<List<IncomeExpenseListItemDto>> GetListAsync(string? transDate, string companyId)
            => _repository.GetListAsync(transDate, companyId);

        public Task<IncomeExpensePageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<IncomeExpenseDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<(bool Success, string Message)> AddAsync(IncomeExpenseSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(IncomeExpenseSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);
    }
}
