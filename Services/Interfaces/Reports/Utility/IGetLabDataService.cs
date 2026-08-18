using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IGetLabDataService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<GetLabDataResult> GetItemsFromListAsync(GetLabLotRequestDto request, string userTypeId, string companyId);
        Task<GetLabDataResult> ImportCertAsync(GetLabDataDTO importData, string userTypeId, string companyId);
        Task<GetLabDataResult> GetGiaDataAsync(GetCertRequestDto request, string companyId);
        Task<GetLabDataResult> GetIgiDataAsync(GetCertRequestDto request, string companyId);
        Task<GetLabDataResult> PrintExcelAsync(GetLabDataExcelRequestDto request, string companyId);
    }
}
