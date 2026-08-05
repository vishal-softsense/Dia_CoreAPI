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
    public class SolitaireConsumptionReportController : ControllerBase
    {
        private readonly ISolitaireConsumptionReportService _service;

        public SolitaireConsumptionReportController(ISolitaireConsumptionReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value
                ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        /// <summary>
        /// POST api/SolitaireConsumptionReport/search
        /// Ported from MVC POST report/stock-consumption-report-List
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SolitaireConsumptionReportFilterDto filter)
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

        /// <summary>
        /// GET api/SolitaireConsumptionReport/dropdowns
        /// Ported from MVC GET — AdminStockSearchDropDown / AllMasterDataForAdminStockSearch
        /// </summary>
        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetDropdownsAsync(companyId);

                var helper = new Func<string, object>(type =>
                    list.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                        .Select(x => new { id = x.ID, name = string.IsNullOrWhiteSpace(x.Name) ? x.ID : x.Name })
                        .ToList());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        shape = helper("Shape"),
                        color = helper("Color"),
                        clarity = helper("Clarity"),
                        size = helper("Size"),
                        proportion = helper("Proportion"),
                        polish = helper("Polish"),
                        symmetry = helper("Symmetry")
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
