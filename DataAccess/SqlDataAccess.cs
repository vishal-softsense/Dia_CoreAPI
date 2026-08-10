using System.Dynamic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

        public async Task<string> GetClientConnectionString(string companyId)
        {
            string cacheKey = $"ClientDb_{companyId}";
            if (!_cache.TryGetValue(cacheKey, out string? connStringResult))
            {
                using var masterConnection = (SqlConnection)_connectionFactory.CreateConnection("MasterConnection");
                await masterConnection.OpenAsync();
                const string masterQuery = "SELECT ConnectionString FROM TabCompanyMaster WHERE CompanyID = @CompanyID";
                using var cmd = new SqlCommand(masterQuery, masterConnection);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                
                var result = await cmd.ExecuteScalarAsync();
                connStringResult = result?.ToString();
                
                if (string.IsNullOrWhiteSpace(connStringResult))
                    throw new Exception("Invalid Company ID");

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

        private void AddParametersToCommand(SqlCommand cmd, object parameters)
        {
            if (parameters == null) return;

            if (parameters is Dictionary<string, object> dictParams)
            {
                foreach (var param in dictParams)
                {
                    if (param.Value is SqlParameter sqlParam)
                    {
                        cmd.Parameters.Add(sqlParam);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
            }
            else
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var val = prop.GetValue(parameters) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue($"@{prop.Name}", val);
                }
            }
        }

        private List<T> MapReaderToList<T>(SqlDataReader reader)
        {
            var list = new List<T>();
            var type = typeof(T);
            
            if (type == typeof(string) || type == typeof(int) || type == typeof(bool) || type.IsPrimitive)
            {
                while (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                        list.Add((T)ConvertValue(reader[0], type));
                }
                return list;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToArray();
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                columnNames.Add(reader.GetName(i).ToLowerInvariant());

            while (reader.Read())
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var prop in properties)
                {
                    int index = columnNames.IndexOf(prop.Name.ToLowerInvariant());
                    if (index != -1 && reader[index] != DBNull.Value)
                    {
                        var converted = ConvertValue(reader[index], prop.PropertyType);
                        if (converted != null || !prop.PropertyType.IsValueType || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                            prop.SetValue(obj, converted);
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// Safe ADO.NET value conversion for SQL decimal/float/real/bigint → DTO float/double/int/string.
        /// </summary>
        private static object ConvertValue(object val, Type targetType)
        {
            if (val == null || val == DBNull.Value)
                return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsInstanceOfType(val))
                return val;

            if (underlying == typeof(string))
                return Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture);

            if (underlying == typeof(Guid))
                return val is Guid g ? g : Guid.Parse(val.ToString());

            if (underlying == typeof(DateTime))
                return Convert.ToDateTime(val, System.Globalization.CultureInfo.InvariantCulture);

            if (underlying == typeof(bool))
                return Convert.ToBoolean(val, System.Globalization.CultureInfo.InvariantCulture);

            // Numeric: go through decimal first so SQL decimal/float/real/money all map cleanly
            if (underlying == typeof(byte) || underlying == typeof(short) || underlying == typeof(int) ||
                underlying == typeof(long) || underlying == typeof(float) || underlying == typeof(double) ||
                underlying == typeof(decimal))
            {
                var dec = Convert.ToDecimal(val, System.Globalization.CultureInfo.InvariantCulture);
                return Convert.ChangeType(dec, underlying, System.Globalization.CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(val, underlying, System.Globalization.CultureInfo.InvariantCulture);
        }

        public async Task<IEnumerable<T>> SelectDataFromSP<T>(string spName, Dictionary<string, object> parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 180;
            AddParametersToCommand(cmd, parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            return MapReaderToList<T>(reader);
        }

        public async Task<T?> SelectDataFromSPSingle<T>(string spName, Dictionary<string, object> parameters, string companyId)
        {
            var list = await SelectDataFromSP<T>(spName, parameters, companyId);
            return list.FirstOrDefault();
        }

        public async Task<int> ExecuteSP(string spName, Dictionary<string, object> parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            AddParametersToCommand(cmd, parameters);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<T?> ExecuteQuerySingle<T>(string query, object parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(query, connection);
            cmd.CommandType = CommandType.Text;
            AddParametersToCommand(cmd, parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            var list = MapReaderToList<T>(reader);
            return list.FirstOrDefault();
        }

        public async Task<int> ExecuteQuery(string query, object parameters, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(query, connection);
            cmd.CommandType = CommandType.Text;
            AddParametersToCommand(cmd, parameters);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<T> GetScalarDataFromSP<T>(
            string storedProcedure,
            Dictionary<string, object> parameters,
            string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(storedProcedure, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 180;
            AddParametersToCommand(cmd, parameters);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return default(T)!;

            return (T)ConvertValue(result, typeof(T));
        }

        // Executes raw SQL text (EXEC SP with inline string-quoted params, matching MVC pattern)
        // Uses ADO.NET (same as rest of this project) — not Dapper
        public async Task<IEnumerable<T>> SelectRawSql<T>(string sql, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 180; // matches MVC SelectData commandTimeout for analysis reports
            using var reader = await cmd.ExecuteReaderAsync();
            return MapReaderToList<T>(reader);
        }

        public async Task<T?> SelectRawSqlSingle<T>(string sql, string companyId)
        {
            var list = await SelectRawSql<T>(sql, companyId);
            return list.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> SelectDataAdo<T>(string sql, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 180;
            using var reader = await cmd.ExecuteReaderAsync();
            return MapReaderToList<T>(reader);
        }

        public async Task<IEnumerable<dynamic>> SelectDataAdoDynamic(string sql, string companyId)
        {
            var connString = await GetClientConnectionString(companyId);
            using var connection = (SqlConnection)_connectionFactory.CreateConnectionWithConnectionString(connString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 180;
            using var reader = await cmd.ExecuteReaderAsync();
            
            var list = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                var row = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // Match MVC DataTable.Load: duplicate SP column names become Date, Date1, Account, Account1, etc.
                    // Without this, credit-side dual-ledger fields overwrite debit-side fields.
                    string baseName = reader.GetName(i) ?? $"Column{i}";
                    string uniqueName = baseName;
                    int suffix = 1;
                    while (row.ContainsKey(uniqueName))
                    {
                        uniqueName = baseName + suffix;
                        suffix++;
                    }
                    row[uniqueName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                list.Add(row);
            }
            return list;
        }
    }
}
