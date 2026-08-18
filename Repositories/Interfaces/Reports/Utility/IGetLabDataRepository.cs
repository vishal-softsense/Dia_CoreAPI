using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IGetLabDataRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<GetLabLotDto>> GetLotCrtAsync(string where, string companyId);
        Task<string?> GetLabDataIdAsync(string parameter, string parameterValue, string usedFor, string companyId);
        Task<string?> GetSettingValueAsync(string name, string companyId);
    }
}
