using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.StockAccount;

// Resolve CS0104 ambiguity by alias
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixPurchaseReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetPurchaseReportDropDownAsync(string companyId);
        Task<IEnumerable<MixPurchaseReportResultDTO>> GetGeneralPurchaseParcelReportAsync(string whereClause, string companyId);
    }
}