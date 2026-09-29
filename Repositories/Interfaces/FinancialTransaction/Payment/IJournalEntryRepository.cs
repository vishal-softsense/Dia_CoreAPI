using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;

namespace Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment
{
    public interface IJournalEntryRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId);
        Task<List<JournalEntryListItemDto>> GetListAsync(string companyId);
        Task<JournalEntryPageDataDto> GetPageDataAsync(string companyId);
        Task<JournalEntryDetailDto?> GetDetailAsync(int id, string companyId);
        Task<(bool Success, string Message)> AddAsync(JournalEntrySaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(JournalEntrySaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
    }
}
