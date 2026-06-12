using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Masters
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AllMasterController : ControllerBase
    {
        private readonly IAllMasterService _masterService;

        public AllMasterController(IAllMasterService masterService)
        {
            _masterService = masterService;
        }

        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";

        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        #region All Master
        [HttpGet("all-master-list")]
        public async Task<IActionResult> GetAllMasterList()
        {
            try
            {
                var result =
                    await _masterService.GetAllMasterListAsync(GetUserTypeId(), GetCompanyId());
                
                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }
        #endregion

        #region Shape Master

        #endregion
    }
}