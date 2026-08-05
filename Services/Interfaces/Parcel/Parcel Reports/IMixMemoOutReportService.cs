using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixMemoOutReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MixMemoOutReportDropdownsDTO> GetDropdownDataAsync(string companyId);
        Task<IEnumerable<MixMemoOutReportResultDTO>> GetMixMemoReportAsync(MixMemoOutReportFilterDTO filter, string companyId);
    }
}