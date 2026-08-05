using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixSaleReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixSaleReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixSaleReportResultDTO>> GetGeneralSaleParcelReportAsync(MixSaleReportFilterDTO filter, string companyId);
    }
}