using Dia_CoreAPI.DataAccess;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Utilities
{
    /// <summary>
    /// Central helper for Option A carat precision ("0.00" = 2 decimals, "0.000" = 3 decimals).
    /// Always truncates toward zero using decimal arithmetic to prevent rounding up or float drift.
    /// </summary>
    public static class CaratFormatHelper
    {
        public const string Format2 = "0.00";
        public const string Format3 = "0.000";
        public const string SolitaireSettingName = "SolitaireCaratFormat";
        public const string ParcelSettingName = "ParcelCaratFormat";

        public static string NormalizeCaratFormat(string? caratFormat)
        {
            var trimmed = caratFormat?.Trim();
            if (string.Equals(trimmed, Format3, StringComparison.Ordinal) ||
                string.Equals(trimmed, "3", StringComparison.Ordinal) ||
                string.Equals(trimmed, "3.00", StringComparison.Ordinal) ||
                string.Equals(trimmed, "3.000", StringComparison.Ordinal))
            {
                return Format3;
            }

            return Format2;
        }

        public static int GetCaratDecimals(string? caratFormat) =>
            NormalizeCaratFormat(caratFormat) == Format3 ? 3 : 2;

        private static int NormalizeDecimals(int decimals) => decimals >= 3 ? 3 : 2;

        public static decimal ToCleanDecimal(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0m;

            return Convert.ToDecimal(value);
        }

        public static decimal ToCleanDecimal(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return 0m;

            var str = value.ToString("G15", CultureInfo.InvariantCulture);
            if (decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return result;

            return 0m;
        }

        public static decimal TruncateDecimal(decimal value, int decimals)
        {
            decimal factor = NormalizeDecimals(decimals) == 3 ? 1000m : 100m;
            return decimal.Truncate(value * factor) / factor;
        }

        public static decimal TruncateDecimal(decimal value, string? caratFormat) =>
            TruncateDecimal(value, GetCaratDecimals(caratFormat));

        public static double TruncateDouble(double value, int decimals) =>
            (double)TruncateDecimal(ToCleanDecimal(value), decimals);

        public static double TruncateDouble(double value, string? caratFormat) =>
            TruncateDouble(value, GetCaratDecimals(caratFormat));

        public static float TruncateFloat(float value, int decimals) =>
            (float)TruncateDecimal(ToCleanDecimal(value), decimals);

        public static float TruncateFloat(float value, string? caratFormat) =>
            TruncateFloat(value, GetCaratDecimals(caratFormat));

        public static decimal ParseAndTruncateDecimal(string? raw, string? caratFormat)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return 0m;

            var cleaned = raw.Trim().Replace(",", string.Empty);
            if (decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return TruncateDecimal(parsed, caratFormat);

            return 0m;
        }

        public static double ParseAndTruncateDouble(string? raw, string? caratFormat) =>
            (double)ParseAndTruncateDecimal(raw, caratFormat);

        public static float ParseAndTruncateFloat(string? raw, string? caratFormat) =>
            (float)ParseAndTruncateDecimal(raw, caratFormat);

        public static string FormatCaratString(decimal value, string? caratFormat)
        {
            var normalized = NormalizeCaratFormat(caratFormat);
            var truncated = TruncateDecimal(value, normalized);
            return truncated.ToString(normalized, CultureInfo.InvariantCulture);
        }

        public static string FormatCaratString(double value, string? caratFormat) =>
            FormatCaratString(ToCleanDecimal(value), caratFormat);

        public static string FormatCaratString(float value, string? caratFormat) =>
            FormatCaratString(ToCleanDecimal(value), caratFormat);

        public static Task<string> GetCaratFormatAsync(ISqlDataAccess sqlDataAccess, string companyId) =>
            GetParcelCaratFormatAsync(sqlDataAccess, companyId);

        public static Task<string> GetCaratFormatAsync(SqlConnection openConnection, SqlTransaction? transaction = null) =>
            GetParcelCaratFormatAsync(openConnection, transaction);

        public static Task<string> GetSolitaireCaratFormatAsync(ISqlDataAccess sqlDataAccess, string companyId) =>
            GetCaratFormatByPrimarySettingAsync(sqlDataAccess, companyId, SolitaireSettingName);

        public static Task<string> GetSolitaireCaratFormatAsync(SqlConnection openConnection, SqlTransaction? transaction = null) =>
            GetCaratFormatByPrimarySettingAsync(openConnection, transaction, SolitaireSettingName);

        public static Task<string> GetParcelCaratFormatAsync(ISqlDataAccess sqlDataAccess, string companyId) =>
            GetCaratFormatByPrimarySettingAsync(sqlDataAccess, companyId, ParcelSettingName);

        public static Task<string> GetParcelCaratFormatAsync(SqlConnection openConnection, SqlTransaction? transaction = null) =>
            GetCaratFormatByPrimarySettingAsync(openConnection, transaction, ParcelSettingName);

        private static string BuildPrioritySql(string primarySettingName) =>
            primarySettingName == SolitaireSettingName
                ? @"SELECT TOP 1 Value
                    FROM TabSettingMaster
                    WHERE Name IN ('SolitaireCaratFormat', 'CaratFormatNew', 'CaratFormat')
                      AND NULLIF(LTRIM(RTRIM(CAST(Value AS NVARCHAR(50)))), '') IS NOT NULL
                    ORDER BY CASE
                        WHEN Name = 'SolitaireCaratFormat' THEN 0
                        WHEN Name = 'CaratFormatNew' THEN 1
                        ELSE 2
                    END"
                : @"SELECT TOP 1 Value
                    FROM TabSettingMaster
                    WHERE Name IN ('ParcelCaratFormat', 'CaratFormatNew', 'CaratFormat')
                      AND NULLIF(LTRIM(RTRIM(CAST(Value AS NVARCHAR(50)))), '') IS NOT NULL
                    ORDER BY CASE
                        WHEN Name = 'ParcelCaratFormat' THEN 0
                        WHEN Name = 'CaratFormatNew' THEN 1
                        ELSE 2
                    END";

        private static async Task<string> GetCaratFormatByPrimarySettingAsync(ISqlDataAccess sqlDataAccess, string companyId, string primarySettingName)
        {
            var value = await sqlDataAccess.ExecuteQuerySingle<string>(
                BuildPrioritySql(primarySettingName),
                new { },
                companyId);

            return NormalizeCaratFormat(value);
        }

        private static async Task<string> GetCaratFormatByPrimarySettingAsync(SqlConnection openConnection, SqlTransaction? transaction, string primarySettingName)
        {
            if (openConnection.State != ConnectionState.Open)
                await openConnection.OpenAsync();

            using var cmd = new SqlCommand(BuildPrioritySql(primarySettingName), openConnection, transaction);
            var result = await cmd.ExecuteScalarAsync();
            return NormalizeCaratFormat(result?.ToString());
        }

        public static async Task<(string DefaultFormat, string SolitaireFormat, string ParcelFormat)> GetLoginCaratFormatsAsync(
            SqlConnection openConnection,
            SqlTransaction? transaction = null)
        {
            if (openConnection.State != ConnectionState.Open)
                await openConnection.OpenAsync();

            const string sql = @"
                SELECT Name, Value
                FROM TabSettingMaster
                WHERE Name IN ('SolitaireCaratFormat', 'ParcelCaratFormat', 'CaratFormatNew', 'CaratFormat')
                  AND NULLIF(LTRIM(RTRIM(CAST(Value AS NVARCHAR(50)))), '') IS NOT NULL";

            string? solitaireRaw = null;
            string? parcelRaw = null;
            string? newRaw = null;
            string? legacyRaw = null;

            using var cmd = new SqlCommand(sql, openConnection, transaction);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader["Name"]?.ToString()?.Trim();
                var value = reader["Value"]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                    continue;

                if (string.Equals(name, SolitaireSettingName, StringComparison.OrdinalIgnoreCase))
                    solitaireRaw = value;
                else if (string.Equals(name, ParcelSettingName, StringComparison.OrdinalIgnoreCase))
                    parcelRaw = value;
                else if (string.Equals(name, "CaratFormatNew", StringComparison.OrdinalIgnoreCase))
                    newRaw = value;
                else if (string.Equals(name, "CaratFormat", StringComparison.OrdinalIgnoreCase))
                    legacyRaw = value;
            }

            var fallbackRaw = newRaw ?? legacyRaw ?? parcelRaw ?? solitaireRaw;
            return (
                NormalizeCaratFormat(fallbackRaw),
                NormalizeCaratFormat(solitaireRaw ?? fallbackRaw),
                NormalizeCaratFormat(parcelRaw ?? fallbackRaw)
            );
        }

        private sealed record SettingColumnMeta(string Name, bool IsIdentity, bool IsComputed, string DataType, bool IsNullable);

        /// <summary>
        /// Ensures SolitaireCaratFormat and ParcelCaratFormat rows exist and are active in
        /// TabSettingMaster under 'DEFAULT STOCK SETTING' for the current client company DB.
        /// Clones CaratFormatNew/CaratFormat column layout dynamically to match any DB schema.
        /// </summary>
        public static async Task EnsureSeparateCaratSettingsAsync(ISqlDataAccess sqlDataAccess, string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return;

            try
            {
                var connStr = await sqlDataAccess.GetClientConnectionString(companyId);
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                var columns = await LoadTabSettingColumnsAsync(conn);
                if (columns.Count == 0)
                    return;

                var hasGroupCol = columns.Any(c => string.Equals(c.Name, "GroupId", StringComparison.OrdinalIgnoreCase));
                var hasActiveCol = columns.Any(c => string.Equals(c.Name, "Active", StringComparison.OrdinalIgnoreCase));

                var existingMap = new Dictionary<string, (string? GroupId, bool Active)>(StringComparer.OrdinalIgnoreCase);
                var checkCols = "Name"
                    + (hasGroupCol ? ", CAST(GroupId AS NVARCHAR(200)) AS GroupId" : ", CAST(NULL AS NVARCHAR(200)) AS GroupId")
                    + (hasActiveCol ? ", CAST(Active AS NVARCHAR(20)) AS Active" : ", CAST('1' AS NVARCHAR(20)) AS Active");

                await using (var checkCmd = new SqlCommand(
                    $"SELECT {checkCols} FROM TabSettingMaster WHERE Name IN ('SolitaireCaratFormat', 'ParcelCaratFormat')",
                    conn))
                await using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var rowName = reader["Name"]?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(rowName))
                            continue;

                        var rowGroup = reader["GroupId"] == DBNull.Value ? null : reader["GroupId"]?.ToString()?.Trim();
                        var activeStr = reader["Active"]?.ToString()?.Trim() ?? "1";
                        var isActive = activeStr == "1" || string.Equals(activeStr, "true", StringComparison.OrdinalIgnoreCase);
                        existingMap[rowName] = (rowGroup, isActive);
                    }
                }

                var targets = new[]
                {
                    (Name: SolitaireSettingName, Description: "Solitaire Carat Decimal Format (0.00 or 0.000)", PriorityOffset: 1),
                    (Name: ParcelSettingName, Description: "Parcel Carat Decimal Format (0.00 or 0.000)", PriorityOffset: 2)
                };

                // Fix existing rows if inserted with inactive flag or wrong/null GroupId
                foreach (var target in targets)
                {
                    if (!existingMap.TryGetValue(target.Name, out var state))
                        continue;

                    var needsGroupFix = hasGroupCol && !string.Equals(state.GroupId, "DEFAULT STOCK SETTING", StringComparison.OrdinalIgnoreCase);
                    var needsActiveFix = hasActiveCol && !state.Active;
                    if (!needsGroupFix && !needsActiveFix)
                        continue;

                    var sets = new List<string>();
                    if (needsGroupFix) sets.Add("[GroupId] = 'DEFAULT STOCK SETTING'");
                    if (needsActiveFix) sets.Add("[Active] = 1");

                    await using var fixCmd = new SqlCommand(
                        $"UPDATE TabSettingMaster SET {string.Join(", ", sets)} WHERE Name = @Name",
                        conn);
                    fixCmd.Parameters.AddWithValue("@Name", target.Name);
                    await fixCmd.ExecuteNonQueryAsync();
                }

                var missingTargets = targets.Where(t => !existingMap.ContainsKey(t.Name)).ToList();
                if (missingTargets.Count == 0)
                    return;

                var template = await LoadTemplateSettingRowAsync(conn);
                var defaultVal = template != null && template.TryGetValue("Value", out var rawTemplateVal)
                    ? NormalizeCaratFormat(rawTemplateVal?.ToString())
                    : Format2;

                var idColumn = columns.FirstOrDefault(c => string.Equals(c.Name, "Id", StringComparison.OrdinalIgnoreCase));
                int nextManualId = 0;
                if (idColumn != null && !idColumn.IsIdentity)
                {
                    await using var maxCmd = new SqlCommand($"SELECT ISNULL(MAX([{idColumn.Name}]), 0) FROM TabSettingMaster", conn);
                    var maxRes = await maxCmd.ExecuteScalarAsync();
                    nextManualId = Convert.ToInt32(maxRes ?? 0, CultureInfo.InvariantCulture);
                }

                var insertableCols = columns.Where(c => !c.IsIdentity && !c.IsComputed).ToList();

                foreach (var target in missingTargets)
                {
                    var colNames = new List<string>();
                    var paramNames = new List<string>();
                    await using var insertCmd = new SqlCommand { Connection = conn };

                    int pIdx = 0;
                    foreach (var col in insertableCols)
                    {
                        object? val = null;
                        bool hasVal = template != null && template.TryGetValue(col.Name, out val) && val != null && val != DBNull.Value;

                        if (string.Equals(col.Name, "Id", StringComparison.OrdinalIgnoreCase))
                        {
                            nextManualId++;
                            val = nextManualId;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "Name", StringComparison.OrdinalIgnoreCase))
                        {
                            val = target.Name;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "Value", StringComparison.OrdinalIgnoreCase))
                        {
                            val = defaultVal;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "Description", StringComparison.OrdinalIgnoreCase))
                        {
                            val = target.Description;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "COMMENT", StringComparison.OrdinalIgnoreCase))
                        {
                            val = string.Empty;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "GroupId", StringComparison.OrdinalIgnoreCase))
                        {
                            val = "DEFAULT STOCK SETTING";
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "Active", StringComparison.OrdinalIgnoreCase))
                        {
                            val = col.DataType.Contains("bit", StringComparison.OrdinalIgnoreCase) ? true : 1;
                            hasVal = true;
                        }
                        else if (string.Equals(col.Name, "Priority", StringComparison.OrdinalIgnoreCase))
                        {
                            if (hasVal && int.TryParse(val?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int basePri))
                            {
                                val = basePri + target.PriorityOffset;
                            }
                            else
                            {
                                val = target.PriorityOffset + 1;
                            }
                            hasVal = true;
                        }

                        if (!hasVal)
                        {
                            if (!col.IsNullable)
                            {
                                val = GetDefaultForSqlType(col.DataType);
                                hasVal = true;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        var pName = $"@p{pIdx++}";
                        colNames.Add($"[{col.Name}]");
                        paramNames.Add(pName);
                        insertCmd.Parameters.AddWithValue(pName, val ?? DBNull.Value);
                    }

                    if (colNames.Count == 0)
                        continue;

                    insertCmd.CommandText = $"INSERT INTO TabSettingMaster ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                // Never block Setting Master listing if schema/permissions disallow auto-seed
            }
        }

        private static async Task<List<SettingColumnMeta>> LoadTabSettingColumnsAsync(SqlConnection conn)
        {
            const string sql = @"
                SELECT
                    c.name AS ColumnName,
                    CAST(ISNULL(COLUMNPROPERTY(c.object_id, c.name, 'IsIdentity'), 0) AS INT) AS IsIdentity,
                    CAST(c.is_computed AS INT) AS IsComputed,
                    t.name AS DataType,
                    CAST(c.is_nullable AS INT) AS IsNullable
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                WHERE c.object_id = OBJECT_ID('TabSettingMaster')
                ORDER BY c.column_id";

            var result = new List<SettingColumnMeta>();
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new SettingColumnMeta(
                    reader["ColumnName"]?.ToString() ?? string.Empty,
                    Convert.ToInt32(reader["IsIdentity"], CultureInfo.InvariantCulture) == 1,
                    Convert.ToInt32(reader["IsComputed"], CultureInfo.InvariantCulture) == 1,
                    reader["DataType"]?.ToString() ?? "nvarchar",
                    Convert.ToInt32(reader["IsNullable"], CultureInfo.InvariantCulture) == 1));
            }
            return result;
        }

        private static async Task<Dictionary<string, object?>?> LoadTemplateSettingRowAsync(SqlConnection conn)
        {
            const string sql = @"
                SELECT TOP 1 *
                FROM TabSettingMaster
                WHERE Name IN ('CaratFormatNew', 'CaratFormat')
                ORDER BY CASE WHEN Name = 'CaratFormatNew' THEN 0 ELSE 1 END";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                row[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            return row;
        }

        private static object GetDefaultForSqlType(string dataType)
        {
            var dt = (dataType ?? string.Empty).ToLowerInvariant();
            if (dt.Contains("int") || dt.Contains("decimal") || dt.Contains("numeric") || dt.Contains("float") || dt.Contains("real") || dt.Contains("money"))
                return 0;
            if (dt == "bit")
                return false;
            if (dt.Contains("date") || dt.Contains("time"))
                return DateTime.Now;
            if (dt == "uniqueidentifier")
                return Guid.NewGuid();
            return string.Empty;
        }
    }
}
