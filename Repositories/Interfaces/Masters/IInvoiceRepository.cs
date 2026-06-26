using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface IInvoiceRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        // Term Master
        Task<List<TermMaster>> GetTermMasterListAsync(string where, string companyId);
        Task<bool> SaveTermMasterAsync(TermMaster model, string userId, string companyId, string actionType);

        // Type Master
        Task<List<TypeMaster>> GetTypeMasterListAsync(string where, string companyId);
        Task<bool> SaveTypeMasterAsync(TypeMaster model, string userId, string companyId, string actionType);

        // SubType Master
        Task<List<SubTypeMaster>> GetSubTypeMasterListAsync(string where, string companyId);
        Task<bool> SaveSubTypeMasterAsync(SubTypeMaster model, string userId, string companyId, string actionType);

        // Currency Master
        Task<List<CurrencyMaster>> GetCurrencyMasterListAsync(string where, string companyId);
    }
}
