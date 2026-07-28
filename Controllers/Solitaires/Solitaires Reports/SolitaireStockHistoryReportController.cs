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
    public class SolitaireStockHistoryReportController : ControllerBase
    {
        private readonly ISolitaireStockHistoryReportService _service;

        public SolitaireStockHistoryReportController(ISolitaireStockHistoryReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SolitaireStockHistoryReportDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetStockHistoryReportAsync(filter, companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
