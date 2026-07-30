using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.ParcelDashboard
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParcelPurchaseAnalysisController : ControllerBase
    {
        private readonly IParcelPurchaseAnalysisService _service;

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        public ParcelPurchaseAnalysisController(IParcelPurchaseAnalysisService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("parcelpur-data")]
        public async Task<IActionResult> GetParcelPurchaseAnalysisData([FromBody] ParcelPurchaseAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetParcelPurchaseAnalysisDataAsync(filter, companyId);

                if (data == null)
                {
                    return Ok(new { error = false, message = "" });
                }

                return new JsonResult(new { success = true, Data = data }, PascalCaseJson);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetPurchaseSearchDropdownAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
