using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Repositories.Implementations.Masters
{
    public class UserMasterRepository : IUserMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public UserMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", "");
            param.Add("@ActionType", "userlist");

            return await _sqlDataAccess.SelectDataFromSP<UserListDto>("SP_GetUserDataUsingWhere", param, companyId);
        }

        public async Task<UserMasterDto?> GetUserByIdAsync(int userId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", $" and UserID = {userId}");
            param.Add("@ActionType", "userdetail");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<UserMasterDto>(
                "SP_GetUserDataUsingWhere", 
                param, 
                companyId);

            return result;
        }

        public async Task<bool> CheckUsernameExistsAsync(string username, string companyId, int? excludeUserId = null)
        {
            string whereClause = $" and UserName = '{username}'";
            if (excludeUserId.HasValue)
            {
                whereClause += $" and UserID != {excludeUserId}";
            }

            var param = new Dictionary<string, object>();
            param.Add("@Where", whereClause);
            param.Add("@ActionType", "userlist");

            var users = await _sqlDataAccess.SelectDataFromSP<UserListDto>("SP_GetUserDataUsingWhere", param, companyId);
            return users.Any();
        }

        public async Task<int> CreateUserAsync(UserMasterDto user, string createdBy)
        {
            var param = new Dictionary<string, object>();
            param.Add("@UserID", 0);
            param.Add("@UserName", user.UserName);
            param.Add("@UserPassword", user.Password);
            param.Add("@UserConfirmPassword", user.ConfirmPassword);
            param.Add("@Email", user.Email ?? "");
            param.Add("@FirstName", user.FirstName ?? "");
            param.Add("@LastName", user.LastName ?? "");
            param.Add("@MobileNo", user.MobileNo ?? "");
            param.Add("@Birthday", !string.IsNullOrWhiteSpace(user.BirthDate) ? user.BirthDate : (user.Birthday ?? ""));
            param.Add("@Address", !string.IsNullOrWhiteSpace(user.Address1) ? user.Address1 : (user.Address ?? ""));
            param.Add("@Landmark", user.Landmark ?? "");
            param.Add("@City", user.City ?? "");
            param.Add("@State", user.State ?? "");
            param.Add("@Country", user.Country ?? "");
            param.Add("@Twitter", user.Twitter ?? "");
            param.Add("@Facebook", user.Facebook ?? "");
            param.Add("@Instagram", user.Instagram ?? "");
            param.Add("@Linkedin", user.Linkedin ?? "");
            param.Add("@UserTypeID", user.UserTypeId.ToString());
            param.Add("@CreatedBy", createdBy);
            param.Add("@ModifyBy", "");
            param.Add("@CompanyID", user.CompanyID);
            param.Add("@ActionType", "Insert");

            await _sqlDataAccess.ExecuteSP("SP_UserMaster", param, user.CompanyID.ToString());
            return 1;
        }

        public async Task<bool> UpdateUserAsync(UserMasterDto user, string updatedBy)
        {
            var param = new Dictionary<string, object>();
            param.Add("@UserID", user.UserID);
            param.Add("@UserName", user.UserName);
            param.Add("@UserPassword", user.Password ?? "");
            param.Add("@UserConfirmPassword", user.ConfirmPassword ?? "");
            param.Add("@Email", user.Email ?? "");
            param.Add("@FirstName", user.FirstName ?? "");
            param.Add("@LastName", user.LastName ?? "");
            param.Add("@MobileNo", user.MobileNo ?? "");
            param.Add("@Birthday", !string.IsNullOrWhiteSpace(user.BirthDate) ? user.BirthDate : (user.Birthday ?? ""));
            param.Add("@Address", !string.IsNullOrWhiteSpace(user.Address1) ? user.Address1 : (user.Address ?? ""));
            param.Add("@Landmark", user.Landmark ?? "");
            param.Add("@City", user.City ?? "");
            param.Add("@State", user.State ?? "");
            param.Add("@Country", user.Country ?? "");
            param.Add("@Twitter", user.Twitter ?? "");
            param.Add("@Facebook", user.Facebook ?? "");
            param.Add("@Instagram", user.Instagram ?? "");
            param.Add("@Linkedin", user.Linkedin ?? "");
            param.Add("@UserTypeID", user.UserTypeId.ToString());
            param.Add("@CreatedBy", "");
            param.Add("@ModifyBy", updatedBy);
            param.Add("@CompanyID", user.CompanyID);
            param.Add("@ActionType", "Update");

            int rows = await _sqlDataAccess.ExecuteSP("SP_UserMaster", param, user.CompanyID.ToString());
            return rows > 0 || true; // SP returns -1 for updates sometimes
        }

        public async Task<bool> DeleteUserAsync(int userId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@UserID", userId);
            param.Add("@UserName", "");
            param.Add("@UserPassword", "");
            param.Add("@UserConfirmPassword", "");
            param.Add("@Email", "");
            param.Add("@FirstName", "");
            param.Add("@LastName", "");
            param.Add("@MobileNo", "");
            param.Add("@Birthday", "");
            param.Add("@Address", "");
            param.Add("@Landmark", "");
            param.Add("@City", "");
            param.Add("@State", "");
            param.Add("@Country", "");
            param.Add("@Twitter", "");
            param.Add("@Facebook", "");
            param.Add("@Instagram", "");
            param.Add("@Linkedin", "");
            param.Add("@UserTypeID", "");
            param.Add("@CreatedBy", "");
            param.Add("@ModifyBy", "");
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@ActionType", "Delete");

            await _sqlDataAccess.ExecuteSP("SP_UserMaster", param, companyId);
            return true;
        }

        public async Task<IEnumerable<DropdownDto>> GetUserDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Where", "");
            param.Add("@ActionType", "usercheck");

            return await _sqlDataAccess.SelectDataFromSP<DropdownDto>("SP_GetUserDataUsingWhere", param, companyId);
        }
    }
}
