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
    public class SolitaireHoldUnholdReportController : ControllerBase
    {
        private readonly ISolitaireHoldUnholdReportService _service;

        public SolitaireHoldUnholdReportController(ISolitaireHoldUnholdReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value
                ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SolitaireHoldUnholdReportFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.SearchAsync(filter, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = ex.Message });
            }
        }

        [HttpPost("hold-restore")]
        public async Task<IActionResult> HoldRestore([FromBody] SolitaireHoldUnholdActionDto action)
        {
            try
            {
                var companyId = GetCompanyId();

                if (action == null || string.IsNullOrWhiteSpace(action.LotNo))
                    return Ok(new { success = false, Data = "Please Select Stone" });

                if (string.IsNullOrWhiteSpace(action.Type)
                    || (action.Type.Trim().ToLowerInvariant() != "hold"
                        && action.Type.Trim().ToLowerInvariant() != "unhold"))
                {
                    return Ok(new { success = false, Data = "Invalid Type. Use hold or unhold." });
                }

                var ok = await _service.HoldRestoreAsync(action, companyId);
                if (ok)
                    return Ok(new { success = true, Data = "" });

                return Ok(new { success = false, Data = "Hold/Restore failed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = ex.Message, Data = ex.Message });
            }
        }
    }
}
