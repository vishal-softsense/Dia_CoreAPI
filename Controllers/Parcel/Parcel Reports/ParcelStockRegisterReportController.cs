using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ParcelStockRegisterReportController : ControllerBase
    {
        private readonly IParcelStockRegisterReportService _reportService;
        private readonly ILogger<ParcelStockRegisterReportController> _logger;

        public ParcelStockRegisterReportController(
            IParcelStockRegisterReportService reportService,
            ILogger<ParcelStockRegisterReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetParcelStockRegisterReportDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                if (companyId == "0")
                {
                    return Unauthorized(new { Success = false, Message = "CompanyID missing from token." });
                }

                bool hasPermission = await _reportService.HasUserPermissionAsync(
                    GetUserTypeId(),
                    "Parcel Stock History Report",
                    "select",
                    companyId);

                if (!hasPermission)
                {
                    return Ok(new { Success = false, Message = "You are not authorized to perform this task." });
                }

                var dropdownData = await _reportService.GetReportDropdownsAsync(companyId);
                LogUserAccess("PStockHistoryReport");

                return Ok(new { Success = true, Data = dropdownData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dropdown options for Parcel Stock Register Report.");
                return StatusCode(500, new { Success = false, Message = "An internal server error occurred while fetching dropdowns." });
            }
        }

        [HttpPost("datatable")]
        public async Task<IActionResult> GetParcelStockRegisterReport([FromBody] StockRegisterReportFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                if (companyId == "0")
                {
                    return Unauthorized(new { Success = false, Message = "CompanyID missing from token." });
                }

                // MVC only gates the page GET; the datatable POST does not re-check permission.
                // Soft-check here so a missing form permission does not return 403 (which the UI
                // surfaces as "Server Connection Problem..").
                bool hasPermission = await _reportService.HasUserPermissionAsync(
                    GetUserTypeId(),
                    "Parcel Stock History Report",
                    "select",
                    companyId);

                if (!hasPermission)
                {
                    return Ok(new { Success = false, Message = "You are not authorized to perform this task." });
                }

                if (filter == null)
                {
                    return BadRequest(new { Success = false, Message = "Filter payload is required." });
                }

                var reportData = await _reportService.GetParcelStockRegisterReportAsync(filter, companyId);
                return Ok(new { Success = true, Data = reportData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Parcel Stock Register Report datatable.");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("get-parcel-lots")]
        public async Task<IActionResult> GetParcelLots([FromQuery] string? searchby)
        {
            try
            {
                var companyId = GetCompanyId();
                if (companyId == "0")
                {
                    return Unauthorized(new { Success = false, Message = "CompanyID missing from token." });
                }

                var data = await _reportService.GetParcelLotNoListAsync(searchby ?? "", companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        private void LogUserAccess(string page)
        {
            var userName = User.FindFirst("UserName")?.Value ?? "Unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Perform audit logging logic here
        }
    }
}
