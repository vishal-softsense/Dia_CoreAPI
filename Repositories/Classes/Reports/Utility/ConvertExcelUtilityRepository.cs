using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class ConvertExcelUtilityRepository : IConvertExcelUtilityRepository
    {
        private readonly ISqlDataAccess _db;

        public ConvertExcelUtilityRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId = "")
        {
            string where = actionType switch
            {
                "insert" => " InsertData = 1 ",
                "edit" => " EditData = 1 ",
                "delete" => " DeleteData = 1 ",
                "navigate" => " NavigateData = 1 ",
                "select" => " SelectData = 1 ",
                _ => string.Empty
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _db.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }

        public async Task<Dictionary<string, string>> GetColumnAbbreviationMapAsync(string companyId = "")
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sql = "EXEC [TabColumnAbbreviationDetailsUsingWhereClause] @Where=''";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);

            foreach (var row in rows)
            {
                string columnName = GetString(row, "ColumnName");
                string abbreviation = GetString(row, "Abbreviation");

                if (!string.IsNullOrEmpty(abbreviation))
                {
                    foreach (string abbr in abbreviation.Split('|'))
                    {
                        string trimmed = abbr.Trim();
                        if (!string.IsNullOrEmpty(trimmed) && !map.ContainsKey(trimmed))
                        {
                            map[trimmed] = columnName;
                        }
                    }
                }
            }

            return map;
        }

        private static string GetString(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match != null && dict[match] != null && dict[match] != DBNull.Value)
                        return dict[match]?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
