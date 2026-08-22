using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IStockTellyRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<BasicDataDto>> GetStockSearchDropdownAsync(string companyId);
        Task<List<ParcelStockrpt>> GetParcelStockSearchDataAsync(string date, string where, string askingQry, string companyId);
        Task<string?> GetLotNoAsync(string whereClause, string companyId);
        Task<(bool Success, string Message)> SaveStockTellyAsync(
            StockTellySaveRequestDto request,
            int userId,
            string userName,
            int companyId,
            string companyIdStr);
    }
}
