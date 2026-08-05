using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Controllers.Parcel.Parcel_Reports
{
    [ApiController]
    [Route("api/mixreport")]
    [Authorize]
    public class MixPurchaseReportController : ControllerBase
    {
        private readonly IMixPurchaseReportService _reportService;

        public MixPurchaseReportController(IMixPurchaseReportService reportService)
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
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "MixPurchaseReport", "select", CompanyId);
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

        [HttpPost("generalpurchase-report-list")]
        public async Task<IActionResult> GetGeneralPurRpt([FromBody] MixPurchaseReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "MixPurchaseReport", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "Access denied." });
                }

                var report = await _reportService.GetGeneralPurchaseParcelReportAsync(filter, CompanyId);
                return Ok(new { success = true, Data = report });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching report data.", error = ex.Message });
            }
        }
    }
}