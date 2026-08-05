using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixConsignmentInReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixConsignmentInReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixConsignmentInReportResultDTO>> GetMixMemoReportAsync(MixConsignmentInReportFilterDTO filter, string companyId);
        Task<IEnumerable<dynamic>> GetMixConsignmentSummaryReportAsync(MixConsignmentInReportFilterDTO filter, string companyId);
    }
}