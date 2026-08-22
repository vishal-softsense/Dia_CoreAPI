using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IMasterPriceComparisonService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<MasterPriceComparisonPageDto> LoadPageDataAsync(string companyId);
        Task<List<MasterPriceComparisonReportDto>> GetReportAsync(MasterPriceComparisonFilterDto filter, string companyId);
    }
}
