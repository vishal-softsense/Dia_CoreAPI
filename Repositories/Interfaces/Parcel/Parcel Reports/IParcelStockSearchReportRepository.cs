using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IParcelStockSearchReportRepository
    {
        Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId);

        Task<List<BasicDataDto>> GetParcelStockSearchDropdownAsync(
            string companyId);

        Task<List<ParcelStockrpt>> GetParcelStockSearchDataAsync(
            string date,
            string where,
            string askingQry,
            string companyId);

        Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId);

        Task<bool> UpdateLocationAsync(ParcelStockLocationUpdateDTO dto, string companyId);

        Task<bool> UpdateBoxAndGroupAsync(ParcelStockBoxGroupUpdateDTO dto, string companyId);   
    }
}