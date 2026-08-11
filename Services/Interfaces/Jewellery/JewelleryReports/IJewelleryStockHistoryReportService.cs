using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryStockHistoryReportService
    {
        Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);

        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);

        Task<IEnumerable<BasicDataDto>> GetJewStockIdListAsync(string searchby, string companyId);

        Task<IEnumerable<JewelleryStockHistoryReportDatatableDto>> GetReportAsync(
            JewelleryStockHistoryReportFilterDto filter,
            string companyId);
    }
}
