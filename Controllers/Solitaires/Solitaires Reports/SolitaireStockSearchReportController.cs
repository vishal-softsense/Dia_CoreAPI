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
    public class SolitaireStockSearchReportController : ControllerBase
    {
        private readonly ISolitaireStockSearchReportService _service;

        public SolitaireStockSearchReportController(ISolitaireStockSearchReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> GetStockSearchReport([FromBody] SolitaireStockSearchReportDTO filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetStockSearchReportAsync(filter, companyId);
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
                var list = await _service.GetStockSearchDropdownAsync(companyId);
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
                var data = await _service.GetSolitaireLotNoListAsync(searchby ?? "", companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("update-location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDTO dto)
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
        public async Task<IActionResult> UpdateBoxAndGroup([FromBody] BoxGroupUpdateDTO dto)
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

        [HttpGet("email-settings")]
        public async Task<IActionResult> GetEmailSettings()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetEmailSettingsAsync(companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("email-stock-list")]
        public async Task<IActionResult> EmailStockList([FromBody] SendSolitaireStockEmailDTO dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var res = await _service.SendStockListEmailAsync(dto, companyId);
                if (res)
                {
                    return Ok(new { Success = true, Data = "Mail Sent Successfully." });
                }
                return BadRequest(new { Success = false, Message = "Failed to send email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
