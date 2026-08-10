using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Financial_Reports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CashFlowReportController : ControllerBase
    {
        // Match MVC AccReportController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly ICashFlowReportService _reportService;

        public CashFlowReportController(ICashFlowReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET report/cash-flow-report — permission gate for page open.
        /// </summary>
        [HttpGet("cash-flow-report")]
        public async Task<IActionResult> CashFlowReport()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "Cash Flow Report",
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
                return StatusCode(500, new { success = false, message = "Error loading cash flow report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST report/cash-flow-report → Json(new { success = true, data = result })
        /// </summary>
        [HttpPost("cash-flow-report")]
        public async Task<IActionResult> CashFlowReportData()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "Cash Flow Report",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var result = await _reportService.GetCashFlowReportAsync(CompanyId);
                return new JsonResult(new { success = true, data = result }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching cash flow report.", error = ex.Message });
            }
        }
    }
}
