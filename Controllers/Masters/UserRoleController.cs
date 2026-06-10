using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Masters
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";
        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetRoleDropdown()
        {
            try
            {
                var result = await _userRoleService.GetAllRolesAsync(GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("list-user-role")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var result = await _userRoleService.GetAllRolesAsync(GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("edit-user-role/{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var result = await _userRoleService.GetRoleByIdAsync(id, GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("add-user-role-save")]
        public async Task<IActionResult> CreateRole([FromBody] UserRoleDto roleDto)
        {
            if (!ModelState.IsValid)
                return Ok(new { Success = false, Data = "Invalid payload" });

            try
            {
                var result = await _userRoleService.SaveRoleAsync(roleDto, GetUserId(), GetCompanyId());
                return Ok(new { Success = true, Data = "Record Saved." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("edit-user-role-save")]
        public async Task<IActionResult> UpdateRole([FromBody] UserRoleDto roleDto)
        {
            if (!ModelState.IsValid)
                return Ok(new { Success = false, Data = "Invalid payload" });

            try
            {
                var result = await _userRoleService.UpdateRoleAsync(roleDto, GetUserId(), GetCompanyId());
                return Ok(new { Success = true, Data = "Record Updated." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-user-role")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _userRoleService.DeleteRoleAsync(id, GetCompanyId());
                return Ok(new { Success = true, Data = "Records Deleted." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("fill-role-permission")]
        public async Task<IActionResult> GetRolePermissions(int id)
        {
            try
            {
                var result = await _userRoleService.GetRolePermissionsAsync(id, GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-role-permission")]
        public async Task<IActionResult> SaveRolePermissions(int id, [FromBody] List<UserPermissionSaveDto> permissions)
        {
            if (!ModelState.IsValid)
                return Ok(new { Success = false, Data = "Invalid payload" });

            try
            {
                var result = await _userRoleService.SaveRolePermissionsAsync(id, permissions, GetUserId(), GetCompanyId());
                return Ok(new { Success = true, Data = "Record saved successfully." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
    }
}
