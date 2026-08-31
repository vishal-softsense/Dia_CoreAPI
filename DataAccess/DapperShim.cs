using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Dapper
{
    public class DynamicParameters
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public void Add(string name, object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            _parameters[name] = value;
        }

        public Dictionary<string, object> ParameterDictionary => _parameters;
    }

    public static class SqlMapper
    {
        public static IEnumerable<T> Query<T>(
            this IDbConnection c,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var list = new List<T>();
            using (var cmd = CreateCommand(c, sql, param, transaction, commandTimeout, commandType))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    var type = typeof(T);
                    if (type == typeof(string) || type == typeof(int) || type == typeof(long) || type == typeof(bool) || type.IsPrimitive)
                    {
                        while (reader.Read())
                        {
                            if (reader[0] != DBNull.Value)
                                list.Add((T)Convert.ChangeType(reader[0], type));
                            else
                                list.Add(default!);
                        }
                    }
                    else
                    {
                        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
                                    var val = reader[index];
                                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                    prop.SetValue(obj, Convert.ChangeType(val, propType));
                                }
                            }
                            list.Add(obj);
                        }
                    }
                }
            }
            return list;
        }

        public static IEnumerable<object> Query(
            this IDbConnection c,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var list = new List<object>();
            using (var cmd = CreateCommand(c, sql, param, transaction, commandTimeout, commandType))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new object());
                    }
                }
            }
            return list;
        }

        public static int Execute(
            this IDbConnection c,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(c, sql, param, transaction, commandTimeout, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        private static SqlCommand CreateCommand(
            IDbConnection c,
            string sql,
            object? param,
            IDbTransaction? transaction,
            int? commandTimeout,
            CommandType? commandType)
        {
            if (!(c is SqlConnection conn))
                throw new NotSupportedException("Only SqlConnection is supported in this shim.");

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (transaction != null)
                cmd.Transaction = (SqlTransaction)transaction;
            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;
            if (commandType.HasValue)
                cmd.CommandType = commandType.Value;

            if (param != null)
            {
                if (param is DynamicParameters dynamicParams)
                {
                    foreach (var kp in dynamicParams.ParameterDictionary)
                    {
                        var parameterName = kp.Key;
                        if (!parameterName.StartsWith("@"))
                            parameterName = "@" + parameterName;
                        cmd.Parameters.AddWithValue(parameterName, kp.Value ?? DBNull.Value);
                    }
                }
                else
                {
                    var properties = param.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        var parameterName = prop.Name;
                        if (!parameterName.StartsWith("@"))
                            parameterName = "@" + parameterName;
                        cmd.Parameters.AddWithValue(parameterName, prop.GetValue(param) ?? DBNull.Value);
                    }
                }
            }

            return cmd;
        }
    }
}
