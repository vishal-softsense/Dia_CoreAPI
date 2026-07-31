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
    public class SolitaireSalePerformanceReportController : ControllerBase
    {
        private readonly ISolitaireSalePerformanceReportService _service;

        public SolitaireSalePerformanceReportController(ISolitaireSalePerformanceReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SolitaireSalePerformanceReportFilterDto filter)
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

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetDropdownsAsync(companyId);

                var helper = new Func<string, object>(type =>
                    list.Where(x => x.Type == type)
                        .Select(x => new { id = x.ID, name = x.Name })
                        .ToList());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        currency = helper("Currency"),
                        account = helper("Account"),
                        shape = helper("Shape"),
                        color = helper("Color"),
                        size = helper("Size"),
                        clarity = helper("Clarity"),
                        proportion = helper("Proportion"),
                        polish = helper("Polish"),
                        symmetry = helper("Symmetry"),
                        lab = helper("LAB"),
                        fluorescence = helper("Fluorescence"),
                        milky = helper("Milky"),
                        luster = helper("Luster"),
                        natts = helper("Natts"),
                        mines = helper("Mines"),
                        productType = helper("ProductType"),
                        sieve = helper("Sieve")
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
