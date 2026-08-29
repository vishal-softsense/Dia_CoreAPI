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
    [Route("api/MixSaleReturn")]
    [Authorize]
    public class MixSaleReturnController : ControllerBase
    {
        private readonly IMixSaleReturnService _service;
        private const string FormName = "Mix Sale Return Partial";

        public MixSaleReturnController(IMixSaleReturnService service)
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
                    return Ok(new { success = false, Data = "Mix Sale Return not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] MixSaleReturnSaveRequest request)
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
        public async Task<IActionResult> Edit([FromBody] MixSaleReturnSaveRequest request)
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
        public async Task<IActionResult> Delete([FromBody] MixSaleReturnDeleteRequest request)
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
        public async Task<IActionResult> LotDetail([FromBody] MixSaleReturnLotRequest request)
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
        public async Task<IActionResult> AddSingleLot([FromBody] MixSaleReturnAddLotRequest request)
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
        public async Task<IActionResult> GetItemFromList([FromBody] MixSaleReturnGetItemFromListRequest request)
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

        /// <summary>MVC: /MixSaleReturn/csale-to-csalertn</summary>
        [HttpPost("csale-to-csalertn")]
        public async Task<IActionResult> CSaleToCSaleRtn([FromBody] MixSaleReturnCSaleLookupRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.PartyCode) && string.IsNullOrWhiteSpace(request?.MemoNo))
                    return Ok(new { success = false, Data = "No invoice Found" });

                var data = await _service.GetCSaleForCSaleRtnAsync(request?.PartyCode, request?.MemoNo, CompanyId);
                if (data == null || data.Count == 0)
                    return Ok(new { success = false, Data = "No invoice Found" });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        /// <summary>MVC: /MixSaleReturn/csale-to-csalertn-fill</summary>
        [HttpPost("csale-to-csalertn-fill")]
        public async Task<IActionResult> CSaleToCSaleRtnFill([FromBody] MixSaleReturnCSaleFillRequest request)
        {
            try
            {
                var data = await _service.FillFromCSaleAsync(
                    request?.Lines ?? new(),
                    request?.Items ?? new(),
                    CompanyId);

                if (!string.IsNullOrWhiteSpace(data?.Alert) &&
                    data.Alert.Contains("invalid data", StringComparison.OrdinalIgnoreCase))
                    return Ok(new { success = false, Data = data.Alert });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
