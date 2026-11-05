using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces;
using Dia_CoreAPI.Services.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class UserMasterService : IUserMasterService
    {
        private readonly IUserMasterRepository _userRepository;
        private readonly ISecurityService _securityService;

        public UserMasterService(IUserMasterRepository userRepository, ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(string companyId)
        {
            return await _userRepository.GetAllUsersAsync(companyId);
        }

        public async Task<UserMasterDto?> GetUserByIdAsync(int userId, string companyId)
        {
            return await _userRepository.GetUserByIdAsync(userId, companyId);
        }

        /// <summary>
        /// Validates new/changed passwords only. Login continues to accept existing (legacy) stored credentials.
        /// </summary>
        private static bool IsStrongPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters.";
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]"))
            {
                errorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
            {
                errorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
            {
                errorMessage = "Password must contain at least one number.";
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[^A-Za-z0-9]"))
            {
                errorMessage = "Password must contain at least one special symbol.";
                return false;
            }
            return true;
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(UserMasterDto user, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
                return (false, "Username is required.");
            
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password != user.ConfirmPassword)
                return (false, "Password and Confirm Password must match and cannot be empty.");

            if (!IsStrongPassword(user.Password, out string strengthError))
                return (false, strengthError);

            // Check duplicate username
            bool exists = await _userRepository.CheckUsernameExistsAsync(user.UserName, user.CompanyID.ToString());
            if (exists)
                return (false, $"User with username '{user.UserName}' already exists.");

            // Encrypt passwords securely using AES (never store plaintext)
            user.Password = _securityService.EncryptPassword(user.Password);
            user.ConfirmPassword = _securityService.EncryptPassword(user.ConfirmPassword);

            try
            {
                await _userRepository.CreateUserAsync(user, createdBy);
                return (true, "User created successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error creating user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(UserMasterDto user, string updatedBy)
        {
            // Check duplicate username (excluding the current user being edited)
            bool exists = await _userRepository.CheckUsernameExistsAsync(user.UserName, user.CompanyID.ToString(), user.UserID);
            if (exists)
                return (false, $"User with username '{user.UserName}' already exists.");

            // If password is provided, validate strength then encrypt (legacy passwords left unchanged when blank)
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (user.Password != user.ConfirmPassword)
                    return (false, "Password and Confirm Password must match.");

                if (!IsStrongPassword(user.Password, out string strengthError))
                    return (false, strengthError);

                user.Password = _securityService.EncryptPassword(user.Password);
                user.ConfirmPassword = _securityService.EncryptPassword(user.ConfirmPassword);
            }
            else
            {
                // Fetch existing user to retain current password (no re-encryption / no strength check)
                var existingUser = await _userRepository.GetUserByIdAsync(user.UserID, user.CompanyID.ToString());
                if (existingUser != null)
                {
                    user.Password = !string.IsNullOrWhiteSpace(existingUser.Password) ? existingUser.Password : existingUser.UserPassword;
                    user.ConfirmPassword = !string.IsNullOrWhiteSpace(existingUser.ConfirmPassword) ? existingUser.ConfirmPassword : existingUser.UserConfirmPassword;
                }
            }

            try
            {
                bool success = await _userRepository.UpdateUserAsync(user, updatedBy);
                if (success)
                    return (true, "User updated successfully.");
                else
                    return (false, "Record not found or update failed.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId, string companyId)
        {
            try
            {
                bool success = await _userRepository.DeleteUserAsync(userId, companyId);
                if (success)
                    return (true, "User deleted successfully.");
                else
                    return (false, "Record not found or delete failed.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting user: {ex.Message}");
            }
        }

        public async Task<IEnumerable<DropdownDto>> GetUserDropdownAsync(string companyId)
        {
            return await _userRepository.GetUserDropdownAsync(companyId);
        }
    }
}
