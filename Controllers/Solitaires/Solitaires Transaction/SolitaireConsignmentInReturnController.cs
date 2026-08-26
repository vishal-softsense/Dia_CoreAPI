using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Solitaires.Solitaires_Transaction
{
    [ApiController]
    [Route("api/SolitaireConsignmentInReturn")]
    [Authorize]
    public class SolitaireConsignmentInReturnController : ControllerBase
    {
        private const string FormName = "Consignment Return";
        private readonly ISolitaireConsignmentInReturnService _service;

        public SolitaireConsignmentInReturnController(ISolitaireConsignmentInReturnService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        [HttpGet("pagedata")]
        public async Task<IActionResult> GetPageData()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetPageDataAsync(CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] SolitaireConsignmentInReturnFilterRequest filter)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetListAsync(filter ?? new(), CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnSelected([FromBody] SolitaireConsignmentInReturnSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var result = await _service.ReturnAsync(request ?? new(), CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
