using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Services.Classes.FinancialTransaction.Payment
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository _repository;

        public JournalEntryService(IJournalEntryRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, actionType, companyId);

        public Task<List<JournalEntryListItemDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(companyId);

        public Task<JournalEntryPageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<JournalEntryDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<(bool Success, string Message)> AddAsync(JournalEntrySaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(JournalEntrySaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request, userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);
    }
}
