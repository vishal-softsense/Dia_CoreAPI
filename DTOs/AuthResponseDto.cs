using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public LoginResponseModel User { get; set; } = new();
    }
}
