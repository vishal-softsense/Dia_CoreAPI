using Dia_CoreAPI.DTOs.Masters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface ILocationRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);

        #region Area Master
        Task<List<AreaMaster>> GetAreaMasterListAsync(string where, string companyId);
        Task<bool> SaveAreaMasterAsync(AreaMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteAreaAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region City Master
        Task<List<CityMaster>> GetCityMasterListAsync(string where, string companyId);
        Task<bool> SaveCityMasterAsync(CityMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteCityAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region State Master
        Task<List<StateMaster>> GetStateMasterListAsync(string where, string companyId);
        Task<bool> SaveStateMasterAsync(StateMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteStateAsync(int id, string userTypeId, string userId, string companyId);
        #endregion

        #region Country Master
        Task<List<CountryMaster>> GetCountryMasterListAsync(string where, string companyId);
        Task<bool> SaveCountryMasterAsync(CountryMaster model, string userId, string companyId, string actionType);
        Task<bool> DeleteCountryAsync(int id, string userTypeId, string userId, string companyId);
        #endregion
    }
}
