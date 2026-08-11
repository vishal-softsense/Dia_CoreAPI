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
    public class JewellerySaleReportController : ControllerBase
    {
        private readonly IJewellerySaleReportService _service;

        public JewellerySaleReportController(IJewellerySaleReportService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpPost("search")]
        [HttpPost("jewsale-report-list")]
        public async Task<IActionResult> Search([FromBody] JewellerySaleReportFilterDto filter)
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
                var dropdowns = await _service.GetDropdownsAsync(companyId);
                var list = dropdowns.PurchaseDropdowns?.ToList() ?? new();

                var helper = new Func<string, object>(type =>
                    list.Where(x => x.Type == type)
                        .Select(x => new { id = x.ID, name = x.Name })
                        .ToList());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        type = helper("Type"),
                        subType = helper("SubType"),
                        category = helper("Category"),
                        design = (dropdowns.DesignList ?? Enumerable.Empty<Dia_CoreAPI.DTOs.Masters.JewDesignMaster>())
                            .Select(x => new { id = x.DesignID, name = x.DesignID })
                            .ToList(),
                        subCategory = (dropdowns.SubCategoryList ?? Enumerable.Empty<Dia_CoreAPI.DTOs.Masters.JewSubCategoryMaster>())
                            .Select(x => new { id = x.SubCategoryID, name = x.SubCategoryID })
                            .ToList(),
                        metal = (dropdowns.MetalList ?? Enumerable.Empty<Dia_CoreAPI.DTOs.Masters.JewMetalMaster>())
                            .Select(x => new { id = x.MetalID, name = x.MetalID })
                            .ToList(),
                        color = (dropdowns.ColorList ?? Enumerable.Empty<Dia_CoreAPI.DTOs.Masters.JewColorMaster>())
                            .Select(x => new { id = x.JewColorID, name = x.JewColorID })
                            .ToList(),
                        certificate = new[]
                        {
                            new { id = "IGI", name = "IGI" },
                            new { id = "GIA", name = "GIA" }
                        }
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
