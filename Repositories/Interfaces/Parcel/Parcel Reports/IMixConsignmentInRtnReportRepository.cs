using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixConsignmentInRtnReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId);
        Task<IEnumerable<MixConsignmentInRtnReportResultDTO>> GetMixMemoInRtnReportAsync(string whereClause, string companyId);
    }
}