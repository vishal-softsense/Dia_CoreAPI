using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dia_CoreAPI.DataAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> SelectDataFromSP<T>(string spName, Dictionary<string, object> parameters, string companyId);
        Task<T?> SelectDataFromSPSingle<T>(string spName, Dictionary<string, object> parameters, string companyId);
        Task<int> ExecuteSP(string spName, Dictionary<string, object> parameters, string companyId);
        Task<T?> ExecuteQuerySingle<T>(string query, object parameters, string companyId);
        Task<int> ExecuteQuery(string query, object parameters, string companyId);
        Task<string> GetClientConnectionString(string companyId);

        Task<T> GetScalarDataFromSP<T>(string storedProcedure, Dictionary<string, object> parameters, string companyId);
        Task<IEnumerable<T>> SelectRawSql<T>(string sql, string companyId);
        Task<T?> SelectRawSqlSingle<T>(string sql, string companyId);
        Task<IEnumerable<T>> SelectDataDapper<T>(string sql, string companyId);
        Task<IEnumerable<dynamic>> SelectDataDapperDynamic(string sql, string companyId);
    }
}
