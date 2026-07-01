using Dia_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace Dia_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/common")]
    [Authorize]
    public class MenuSearchController : ControllerBase
    {
        private readonly IMenuSearchService _service;

        public MenuSearchController(IMenuSearchService service)
        {
            _service = service;
        }
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";
        [HttpGet("get-menu-list")]
        public async Task<IActionResult> GetMenuList(string searchby)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchby))
                    return Ok(new List<object>());

                string where = $" AND (Form_MnuName LIKE '%{searchby}%') AND (Form_Title = 'Form' OR Form_Title = 'Header')";

                var data = await _service.GetMenuForDropdownAsync(where, GetCompanyId());

                var result = data
                    .Where(x =>
                        (x.Parent == "Header" &&
                        (x.FormName == "DashBoard" || x.FormName == "DiaAnalytics"))
                        || x.Parent == "Form")
                    .Select(x => new
                    {
                        id = x.Path,
                        text = x.FormName,
                        parent = x.Parent,
                        group = x.Group
                    });
                return Ok(data.Select(x => new
                {
                    id = x.Path,
                    text = x.FormName,
                    parent = x.Parent,
                    group = x.Group
                }));
                //return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}