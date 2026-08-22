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
    public class ImportPricingController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IImportPricingService _service;

        public ImportPricingController(IImportPricingService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? User.FindFirstValue("userid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

        /// <summary>Matches MVC GET /data/import-pricing permission gate.</summary>
        [HttpGet("import-pricing")]
        public async Task<IActionResult> ImportPricingData()
        {
            try
            {
                var result = await _service.ImportPricingDataAsync(UserTypeId, CompanyId);
                if (result.Success)
                    return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);

                return StatusCode(403, new { success = false, message = result.Data ?? "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Import Pricing.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST /data/import-pricing Excel upload.</summary>
        [HttpPost("imports-pricing")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPricing([FromForm] ImportPricingDTO importData)
        {
            try
            {
                var result = await _service.ImportPricingFileAsync(importData, UserTypeId, CompanyId);
                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST /data/save-price.</summary>
        [HttpPost("save-price")]
        [Consumes("application/json")]
        public async Task<IActionResult> SavePricing([FromBody] SavePricingRequestDto request)
        {
            try
            {
                var result = await _service.SavePricingAsync(request, UserTypeId, UserId, CompanyId);
                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, Data = ex.Message }, PascalCaseJson);
            }
        }
    }
}
