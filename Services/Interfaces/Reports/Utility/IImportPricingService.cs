using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IImportPricingService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<ImportPricingResult> ImportPricingDataAsync(string userTypeId, string companyId);
        Task<ImportPricingResult> ImportPricingFileAsync(ImportPricingDTO importData, string userTypeId, string companyId);
        Task<ImportPricingResult> SavePricingAsync(SavePricingRequestDto request, string userTypeId, string userId, string companyId);
    }
}
