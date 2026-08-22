using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IConvertExcelUtilityRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId = "");
        Task<Dictionary<string, string>> GetColumnAbbreviationMapAsync(string companyId = "");
    }
}
