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
    public class JewelleryMemoOutReportController : ControllerBase
    {
        private readonly IJewelleryMemoOutReportService _service;

        public JewelleryMemoOutReportController(IJewelleryMemoOutReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        // MVC: POST jewreport/jewmemo-report-list
        [HttpPost("search")]
        [HttpPost("jewmemo-report-list")]
        public async Task<IActionResult> Search([FromBody] JewelleryMemoOutReportFilterDto filter)
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
                var list = (await _service.GetDropdownsAsync(companyId)).ToList();

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
                        type = helper("Type")
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
