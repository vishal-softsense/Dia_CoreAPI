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
    public class DownloadMRPRepository : IDownloadMRPRepository
    {
        private readonly ISqlDataAccess _db;

        public DownloadMRPRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

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

        public async Task<RapaportCredentialsDTO> GetRapaportCredentialsAsync(string companyId)
        {
            const string sql = @"SELECT Name, Value FROM TabSettingMaster WHERE Name IN ('RapUsername', 'RapPassword')";

            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            var credentials = new RapaportCredentialsDTO();

            foreach (var row in rows)
            {
                var name = GetString(row, "Name", "name");
                var value = GetString(row, "Value", "value");
                if (string.Equals(name, "RapUsername", StringComparison.OrdinalIgnoreCase))
                    credentials.RapUsername = value;
                else if (string.Equals(name, "RapPassword", StringComparison.OrdinalIgnoreCase))
                    credentials.RapPassword = value;
            }

            return credentials;
        }

        /// <summary>Matches MVC SqlDataAccess.DeleteTabPriceIndexR → SP_DeletePriceIndexR</summary>
        public async Task<bool> DeleteTabPriceIndexRAsync(string shape, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Cut", shape }
            };

            await _db.ExecuteSP("SP_DeletePriceIndexR", parameters, companyId);
            return true;
        }

        /// <summary>Matches MVC SqlDataAccess.InsertTabPriceIndexR → SP_InsertPriceIndexR</summary>
        public async Task<bool> InsertTabPriceIndexRAsync(
            string shape,
            RapaportPriceRowDTO row,
            DateTime rapaportDate,
            string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Cut", shape },
                { "@Color", row.Color },
                { "@Clarity", row.Clarity },
                { "@CaratsFrom", row.CaratsFrom },
                { "@CaratsTo", row.CaratsTo },
                { "@Rate", row.Rate },
                { "@Rapaportdate", rapaportDate }
            };

            await _db.ExecuteSP("SP_InsertPriceIndexR", parameters, companyId);
            return true;
        }

        /// <summary>Matches MVC SqlDataAccess.UpdatePriceToCarat → SP_UpdateToCaratPriceIndexR</summary>
        public async Task<bool> UpdatePriceToCaratAsync(string companyId)
        {
            await _db.ExecuteSP("SP_UpdateToCaratPriceIndexR", new Dictionary<string, object>(), companyId);
            return true;
        }

        /// <summary>Matches MVC SqlDataAccess.UpdateMRP → SP_UpdateMRP</summary>
        public async Task<bool> UpdateMRPAsync(string companyId)
        {
            await _db.ExecuteSP("SP_UpdateMRP", new Dictionary<string, object>(), companyId);
            return true;
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
