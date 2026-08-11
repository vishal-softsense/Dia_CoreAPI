using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Jewellery.JewelleryReports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JewelleryStockHistoryReportController : ControllerBase
    {
        private readonly IJewelleryStockHistoryReportService _service;

        public JewelleryStockHistoryReportController(IJewelleryStockHistoryReportService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        [HttpPost("search")]
        [HttpPost("jewstock-register-report-datatable")]
        public async Task<IActionResult> Search([FromBody] JewelleryStockHistoryReportFilterDto filter)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "Jewellery Stock History Report", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });
                }

                var data = await _service.GetReportAsync(filter, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "Jewellery Stock History Report", "select", CompanyId);
                if (permission != 1)
                {
                    return StatusCode(403, new { success = false, message = "Access denied." });
                }

                var list = await _service.GetDropdownsAsync(CompanyId);

                var helper = new Func<string, object>(type =>
                    list.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                        .Select(x => new { id = x.ID, name = x.Name })
                        .ToList());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        shape = helper("Shape"),
                        color = helper("Color"),
                        size = helper("Size"),
                        clarity = helper("Clarity")
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("stock-ids")]
        public async Task<IActionResult> GetStockIds([FromBody] JewelleryStockIdsRequestDto request)
        {
            try
            {
                var data = await _service.GetJewStockIdListAsync(request?.SearchText ?? "", CompanyId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
