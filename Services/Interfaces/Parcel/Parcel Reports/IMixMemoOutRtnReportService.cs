using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixMemoOutRtnReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixMemoOutRtnReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixMemoOutRtnReportResultDTO>> GetMixMemoRtnReportAsync(MixMemoOutRtnReportFilterDTO filter, string companyId);
    }
}