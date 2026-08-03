using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports
{
    public interface IParcelStockSearchReportService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<ParcelStockSearchReportFilterDto> LoadReportDataAsync(string companyId);
        Task<List<BasicDataDto>> GetStockSearchDropdownAsync(string companyId);
        Task<List<ParcelStockrpt>> GetParcelStockSearchReportAsync(ParcelStockSearchReportFilterDto filter, string companyId);

        Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId);

        Task<bool> UpdateLocationAsync(ParcelStockLocationUpdateDTO dto, string companyId);

        Task<bool> UpdateBoxAndGroupAsync(ParcelStockBoxGroupUpdateDTO dto, string companyId);
    }
}