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
    public class InvoiceLedgerReportController : ControllerBase
    {
        // Match MVC AccReportController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IInvoiceLedgerReportService _reportService;

        public InvoiceLedgerReportController(IInvoiceLedgerReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        /// <summary>
        /// Matches MVC GET report/invoice-ledger-report — permission gate for page open.
        /// </summary>
        [HttpGet("invoice-ledger-report")]
        public async Task<IActionResult> InvoiceLedgerReport()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "InvoiceLedgerReport",
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
                return StatusCode(500, new { success = false, message = "Error loading invoice ledger report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC POST report/invoice-ledger-report → Json(new { success = true, data = result })
        /// </summary>
        [HttpPost("invoice-ledger-report")]
        public async Task<IActionResult> InvoiceLedgerReportData([FromBody] InvoiceLedgerReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId,
                    "InvoiceLedgerReport",
                    "select",
                    CompanyId);

                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                // MVC: only runs SP when intedatadays is 30/365 and intedataper is set
                if ((filter.intedatadays == "30" || filter.intedatadays == "365")
                    && !string.IsNullOrEmpty(filter.intedataper))
                {
                    var result = await _reportService.GetInvoiceLedgerReportAsync(filter, CompanyId);
                    return new JsonResult(new { success = true, data = result }, PascalCaseJson);
                }

                // MVC: return Json(new { error = false, message = "" });
                return new JsonResult(new { error = false, message = "" }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching invoice ledger report.", error = ex.Message });
            }
        }
    }
}
