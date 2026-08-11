using System;
using System.Linq;
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
    public class JewelleryPurchaseReportController : ControllerBase
    {
        private readonly IJewelleryPurchaseReportService _service;

        public JewelleryPurchaseReportController(IJewelleryPurchaseReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        [HttpPost("jewpurchase-report-list")]
        public async Task<IActionResult> Search([FromBody] JewelleryPurchaseReportFilterDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetReportAsync(filter, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Data = ex.Message });
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
                        type = helper("Type"),
                        subType = helper("SubType"),
                        category = helper("Category")
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
