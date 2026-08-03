using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IParcelStockRegisterReportRepository
    {
        Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);

        Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId);

        Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId);
        Task<IEnumerable<ParcelStockRegisterReportDatatable>> GetParcelStockRegisterReportAsync(
            string Type,
            string StkIn,
            string StkOut,
            string WhereFilter,
            string FromDate,
            string ToDate,
            string companyId);
    }
}