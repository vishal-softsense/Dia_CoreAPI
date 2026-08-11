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
    public class TrialBalanceSheetReportController : ControllerBase
    {
        // Match MVC TrialBalanceSheetController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly ITrialBalanceSheetReportService _reportService;

        public TrialBalanceSheetReportController(ITrialBalanceSheetReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET Account/TrialBalSheet — permission gate for page open.
        /// </summary>
        [HttpGet("TrialBalSheet")]
        public async Task<IActionResult> TrialBalSheet()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "TrialBalanceSheet",
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
                return StatusCode(500, new { success = false, message = "Error loading trial balance sheet report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST TrialblsheetRpt/GetData → Json(new { success = true, Data = trialbalancesheetdata })
        /// </summary>
        [HttpPost("GetData")]
        public async Task<IActionResult> GetData([FromBody] TrialBalanceSheetReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "TrialBalanceSheet",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var reportData = await _reportService.GetTrialBalanceSheetReportAsync(filter, CompanyId);
                return new JsonResult(new { success = true, Data = reportData }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching trial balance sheet report.", error = ex.Message });
            }
        }
    }
}
