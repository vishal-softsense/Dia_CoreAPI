using System;
using System.Collections.Generic;
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
    public class JewelleryStockReportController : ControllerBase
    {
        private readonly IJewelleryStockReportServices _service;

        public JewelleryStockReportController(IJewelleryStockReportServices service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        [HttpPost("jewellery-stock-search-result")]
        public async Task<IActionResult> GetJewelleryStockReport([FromBody] JewelleryStockSearchFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetJewelleryStockReportAsync(filter, companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var dropdowns = await _service.GetJewelleryStockDropdownsAsync(companyId);
                return Ok(new { Success = true, Data = dropdowns });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("stock-ids")]
        public async Task<IActionResult> GetStockIds([FromBody] JewelleryStockIdsRequestDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetStockIdsAsync(request?.SearchText ?? "", companyId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
