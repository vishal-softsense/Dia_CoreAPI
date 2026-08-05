using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixMemoOutReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId);
        Task<IEnumerable<MixMemoOutReportResultDTO>> GetMixMemoReportAsync(string whereClause, string action, string companyId);
    }
}