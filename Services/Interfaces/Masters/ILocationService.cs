using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface ILocationService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        #region Area Master
        Task<List<AreaMaster>> GetAreaMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveAreaMasterAsync(AreaMaster model, string userId, string userTypeId, string companyId, string actionType);
        Task<(bool Success, string Message)> DeleteAreaAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region City Master
        Task<List<CityMaster>> GetCityMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveCityMasterAsync(CityMaster model, string userId, string userTypeId, string companyId, string actionType);
        Task<(bool Success, string Message)> DeleteCityAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region State Master
        Task<List<StateMaster>> GetStateMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveStateMasterAsync(StateMaster model, string userId, string userTypeId, string companyId, string actionType);
        Task<(bool Success, string Message)> DeleteStateAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Country Master
        Task<List<CountryMaster>> GetCountryMasterListAsync(string where, string companyId);
        Task<(bool Success, string Message)> SaveCountryMasterAsync(CountryMaster model, string userId, string userTypeId, string companyId, string actionType);
        Task<(bool Success, string Message)> DeleteCountryAsync(int id, string userTypeId, string userId, string companyId);
        #endregion
    }
}
