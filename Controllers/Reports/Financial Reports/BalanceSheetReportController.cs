using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Financial_Reports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BalanceSheetReportController : ControllerBase
    {
        // Match MVC BalanceSheetController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IBalanceSheetReportService _reportService;

        public BalanceSheetReportController(IBalanceSheetReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET Account/BalanceSheet — permission gate for page open.
        /// </summary>
        [HttpGet("BalanceSheet")]
        public async Task<IActionResult> BalanceSheet()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "BalanceSheet",
                    "select",
                    CompanyId);

                if (permission == 1)
                {
                    return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);
                }

                return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading balance sheet report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST balsheetRpt/GetbalsheetReportListNew → Json(new { success = true, Data = balancesheetdata })
        /// </summary>
        [HttpPost("GetbalsheetReportListNew")]
        public async Task<IActionResult> GetBalanceSheetReportListNew(
            [FromBody] BalanceSheetReportFilterDTO filter,
            [FromQuery] string? Value = null)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "BalanceSheet",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                bool hideZeroBalance = string.Equals(Value, "true", StringComparison.OrdinalIgnoreCase);
                var reportData = await _reportService.GetBalanceSheetReportAsync(filter, hideZeroBalance, CompanyId);
                return new JsonResult(new { success = true, Data = reportData }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching balance sheet report.", error = ex.Message });
            }
        }
    }
}
