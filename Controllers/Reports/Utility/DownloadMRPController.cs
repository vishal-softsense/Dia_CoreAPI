using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Utility
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DownloadMRPController : ControllerBase
    {
        // Match MVC JsonResult (no camelCase renaming) — success / Data
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IDownloadMRPService _service;

        public DownloadMRPController(IDownloadMRPService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET data/downloadMRP — permission gate for page open.
        /// Form: "Download MRP", action: insert (same as MVC CheckUserPermission).
        /// </summary>
        [HttpGet("downloadMRP")]
        public async Task<IActionResult> DownloadMRPData()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(
                    UserTypeId,
                    "Download MRP",
                    "insert",
                    CompanyId);

                if (permission == 1)
                {
                    return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);
                }

                return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Download MRP.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST data/downloadMRP → Json(new { success = true, Data = "MRP Download Succesfull." })
        /// </summary>
        [HttpPost("downloadsMRP")]
        public async Task<IActionResult> DownloadMRP()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(
                    UserTypeId,
                    "Download MRP",
                    "insert",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });
                }

                var (success, data) = await _service.DownloadMRPAsync(CompanyId);
                return new JsonResult(new { success, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST data/updateMRP → Json(new { success = true, Data = "..." })
        /// </summary>
        [HttpPost("updateMRP")]
        public async Task<IActionResult> UpdateMRP()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(
                    UserTypeId,
                    "Download MRP",
                    "insert",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });
                }

                var (success, data) = await _service.UpdateMRPAsync(CompanyId);
                return new JsonResult(new { success, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }
    }
}
