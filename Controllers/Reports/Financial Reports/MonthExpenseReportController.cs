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
    public class MonthExpenseReportController : ControllerBase
    {
        // Match MVC AccReportController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IMonthExpenseReportService _reportService;

        public MonthExpenseReportController(IMonthExpenseReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET report/month-expense-report — permission gate for page open.
        /// </summary>
        [HttpGet("month-expense-report")]
        public async Task<IActionResult> MonthExpenseReport()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "Month Expense Report",
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
                return StatusCode(500, new { success = false, message = "Error loading month expense report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST report/month-expense-report → Json(new { success = true, data = result })
        /// </summary>
        [HttpPost("month-expense-report")]
        public async Task<IActionResult> MonthExpenseReportData()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "Month Expense Report",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var result = await _reportService.GetMonthExpenseReportAsync(CompanyId);
                // MVC: return Json(new { success = true, data = result })
                return new JsonResult(new { success = true, data = result }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching month expense report.", error = ex.Message });
            }
        }
    }
}
