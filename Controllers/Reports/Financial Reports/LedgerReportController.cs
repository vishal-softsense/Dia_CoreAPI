using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dia_CoreAPI.DTOs.Reports.Financial_Reports;
using Dia_CoreAPI.Services.Interfaces.Reports.Financial_Reports;

namespace Dia_CoreAPI.Controllers.Reports.Financial_Reports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LedgerReportController : ControllerBase
    {
        // Match MVC AccReportController JsonResult (no camelCase renaming)
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly ILedgerReportService _reportService;

        public LedgerReportController(ILedgerReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;
        private string UserName => User.FindFirstValue("UserName") ?? string.Empty;
        private string SearchLogDevice => User.FindFirstValue("SearchLogDevice") ?? string.Empty;
        private string SearchLogDeviceType => User.FindFirstValue("SearchLogDeviceType") ?? string.Empty;
        private string SearchLogOS => User.FindFirstValue("SearchLogOS") ?? string.Empty;
        private string SearchLogIP => User.FindFirstValue("SearchLogIP") ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        [HttpGet("LedgerRpt")]
        public async Task<IActionResult> LedgerReport()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "Ledger", "select", CompanyId);
                if (permission == 1)
                {
                    var dropdownData = await _reportService.GetDropdownDataAsync(CompanyId);

                    return new JsonResult(new { success = true, mode = "Add", data = dropdownData }, PascalCaseJson);
                }
                else
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading ledger report options.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC AccReportController.LedgerReportViews → Json(new { data = list })
        /// </summary>
        [HttpPost("GetLedgerReportLists")]
        public async Task<IActionResult> LedgerReportViews([FromBody] LedgerReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "Ledger", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var list = await _reportService.GetLedgerReportViewsAsync(filter, CompanyId);
                // MVC: return Json(new { data = list })
                return new JsonResult(new { data = list }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching ledger report views.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC AccReportController.GetLedgerReporList1 → Json(new { success = true, Data = ... })
        /// </summary>
        [HttpPost("GetLedgerReporList1")]
        public async Task<IActionResult> GetLedgerReporList1([FromBody] LedgerReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "Ledger", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var reportData = await _reportService.GetLedgerReporList1Async(filter, CompanyId);
                // MVC: return Json(new { success = true, Data = LdgDetailsdata })
                return new JsonResult(new { success = true, Data = reportData }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching ledger report list.", error = ex.Message });
            }
        }

        /// <summary>
        /// Matches MVC AccReportController.LedgerReportPrintJS — returns print model JSON
        /// (React renders LedgerReportJS_Print layout / opens print window).
        /// </summary>
        [HttpPost("LedgerReportPrintJS")]
        public async Task<IActionResult> LedgerReportPrintJS([FromBody] LedgerReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            if (filter.Party <= 0)
            {
                return BadRequest(new { success = false, message = "Please select a party before continuing." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "Ledger", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
                }

                var printData = await _reportService.GetLedgerReportPrintAsync(filter, CompanyId);
                return new JsonResult(new { success = true, data = printData }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error generating ledger print.", error = ex.Message });
            }
        }

        /// <summary>
        /// GET parity with MVC route Account/LedgerReportPrintJS?FromDate&ToDate&Party&Curcode&PartyName
        /// </summary>
        [HttpGet("LedgerReportPrintJS")]
        public async Task<IActionResult> LedgerReportPrintJSGet(
            [FromQuery] DateTime FromDate,
            [FromQuery] DateTime ToDate,
            [FromQuery] int Party,
            [FromQuery] int Curcode = 0,
            [FromQuery] string? PartyName = null)
        {
            var filter = new LedgerReportFilterDTO
            {
                FromDate = FromDate,
                ToDate = ToDate,
                Party = Party,
                Curcode = Curcode,
                PartyName = PartyName
            };

            return await LedgerReportPrintJS(filter);
        }
    }
}
