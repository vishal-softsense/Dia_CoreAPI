using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Dia_CoreAPI.Repositories.Classes.Masters
{
    public class SyncSettingRepository : ISyncSettingRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly string _connectionString =
            "Data Source=3.110.39.159;Initial Catalog=MVC_StockSync;Persist Security Info=True;User ID=sa;Password=SSMvc@123;TrustServerCertificate=True;";
        public SyncSettingRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
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

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission",
                parameters,
                companyId);

            return result.Count();
        }
        //public async Task<ApiResponse> SaveSyncAsync(
        //    ProviderSettings model,
        //    int companyId,
        //    string companyName,
        //    string sqlConnection,int enable)
        //{
        //    // Save Code

        //    return new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Sync Saved Successfully"
        //    };
        //}



        public async Task<ApiResponse> SaveSyncAsync(
            ProviderSettings model,
            string companyId,
            string companyName,
            string sqlConnection)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                DataTable dtStockSQL = new DataTable();

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("User_SelectDataUsingWhereClause", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Where", "");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dtStockSQL);
                        }
                    }
                }

                DataRow dr = dtStockSQL.NewRow();

                dr["CompanyID"] = companyId;
                dr["Name"] = companyName;
                dr["SyncPlatform"] = model.provider;
                dr["ConnectionString"] = sqlConnection;
                dr["Enable"] = model.enabled;
                dr["UploadType"] = model.mode;
                dr["WhereCondition"] = "";
                dr["ApiUserName"] = model.apiUser;
                dr["ApiPassword"] = model.apiPass;
                dr["ApiUrl"] = model.apiLink;
                dr["FtpUsername"] = model.ftpUser;
                dr["FtpPassword"] = model.ftpPass;
                dr["FtpHost"] = model.ftpHost;
                dr["Port"] = model.ftpPort;
                dr["AmtPercent"] = model.ftpAmt;

                dtStockSQL.Rows.Add(dr);

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    using (SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT * FROM TabCompanySyncMaster WHERE 1=0", con))
                    {
                        SqlCommandBuilder cb = new SqlCommandBuilder(da);
                        da.Update(dtStockSQL);
                    }
                }

                response.Success = true;
                response.Message = "Upload Stock Successfully";
                response.Data = null;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ApiResponse> GetSyncAsync(int companyId)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT * FROM TabCompanySyncMaster WHERE CompanyID=@CompanyID", con))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                var list = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    var dic = new Dictionary<string, object>();

                    foreach (DataColumn col in dt.Columns)
                    {
                        dic[col.ColumnName] = row[col] == DBNull.Value
                            ? null
                            : row[col];
                    }

                    list.Add(dic);
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
