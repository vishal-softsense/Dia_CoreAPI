using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;

namespace Dia_CoreAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(AuthResponseDto? Result, string ErrorMessage)> AuthenticateAsync(LoginDto model);
    }
}
