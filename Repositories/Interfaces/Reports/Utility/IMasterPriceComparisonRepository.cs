using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IMasterPriceComparisonRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<BasicDataDto>> GetFilterDropdownAsync(string companyId);
        Task<List<MasterPriceComparisonReportDto>> GetReportDataAsync(string where, string type, string companyId);
    }
}
