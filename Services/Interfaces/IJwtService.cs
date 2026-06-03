using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(LoginResponseModel user);
    }
}
