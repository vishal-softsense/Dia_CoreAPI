using System;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Solitaires.Solitaires_Reports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolitaireAdminStockReportController : ControllerBase
    {
        private readonly ISolitaireAdminStockReportService _service;

        public SolitaireAdminStockReportController(ISolitaireAdminStockReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> GetAdminStockSearchReport([FromBody] SolitaireAdminStockReportDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAdminStockSearchReportAsync(filter, companyId);
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
                var list = await _service.GetAdminStockSearchDropdownAsync(companyId);
                return Ok(new { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("get-solitaire-lots")]
        public async Task<IActionResult> GetSolitaireLots([FromQuery] string? searchby)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAdminSolitaireLotNoListAsync(searchby ?? "", companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("update-location")]
        public async Task<IActionResult> UpdateLocation([FromBody] SolitaireAdminLocationUpdateDTO dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var res = await _service.UpdateLocationAsync(dto, companyId);
                if (res)
                {
                    return Ok(new { Success = true, Data = "Location Update Successfully" });
                }
                return BadRequest(new { Success = false, Message = "Please Select Lotnumbers" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("update-box-group")]
        public async Task<IActionResult> UpdateBoxAndGroup([FromBody] SolitaireAdminBoxGroupUpdateDTO dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var res = await _service.UpdateBoxAndGroupAsync(dto, companyId);
                if (res)
                {
                    return Ok(new { Success = true, Data = "Box And Group Update Successfully" });
                }
                return BadRequest(new { Success = false, Message = "Please Select Lotnumbers" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
