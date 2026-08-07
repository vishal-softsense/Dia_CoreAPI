using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixSaleRtnReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixSaleRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixSaleRtnReportResultDTO>> GetGeneralSaleRtnRptAsync(MixSaleRtnReportFilterDTO filter, string companyId);
    }
}