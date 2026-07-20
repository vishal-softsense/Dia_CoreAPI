using System;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SaleAnalysisReportController : ControllerBase
    {
        private readonly ISaleAnalysisReportService _service;

        public SaleAnalysisReportController(ISaleAnalysisReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value
                ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        /// <summary>
        /// Report data — mirrors MVC /Group-by/sale-data
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] SaleAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });
                if (string.IsNullOrWhiteSpace(filter.SaleAna))
                    return BadRequest(new { success = false, message = "SaleAna grouping is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetSaleAnalysisReportAsync(filter, companyId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Pivot data — mirrors MVC /Group-by/sale-pivotdata
        /// </summary>
        [HttpPost("pivotdata")]
        public async Task<IActionResult> GetPivotData([FromBody] SaleAnalysisRequestDto filter)
        {
            try
            {
                if (filter == null)
                    return BadRequest(new { success = false, message = "Filter is required." });

                var companyId = GetCompanyId();
                var data = await _service.GetSaleAnalysisPivotDataAsync(filter, companyId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dropdown lists — same SP as Purchase/Parcel analysis (AllMasterDataForPurchaseReport)
        /// </summary>
        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetDropdownDataAsync(companyId);

                var helper = new Func<string, object>(type =>
                    list.Where(x => x.Type == type).Select(x => new { id = x.ID, name = x.Name }).ToList()
                );

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        currency    = helper("Currency"),
                        type        = helper("Type"),
                        subType     = helper("SubType"),
                        category    = helper("Category"),
                        shape       = helper("Shape"),
                        color       = helper("Color"),
                        size        = helper("Size"),
                        clarity     = helper("Clarity"),
                        lab         = helper("LAB"),
                        proportion  = helper("Proportion"),
                        polish      = helper("Polish"),
                        symmetry    = helper("Symmetry"),
                        fluorescence= helper("Fluorescence"),
                        milky       = helper("Milky"),
                        luster      = helper("Luster"),
                        natts       = helper("Natts"),
                        mines       = helper("Mines"),
                        productType = helper("ProductType"),
                        sieve       = helper("Sieve")
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
