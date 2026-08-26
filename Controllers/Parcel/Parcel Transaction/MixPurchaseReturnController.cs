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
    [Route("api/MixPurchaseReturn")]
    [Authorize]
    public class MixPurchaseReturnController : ControllerBase
    {
        private readonly IMixPurchaseReturnService _service;
        private const string FormName = "Mix Purchase Return";

        public MixPurchaseReturnController(IMixPurchaseReturnService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? "0";

        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetListAsync(CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("pagedata")]
        public async Task<IActionResult> GetPageData()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "edit", CompanyId);
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

        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "edit", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetDetailAsync(id, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Mix Purchase Return not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] MixPurchaseReturnSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var result = await _service.AddAsync(request, UserId, CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] MixPurchaseReturnSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "edit", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var result = await _service.EditAsync(request, UserId, CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] MixPurchaseReturnDeleteRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "delete", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var result = await _service.DeleteAsync(request.Id, UserId, CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("lot-detail")]
        public async Task<IActionResult> LotDetail([FromBody] MixPurchaseReturnLotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.LotNo))
                    return Ok(new { success = false, Data = "Lot No is required." });

                var data = await _service.GetLotDetailAsync(request.LotNo, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add-single-lot")]
        public async Task<IActionResult> AddSingleLot([FromBody] MixPurchaseReturnAddLotRequest request)
        {
            try
            {
                var data = await _service.AddItemInGridAsync(request, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("get-item-from-list")]
        public async Task<IActionResult> GetItemFromList([FromBody] MixPurchaseReturnGetItemFromListRequest request)
        {
            try
            {
                var data = await _service.GetItemFromListAsync(request, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("print-view/{invNo}")]
        public async Task<IActionResult> PrintView(string invNo)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetPrintViewAsync(invNo, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Print data not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("lots")]
        public async Task<IActionResult> GetLots([FromQuery] string? searchby)
        {
            try
            {
                var data = await _service.GetLotsAsync(searchby, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
