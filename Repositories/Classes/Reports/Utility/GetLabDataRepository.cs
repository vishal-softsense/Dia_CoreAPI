using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Classes.Reports.Utility
{
    public class GetLabDataRepository : IGetLabDataRepository
    {
        private readonly ISqlDataAccess _db;

        public GetLabDataRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
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

        /// <summary>Matches MVC ReportProcessor.GetLotCrt → GetAllCommonData Type=LotCrt</summary>
        public async Task<List<GetLabLotDto>> GetLotCrtAsync(string where, string companyId)
        {
            var sql = @" EXEC [GetAllCommonData] @Type='LotCrt',@Where = '" + where + "' ";
            var rows = await _db.SelectRawSql<GetLabLotDto>(sql, companyId);
            return rows?.ToList() ?? new List<GetLabLotDto>();
        }

        /// <summary>Matches MVC ReportProcessor.GetLabDataID → GetParameterID</summary>
        public async Task<string?> GetLabDataIdAsync(string parameter, string parameterValue, string usedFor, string companyId)
        {
            var sql = @" EXEC [GetParameterID]  @Parameter = '" + EscapeSql(parameter)
                + "',@ParameterValue = '" + EscapeSql(parameterValue)
                + "',@UsedFor='" + EscapeSql(usedFor) + "' ";

            var rows = await _db.SelectRawSql<LabParameterDto>(sql, companyId);
            return rows?.FirstOrDefault()?.ParameterID;
        }

        public async Task<string?> GetSettingValueAsync(string name, string companyId)
        {
            const string sql = "SELECT Name, Value FROM TabSettingMaster";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);

            foreach (var row in rows)
            {
                var settingName = GetString(row, "Name", "name");
                if (string.Equals(settingName, name, StringComparison.OrdinalIgnoreCase))
                    return GetString(row, "Value", "value");
            }

            return null;
        }

        private static string EscapeSql(string? value)
            => (value ?? string.Empty).Replace("'", "''");

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
