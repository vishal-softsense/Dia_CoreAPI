using System;
using System.Linq;
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
    public class SolitaireGradingComparisonReportController : ControllerBase
    {
        private readonly ISolitarieGradingComparisonReportService _service;

        public SolitaireGradingComparisonReportController(ISolitarieGradingComparisonReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] GradingComparisonReportFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetReportAsync(filter, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = ex.Message });
            }
        }

        [HttpPost("make-lab-weight-diff-entry")]
        public async Task<IActionResult> MakeLabWeightDiffEntry([FromBody] MakeLabWeightDiffEntryRequestDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                var result = await _service.MakeLabWeightDiffEntryAsync(request, companyId);
                if (!result.Success)
                    return BadRequest(new { success = false, Message = result.Message });

                return Ok(new { success = true, Message = result.Message, InsertedCount = result.InsertedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetDropdownsAsync(companyId);

                var helper = new Func<string, object>(type =>
                    list.Where(x => x.Type == type).Select(x => new { id = x.ID, name = x.Name }).ToList()
                );

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        shape = helper("Shape"),
                        color = helper("Color"),
                        size = helper("Size"),
                        clarity = helper("Clarity"),
                        proportion = helper("Proportion"),
                        polish = helper("Polish"),
                        symmetry = helper("Symmetry"),
                        lab = helper("LAB"),
                        fluorescence = helper("Fluorescence")
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
