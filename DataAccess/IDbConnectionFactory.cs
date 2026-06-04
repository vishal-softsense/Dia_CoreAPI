using System.Data;

namespace Dia_CoreAPI.DataAccess
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        IDbConnection CreateConnection(string connectionName);
        IDbConnection CreateConnectionWithConnectionString(string connectionString);
    }
}
