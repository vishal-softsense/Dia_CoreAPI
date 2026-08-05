using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixPurchaseReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixPurchaseReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixPurchaseReportResultDTO>> GetGeneralPurchaseParcelReportAsync(MixPurchaseReportFilterDTO filter, string companyId);
    }
}