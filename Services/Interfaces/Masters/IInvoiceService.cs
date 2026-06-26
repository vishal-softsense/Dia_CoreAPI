using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface IInvoiceService
    {
        // Term Master
        Task<List<TermMaster>> GetTermMasterListAsync(string where, string companyId);
        Task<TermMaster> GetTermByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitTermDataAsync(TermMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteTermAsync(int id, string userTypeId, string userId, string companyId);

        // Type Master
        Task<List<TypeMaster>> GetTypeMasterListAsync(string where, string companyId);
        Task<TypeMaster> GetTypeByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitTypeDataAsync(TypeMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteTypeAsync(int id, string userTypeId, string userId, string companyId);

        // SubType Master
        Task<List<SubTypeMaster>> GetSubTypeMasterListAsync(string where, string companyId);
        Task<SubTypeMaster> GetSubTypeByIdAsync(int id, string companyId);
        Task<(bool Success, string Message)> SubmitSubTypeDataAsync(SubTypeMaster model, string userTypeId, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteSubTypeAsync(int id, string userTypeId, string userId, string companyId);

        // Currency Master (Read-only)
        Task<List<CurrencyMaster>> GetCurrencyMasterListAsync(string where, string companyId);
        Task<CurrencyMaster> GetCurrencyByIdAsync(int id, string companyId);
    }
}
