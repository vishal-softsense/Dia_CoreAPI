using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixPurRtnReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixPurRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixPurRtnReportResultDTO>> GetMixPurchaseReturnReportAsync(MixPurRtnReportFilterDTO filter, string companyId);
    }
}