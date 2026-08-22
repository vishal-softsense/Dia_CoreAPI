using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IMissingStockService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MissingStockReportDto>> GetReportAsync(MissingStockFilterDto filter, string companyId);
    }
}
