using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SyncSettingController : ControllerBase
    {
        private readonly ISyncSettingService _syncSettingService;

        public SyncSettingController(ISyncSettingService syncSettingService)
        {
            _syncSettingService = syncSettingService;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        [HttpGet("list-sync")]
        public async Task<IActionResult> GetSync(
            [FromQuery] int companyId)
        {
            try
            {

                int permission =
                await _syncSettingService.CheckUserPermissionAsync(GetUserTypeId(), "SyncMaster", "select", GetCompanyId());


                if (permission != 1)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "You are not authorized to perform this task."
                    });
                }

                var result = await _syncSettingService.GetSyncAsync(companyId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("save-sync")]
        public async Task<IActionResult> SaveSync([FromBody] ProviderSettings model, [FromQuery] int enable)
        {
            try
            {
                // Permission Check
                int permission = await _syncSettingService.CheckUserPermissionAsync(GetUserTypeId(), "SyncMaster", "insert", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "You are not authorized to perform this task."
                    });
                }

                var result = await _syncSettingService.SaveSyncAsync(
                    model,
                    GetCompanyId(),
                    GetCompanyName(),
                    GetConnectionString());

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        #region Helper Methods



        private string GetCompanyName()
        {
            return User.FindFirst("CompanyName")?.Value ?? "";
        }

        private string GetConnectionString()
        {
            return User.FindFirst("SqlConnection")?.Value ?? "";
        }



        #endregion
    }

    //[HttpPost("save-sync")]
    //public async Task<IActionResult> SaveSync(
    //    [FromBody] ProviderSettings model,
    //    [FromQuery] int companyId,
    //    [FromQuery] string companyName,
    //    [FromQuery] string sqlConnection,
    //    [FromQuery] int enable)
    //{
    //    try
    //    {
    //        var permission = await _syncSettingService.CheckUserPermissionAsync(GetUserTypeId(), "SyncMaster", "insert", GetCompanyId());

    //        if (permission != 1)
    //        {
    //            return Unauthorized(new
    //            {
    //                Success = false,
    //                Message = "You are not authorized to perform this task."
    //            });
    //        }

    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(new ApiResponse
    //            {
    //                Success = false,
    //                Message = "Invalid Request"
    //            });
    //        }



    //        var result = await _syncSettingService.SaveSyncAsync(
    //            model,
    //            companyId,
    //            companyName,
    //            sqlConnection,enable);

    //        if (!result.Success)
    //            return BadRequest(result);

    //        return Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new ApiResponse
    //        {
    //            Success = false,
    //            Message = ex.Message
    //        });
    //    }
    //}
}
