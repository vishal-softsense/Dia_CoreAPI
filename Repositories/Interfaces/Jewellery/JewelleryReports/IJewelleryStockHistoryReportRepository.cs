using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryStockHistoryReportRepository
    {
        Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);

        Task<IEnumerable<BasicDataDto>> GetJewStockIdListAsync(string searchby, string companyId);

        Task<IEnumerable<JewelleryStockHistoryReportDatatableDto>> GetJewStockRegisterReportAsync(
            string Type,
            string stkIn,
            string stkOut,
            string WhereFilter,
            string FromDate,
            string ToDate,
            string companyId);

        Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId);
    }
}
