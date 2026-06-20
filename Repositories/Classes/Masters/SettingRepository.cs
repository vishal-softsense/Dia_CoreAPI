using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Repositories.Implementations.Masters
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

            var parameters = new DynamicParameters();
            parameters.Add("@Type", "Setting");
            parameters.Add("@Where", cleanFilter);

            return await _sqlDataAccess.SelectDataFromSP<CompanySettingMaster>("GetALLMasterList", parameters, companyId);
        }

        public async Task<bool> SaveSettingDataAsync(CompanySettingMaster model, string actionType, string companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", model.Id);
            parameters.Add("@Name", model.Name);
            parameters.Add("@Value", model.Value);
            parameters.Add("@Description", model.Description);
            parameters.Add("@COMMENT", model.COMMENT);
            parameters.Add("@CompanyID", model.CompanyID);
            parameters.Add("@ActionType", actionType);

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

            var parameters = new DynamicParameters();
            parameters.Add("@UserTypeId", $"'{userTypeId}'");
            parameters.Add("@FormName", $"'{formName}'");
            parameters.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSP<dynamic>("GetUserPermission", parameters, companyId);
            return result.Any();
        }

        public async Task LogPageAccessAsync(string userName, string status, string device, string deviceType, string os, string ip, string companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Userid", userName);
            parameters.Add("@Status", status);
            parameters.Add("@Device", device ?? "Unknown");
            parameters.Add("@DeviceType", deviceType ?? "Unknown");
            parameters.Add("@OperatingSystem", os ?? "Unknown");
            parameters.Add("@Model", "API");
            parameters.Add("@IPLocation", ip ?? "Unknown");

            await _sqlDataAccess.ExecuteSP("sp_InsertPagesLog", parameters, companyId);
        }

        public async Task<IEnumerable<GetInvoiceFormatData>> GetInvoiceFormatListAsync(string filter, string companyId)
        {
            string cleanFilter = (filter ?? "").Replace("''", "'");

            var parameters = new DynamicParameters();
            parameters.Add("@Type", "GetInvoiceFormat");
            parameters.Add("@Where", cleanFilter);

            var result = await _sqlDataAccess.SelectDataFromSP<GetInvoiceFormatData>("GetALLMasterList", parameters, companyId);
            return result;
        }

        public async Task<string?> GetInvoicePathAsync(string filter, string companyId)
        {
            string cleanFilter = (filter ?? "").Replace("''", "'");

            var parameters = new DynamicParameters();
            parameters.Add("@Type", "PreviewPrint");
            parameters.Add("@Where", cleanFilter);

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetALLMasterList", parameters, companyId);
            return result;
        }

        public async Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync(string type, string columnName, string abbreviation, string companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Type", type);
            parameters.Add("@ColumnName", columnName);
            parameters.Add("@Abbreviation", abbreviation);

            var result = await _sqlDataAccess.SelectDataFromSP<ColumnAbbrMaster>("SP_UpdateColumnAbbreviation", parameters, companyId);
            return result;
        }
    }
}
