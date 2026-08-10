using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using Dia_CoreAPI.Services.Interfaces.Reports.Payable_Receivables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Payable_Receivables
{
    [ApiController]
    [Route("api/OutstandingReport")]
    [Authorize]
    public class OutstandingReportController : ControllerBase
    {
        private readonly IOutstandingReportService _reportService;

        public OutstandingReportController(IOutstandingReportService reportService)
        {
            _reportService = reportService;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId, "Outstanding Report", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "Access denied." });
                }

                var dropdownData = await _reportService.GetDropdownDataAsync(CompanyId);
                return Ok(new { success = true, data = dropdownData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching dropdowns.", error = ex.Message });
            }
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetOutstandingData([FromBody] OutstandingReportFilterDto filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(
                    UserTypeId, "Outstanding Report", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "Access denied." });
                }

                var report = await _reportService.GetOutstandingReportAsync(filter, CompanyId);
                return Ok(new { success = true, Data = report });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching report data.", error = ex.Message });
            }
        }
    }
}
