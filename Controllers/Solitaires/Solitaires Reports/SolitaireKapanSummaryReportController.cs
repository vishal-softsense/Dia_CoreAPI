using System;
using System.Text.Json;
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
    public class SolitaireKapanSummaryReportController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly ISolitaireKapanSummaryReportService _service;

        public SolitaireKapanSummaryReportController(ISolitaireKapanSummaryReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SolitaireKapanSummaryReportFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetReportAsync(filter, companyId);
                return new JsonResult(new { success = true, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = ex.Message });
            }
        }
    }
}
