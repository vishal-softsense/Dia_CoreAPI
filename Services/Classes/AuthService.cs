using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services.Implementations
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

        public async Task<AuthResponseDto?> AuthenticateAsync(LoginDto model)
        {
            // Encrypt input password
            string encryptedPassword = _securityService.EncryptPassword(model.Password);

            // Fetch matching user from the database via repository
            var user = await _userRepository.GetUserByUsernameAndCompanyAsync(model.Username, model.CompanyId);
            if (user == null)
            {
                return null;
            }

            // Generate JWT Token for authenticated user
            string token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = user
            };
        }
    }
}
