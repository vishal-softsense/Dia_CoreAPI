using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IImportPricingRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<string> GetMaxItemCodeAsync(string whereClause, string companyId);
        Task<(bool Success, string Data)> SavePricingAsync(
            string pricingType,
            IList<ImportPricingRowDto> items,
            int userId,
            string companyId);
    }
}
