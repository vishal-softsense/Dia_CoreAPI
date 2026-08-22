using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Utility
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MissingStockController : ControllerBase
    {
        private const string FormName = "MissingStock";

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IMissingStockService _service;

        public MissingStockController(IMissingStockService service)
        {
            _service = service;
        }

        private string UserTypeId =>
            User.FindFirstValue("UserTypeId")
            ?? User.FindFirstValue("usertypeid")
            ?? string.Empty;

        private string CompanyId =>
            User.FindFirstValue("CompanyID")
            ?? User.FindFirstValue("CompanyId")
            ?? User.FindFirstValue("companyid")
            ?? string.Empty;

        /// <summary>GET data/MissingStock — permission gate (Form: Missing Stock, action: insert).</summary>
        [HttpGet("MissingStock")]
        public async Task<IActionResult> MissingStockPage()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });

                return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Missing Stock.", error = ex.Message });
            }
        }

        /// <summary>POST Missing-stock-search — permission + report.</summary>
        [HttpPost("Missing-stock-search")]
        public async Task<IActionResult> MissingStockSearch([FromBody] MissingStockFilterDto filter)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });

                var report = await _service.GetReportAsync(filter ?? new MissingStockFilterDto(), CompanyId);
                return new JsonResult(new { success = true, Data = report }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }
    }
}
