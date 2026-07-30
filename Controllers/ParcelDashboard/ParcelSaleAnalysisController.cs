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
    public class ParcelSaleAnalysisController : ControllerBase
    {
        private readonly IParcelSaleAnalysisService _service;

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        public ParcelSaleAnalysisController(IParcelSaleAnalysisService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("parcelsale-data")]
        public async Task<IActionResult> GetParcelSaleAnalysisData([FromBody] ParcelSaleAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetParcelSaleAnalysisDataAsync(filter, companyId);

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

        [HttpPost("parcelsale-pivotdata")]
        public async Task<IActionResult> GetParcelSalePivotData([FromBody] ParcelSaleAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetParcelSalePivotDataAsync(filter, companyId);

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
                var data = await _service.GetSaleSearchDropdownAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
