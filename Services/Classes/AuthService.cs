using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services.Classes
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepository,
            ISecurityService securityService,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
            _jwtService = jwtService;
        }

        public async Task<(AuthResponseDto? Result, string ErrorMessage)> AuthenticateAsync(LoginDto model)
        {
            // Encrypt input password
            string encryptedPassword = _securityService.EncryptPassword(model.Password);

            // Fetch matching user from the database via repository (verifying password)
            var (user, errorMessage) = await _userRepository.GetUserByCredentialsAsync(model.Username, encryptedPassword, model.CompanyId);
            if (user == null)
            {
                return (null, errorMessage);
            }

            // Generate JWT Token for authenticated user
            string token = _jwtService.GenerateToken(user);

            var result = new AuthResponseDto
            {
                Token = token,
                User = user
            };

            return (result, string.Empty);
        }
    }
}
