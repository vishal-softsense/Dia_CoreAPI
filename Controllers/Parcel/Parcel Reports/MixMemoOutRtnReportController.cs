using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Controllers.Parcel.Parcel_Reports
{
    [ApiController]
    [Route("api/MixMemo-Return-report")]
    [Authorize]
    public class MixMemoOutRtnReportController : ControllerBase
    {
        private readonly IMixMemoOutRtnReportService _reportService;

        public MixMemoOutRtnReportController(IMixMemoOutRtnReportService reportService)
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

        [HttpGet("mixmemoreturn-report-dropdowns")]
        public async Task<IActionResult> MixMemoOutReturnReport()
        {
            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "MixMemoReturnReport", "select", CompanyId);
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

        [HttpPost("mixmemoreturn-report-list")]
        public async Task<IActionResult> MixMemoReturnReport([FromBody] MixMemoOutRtnReportFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest(new { success = false, message = "Invalid filter data." });
            }

            try
            {
                int permission = await _reportService.CheckUserPermissionAsync(UserTypeId, "MixMemoReturnReport", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "Access denied." });
                }

                var report = await _reportService.GetMixMemoRtnReportAsync(filter, CompanyId);
                return Ok(new { success = true, Data = report });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching report data.", error = ex.Message });
            }
        }
    }
}