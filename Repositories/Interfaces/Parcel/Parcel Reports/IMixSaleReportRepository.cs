using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports
{
    public interface IMixSaleReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetSaleReportDropDownAsync(string companyId);
        Task<IEnumerable<MixSaleReportResultDTO>> GetGeneralSaleParcelReportAsync(string whereClause, string companyId);
    }
}