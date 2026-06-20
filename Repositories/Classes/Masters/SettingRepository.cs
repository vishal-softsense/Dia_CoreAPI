using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Models.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class SettingRepository : ISettingRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SettingRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<CompanySettingMaster>> GetSettingListAsync(string filter, string companyId)
        {
            string cleanFilter = (filter ?? "").Replace("''", "'");

            var parameters = new Dictionary<string, object>
            {
                { "@Type", "Setting" },
                { "@Where", cleanFilter }
            };

            return await _sqlDataAccess.SelectDataFromSP<CompanySettingMaster>("GetALLMasterList", parameters, companyId);
        }

        public async Task<bool> SaveSettingDataAsync(CompanySettingMaster model, string actionType, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ID", model.Id },
                { "@Name", model.Name },
                { "@Value", model.Value },
                { "@Description", model.Description },
                { "@COMMENT", model.COMMENT },
                { "@CompanyID", model.CompanyID },
                { "@ActionType", actionType }
            };

            int result = await _sqlDataAccess.ExecuteSP("SP_CompanySettingMaster", parameters, companyId);
            return result > 0;
        }

        public async Task<bool> CheckUserPermissionAsync(int? userTypeId, string formName, string action, string companyId)
        {
            if (userTypeId == null)
            {
                return false;
            }

            string whereClause = string.Empty;
            string actionLower = action.ToLower();
            if (actionLower == "insert")
                whereClause = " InsertData = 1 ";
            else if (actionLower == "edit")
                whereClause = " EditData = 1 ";
            else if (actionLower == "delete")
                whereClause = " DeleteData = 1 ";
            else if (actionLower == "navigate")
                whereClause = " NavigateData = 1 ";
            else if (actionLower == "select")
                whereClause = " SelectData = 1 ";

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", $"'{userTypeId}'" },
                { "@FormName", $"'{formName}'" },
                { "@Where", whereClause }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<dynamic>("GetUserPermission", parameters, companyId);
            return result.Any();
        }

        public async Task LogPageAccessAsync(string userName, string status, string device, string deviceType, string os, string ip, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Userid", userName },
                { "@Status", status },
                { "@Device", device ?? "Unknown" },
                { "@DeviceType", deviceType ?? "Unknown" },
                { "@OperatingSystem", os ?? "Unknown" },
                { "@Model", "API" },
                { "@IPLocation", ip ?? "Unknown" }
            };

            await _sqlDataAccess.ExecuteSP("sp_InsertPagesLog", parameters, companyId);
        }

        public async Task<IEnumerable<GetInvoiceFormatData>> GetInvoiceFormatListAsync(string filter, string companyId)
        {
            string cleanFilter = (filter ?? "").Replace("''", "'");

            var parameters = new Dictionary<string, object>
            {
                { "@Type", "GetInvoiceFormat" },
                { "@Where", cleanFilter }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<GetInvoiceFormatData>("GetALLMasterList", parameters, companyId);
            return result;
        }

        public async Task<string?> GetInvoicePathAsync(string filter, string companyId)
        {
            string cleanFilter = (filter ?? "").Replace("''", "'");

            var parameters = new Dictionary<string, object>
            {
                { "@Type", "PreviewPrint" },
                { "@Where", cleanFilter }
            };

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLMasterList", parameters, companyId);
            return result;
        }

        public async Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync(string type, string columnName, string abbreviation, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Type", type },
                { "@ColumnName", columnName },
                { "@Abbreviation", abbreviation }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<ColumnAbbrMaster>("SP_UpdateColumnAbbreviation", parameters, companyId);
            return result;
        }
    }
}
