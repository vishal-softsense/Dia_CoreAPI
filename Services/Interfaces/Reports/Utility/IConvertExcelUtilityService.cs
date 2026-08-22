using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IConvertExcelUtilityService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId = "");
        Task<ConvertExcelResultDto> ProcessUploadBothTablesAsync(IFormFile file, string companyId = "");
    }
}
