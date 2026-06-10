using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace Dia_CoreAPI.DataAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IMemoryCache _cache;

        public SqlDataAccess(IDbConnectionFactory connectionFactory, IMemoryCache cache)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        private async Task<string> GetClientConnectionString(string companyId)
        {
            string cacheKey = $"ClientDb_{companyId}";
            if (!_cache.TryGetValue(cacheKey, out string? connStringResult))
            {
                using var masterConnection = (SqlConnection)_connectionFactory.CreateConnection("MasterConnection");
                const string masterQuery = "SELECT ConnectionString FROM TabCompanyMaster WHERE CompanyID = @CompanyID";
                
                connStringResult = await masterConnection.ExecuteScalarAsync<string>(masterQuery, new { CompanyID = companyId });
                
                if (string.IsNullOrWhiteSpace(connStringResult))
                    throw new Exception("Invalid Company ID");

                // Store in cache for 1 hour to reduce hits to Master DB
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, connStringResult, cacheOptions);
            }

            var builder = new SqlConnectionStringBuilder(connStringResult)
            {
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }

        public async Task<IEnumerable<T>> SelectDataFromSP<T>(string spName, DynamicParameters parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            
            return await connection.QueryAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<T?> SelectDataFromSPSingle<T>(string spName, DynamicParameters parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            
            return await connection.QueryFirstOrDefaultAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ExecuteSP(string spName, DynamicParameters parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            
            return await connection.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<T?> ExecuteQuerySingle<T>(string query, object parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            
            return await connection.QueryFirstOrDefaultAsync<T>(query, parameters, commandType: CommandType.Text);
        }

        public async Task<int> ExecuteQuery(string query, object parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            
            return await connection.ExecuteAsync(query, parameters, commandType: CommandType.Text);
        }
    }
}
