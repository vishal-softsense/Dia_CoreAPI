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
    public class ProfitLossReportController : ControllerBase
    {
        // Match MVC TradingAccountController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IProfitLossReportService _reportService;

        public ProfitLossReportController(IProfitLossReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET Account/TradingAccount — permission gate for page open.
        /// </summary>
        [HttpGet("TradingAccount")]
        public async Task<IActionResult> TradingAccount()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "TradingAccount",
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
                return StatusCode(500, new { success = false, message = "Error loading profit and loss report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST Profitandloss/Getprofitandlossdata → Json(new { success = true, Data = LdgDetails })
        /// </summary>
        [HttpPost("Getprofitandlossdata")]
        public async Task<IActionResult> GetProfitAndLossData(
            [FromBody] ProfitLossReportFilterDTO filter,
            [FromQuery] string? value = null)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "TradingAccount",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                bool hideZeroBalance = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                var reportData = await _reportService.GetProfitLossReportAsync(filter, hideZeroBalance, CompanyId);
                return new JsonResult(new { success = true, Data = reportData }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching profit and loss report.", error = ex.Message });
            }
        }
    }
}
