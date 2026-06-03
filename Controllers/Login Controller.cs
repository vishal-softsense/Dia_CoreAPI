using System;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var authResult = await _authService.AuthenticateAsync(model);

                if (authResult == null)
                {
                    return Unauthorized(new { Success = false, Message = "User not found" });

                }

                return Ok(new { Success = true, Token = authResult.Token, User = authResult.User });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });

            }
        }
    }
}
