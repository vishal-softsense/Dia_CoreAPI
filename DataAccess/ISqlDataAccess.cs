using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace Dia_CoreAPI.DataAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> SelectDataFromSP<T>(string spName, DynamicParameters parameters, string companyId);
        Task<T?> SelectDataFromSPSingle<T>(string spName, DynamicParameters parameters, string companyId);
        Task<int> ExecuteSP(string spName, DynamicParameters parameters, string companyId);
        Task<T?> ExecuteQuerySingle<T>(string query, object parameters, string companyId);
        Task<int> ExecuteQuery(string query, object parameters, string companyId);
    }
}
