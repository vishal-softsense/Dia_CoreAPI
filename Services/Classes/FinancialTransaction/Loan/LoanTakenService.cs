using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Loan;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Loan;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Loan;

namespace Dia_CoreAPI.Services.Classes.FinancialTransaction.Loan
{
    public class LoanTakenService : ILoanTakenService
    {
        private readonly ILoanTakenRepository _repository;

        public LoanTakenService(ILoanTakenRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, actionType, companyId);

        public Task<List<LoanListItemDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(companyId);

        public Task<LoanPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<LoanDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<(bool Success, string Message)> AddAsync(LoanSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(LoanSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);
    }
}
