using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Utility
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MasterPriceComparisonController : ControllerBase
    {
        private const string FormName = "Master Price Comparison";

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IMasterPriceComparisonService _service;

        public MasterPriceComparisonController(IMasterPriceComparisonService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;

        /// <summary>Matches MVC GET data/MasterPriceComparison — permission + filter dropdowns.</summary>
        [HttpGet("MasterPriceComparison")]
        public async Task<IActionResult> MasterPriceComparison()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });

                var dropdowns = await _service.LoadPageDataAsync(CompanyId);
                return new JsonResult(new { success = true, mode = "Add", Dropdowns = dropdowns }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Master Price Comparison.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST data/MasterPriceComparison-List.</summary>
        [HttpPost("MasterPriceComparison-List")]
        public async Task<IActionResult> MasterPriceComparisonList([FromBody] MasterPriceComparisonFilterDto filter)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });

                var report = await _service.GetReportAsync(filter ?? new MasterPriceComparisonFilterDto(), CompanyId);
                return new JsonResult(new { success = true, Data = report }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }
    }
}
