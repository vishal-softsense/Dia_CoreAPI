using System;
using System.Linq;
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
    public class ParcelStockAnalysisController : ControllerBase
    {
        private readonly IParcelStockAnalysisService _service;

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        public ParcelStockAnalysisController(IParcelStockAnalysisService service)
        {
            _service = service;
        }

        private string GetCompanyId()
            {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("parcelstockchart-data")]
        public async Task<IActionResult> GetParcelChartData([FromBody] ParcelStockAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetParcelChartDataAsync(filter, companyId);

                if (data == null)
                {
                    return Ok(new { error = false, message = "" });
                }

                return Ok(new { success = true, Data = data });
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

        [HttpPost("parcelstock-pivotdata")]
        public async Task<IActionResult> GetParcelPivotData([FromBody] ParcelStockAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetParcelPivotDataAsync(filter, companyId);

                var rows = data.Select(r => new
                {
                    r.LotNo,
                    Shape = r.Shape ?? "",
                    Size = r.Size ?? "",
                    Color = r.Color ?? "",
                    Clarity = r.Clarity ?? "",
                    Sieve = r.Sieve ?? "",
                    Location = r.Location ?? r.Country ?? r.City ?? "",
                    ItemGroupId = r.ItemGroupId ?? "",
                    BoxName = r.BoxName ?? "",
                    r.Pcs,
                    r.Carats,
                    r.Amount,
                    r.Rate,
                    r.Back,
                    r.AskingRate,
                    r.AskingBack,
                    r.AskingAmount,
                    r.MRPAmount,
                    r.PurChaseAmount
                }).ToList();

                return new JsonResult(new { success = true, Data = rows }, PascalCaseJson);
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
                var data = await _service.GetStockSearchDropdownAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
