using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Parcel.Parcel_Transaction
{
    [ApiController]
    [Route("api/MixConsignmentReturn")]
    [Authorize]
    public class MixConsignmentReturnController : ControllerBase
    {
        // MVC: CheckUserPermission(..., "MixConsignment Return", "insert")
        private const string FormName = "MixConsignment Return";
        private readonly IMixConsignmentReturnService _service;

        public MixConsignmentReturnController(IMixConsignmentReturnService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? "0";

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
        public async Task<IActionResult> GetList([FromBody] MixConsignmentReturnFilterRequest filter)
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
        public async Task<IActionResult> ReturnSelected([FromBody] MixConsignmentReturnSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var result = await _service.ReturnAsync(request ?? new(), UserId, CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
