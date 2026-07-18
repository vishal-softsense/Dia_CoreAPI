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
    public class MonthAnalysisReportController : ControllerBase
    {
        private readonly IMonthAnalysisReportService _service;

        public MonthAnalysisReportController(IMonthAnalysisReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] MonthAnalysisRequestDto filter)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetMonthAnalysisReportAsync(filter, companyId);
                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetDropdownDataAsync(companyId);

                var typeList = list.Where(x => x.Type == "Type").Select(x => new { id = x.ID, name = x.Name }).ToList();
                var subTypeList = list.Where(x => x.Type == "SubType").Select(x => new { id = x.ID, name = x.Name }).ToList();
                var categoryList = list.Where(x => x.Type == "Category").Select(x => new { id = x.ID, name = x.Name }).ToList();
                var currencyList = list.Where(x => x.Type == "Currency").Select(x => new { id = x.ID, name = x.Name }).ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        type = typeList,
                        subType = subTypeList,
                        category = categoryList,
                        currency = currencyList
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
