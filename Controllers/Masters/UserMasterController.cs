using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Masters
{
    [ApiController]
    [Route("api/Masters/[controller]")]
    [Authorize] // Requires JWT Token
    public class UserMasterController : ControllerBase
    {
        private readonly IUserMasterService _userService;

        public UserMasterController(IUserMasterService userService)
        {
            _userService = userService;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new System.UnauthorizedAccessException("CompanyID missing from token.");
        }

        private string GetUsername()
        {
            return User.FindFirst("UserName")?.Value ?? "System";
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllUsers()
        {
            var companyId = GetCompanyId();
            var users = await _userService.GetAllUsersAsync(companyId);
            return Ok(new { Success = true, Data = users });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var companyId = GetCompanyId();
            var user = await _userService.GetUserByIdAsync(id, companyId);
            
            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            return Ok(new { Success = true, Data = user });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] UserMasterDto model)
        {
            // Override CompanyID with the securely authenticated one
            model.CompanyID = int.Parse(GetCompanyId());
            var username = GetUsername();

            var (success, message) = await _userService.CreateUserAsync(model, username);
            
            if (success)
                return Ok(new { Success = true, Message = message });
            
            return BadRequest(new { Success = false, Message = message });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserMasterDto model)
        {
            if (id != model.UserID)
                return BadRequest(new { Success = false, Message = "ID mismatch." });

            model.CompanyID = int.Parse(GetCompanyId());
            var username = GetUsername();

            var (success, message) = await _userService.UpdateUserAsync(model, username);
            
            if (success)
                return Ok(new { Success = true, Message = message });
            
            return BadRequest(new { Success = false, Message = message });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var companyId = GetCompanyId();
            var (success, message) = await _userService.DeleteUserAsync(id, companyId);
            
            if (success)
                return Ok(new { Success = true, Message = message });
            
            return BadRequest(new { Success = false, Message = message });
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetUserDropdown()
        {
            var companyId = GetCompanyId();
            var dropdown = await _userService.GetUserDropdownAsync(companyId);
            return Ok(new { Success = true, Data = dropdown });
        }
    }
}
