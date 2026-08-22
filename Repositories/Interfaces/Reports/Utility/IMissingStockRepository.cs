using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IMissingStockRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MissingStockReportDto>> GetMissingStockDataAsync(string stockType, string where, string companyId);
    }
}
