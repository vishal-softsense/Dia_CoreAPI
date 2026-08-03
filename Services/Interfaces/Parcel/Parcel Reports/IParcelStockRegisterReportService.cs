using Dia_CoreAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IParcelStockRegisterReportService
    {
        Task<bool> HasUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);

        Task<ParcelStockDropdownDto> GetReportDropdownsAsync(string companyId);

        Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId);

        Task<IEnumerable<ParcelStockRegisterReportResponseDto>> GetParcelStockRegisterReportAsync(
            StockRegisterReportFilterDto filter,
            string companyId);
    }
}
