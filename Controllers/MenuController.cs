using System;
using System.Threading.Tasks;
using Dia_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        [HttpGet("sidebar-menu")]
        public async Task<IActionResult> GetSidebarMenu()
        {
            try
            {
                var userId = GetUserId();
                var companyId = GetCompanyId();
                var result = await _menuService.GetUserSidebarMenuAsync(userId, companyId);
                
                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
