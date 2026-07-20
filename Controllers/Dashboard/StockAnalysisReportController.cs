using System;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockAnalysisReportController : ControllerBase
    {
        private readonly IStockAnalysisReportService _service;

        public StockAnalysisReportController(IStockAnalysisReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
            => User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");

        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] StockAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });
                if (string.IsNullOrWhiteSpace(filter.StockAna))
                    return BadRequest(new { success = false, message = "StockAna grouping is required." });

                var data = await _service.GetReportAsync(filter, GetCompanyId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("pivotbyfilter")]
        public async Task<IActionResult> GetPivotByFilter([FromBody] StockAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var data = await _service.GetPivotByFilterAsync(filter, GetCompanyId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("pivotfixed")]
        public async Task<IActionResult> GetPivotFixed([FromBody] StockAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var data = await _service.GetPivotFixedAsync(filter, GetCompanyId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var list = await _service.GetDropdownDataAsync(GetCompanyId());
                var helper = new Func<string, object>(type =>
                    list.Where(x => x.Type == type).Select(x => new { id = x.ID, name = x.Name }).ToList());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        shapeGroup = helper("ShapeGroup"),
                        shape = helper("Shape"),
                        colorGroup = helper("ColorGroup"),
                        color = helper("Color"),
                        size = helper("Size"),
                        clarity = helper("Clarity"),
                        proportion = helper("Proportion"),
                        polish = helper("Polish"),
                        symmetry = helper("Symmetry"),
                        lab = helper("LAB"),
                        rough = helper("Rough"),
                        ha = helper("HA"),
                        location = helper("Location"),
                        fluorescence = helper("Fluorescence"),
                        milky = helper("Milky"),
                        luster = helper("Luster"),
                        natts = helper("Natts"),
                        shade = helper("Shade"),
                        flColor = helper("FlColor"),
                        itemGroup = helper("ItemGroup"),
                        pair = helper("Pair"),
                        boxName = helper("BoxName")
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
