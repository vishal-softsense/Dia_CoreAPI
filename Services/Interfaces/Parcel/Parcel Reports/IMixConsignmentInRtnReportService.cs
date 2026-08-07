using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixConsignmentInRtnReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixConsignmentInRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixConsignmentInRtnReportResultDTO>> GetMixMemoInRtnReportAsync(MixConsignmentInRtnReportFilterDTO filter, string companyId);
    }
}