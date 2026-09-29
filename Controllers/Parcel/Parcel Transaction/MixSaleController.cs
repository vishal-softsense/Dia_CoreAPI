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
    [Route("api/MixSale")]
    [Authorize]
    public class MixSaleController : ControllerBase
    {
        private readonly IMixSaleService _service;

        public MixSaleController(IMixSaleService service)
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "select", CompanyId);
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "insert", CompanyId);
                if (permission != 1)
                {
                    // allow edit users to load dropdowns too
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "edit", CompanyId);
                }
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "edit", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetDetailAsync(id, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Mix Sale not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] MixSaleSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "insert", CompanyId);
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
        public async Task<IActionResult> Edit([FromBody] MixSaleSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "edit", CompanyId);
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
        public async Task<IActionResult> Delete([FromBody] MixSaleDeleteRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "delete", CompanyId);
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
        public async Task<IActionResult> LotDetail([FromBody] MixSaleLotRequest request)
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

        /// <summary>MVC: POST Mix-Pur/consignment-to-MixSale (PartyCode + MemoNo).</summary>
        [HttpPost("consignment")]
        [HttpPost("consignment-to-MixSale")]
        public async Task<IActionResult> Consignment([FromBody] MixConsignmentRequest? request)
        {
            try
            {
                var partyCode = request?.PartyCode?.Trim() ?? "";
                var memoNo = request?.MemoNo?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(partyCode) && string.IsNullOrWhiteSpace(memoNo))
                    return Ok(new { success = false, Data = "No consignment Found" });

                if (string.IsNullOrWhiteSpace(CompanyId))
                    return Ok(new { success = false, Data = "Company not found. Please login again." });

                var data = await _service.GetConsignmentAsync(partyCode, memoNo, CompanyId);
                if (data == null || data.Count == 0)
                    return Ok(new { success = false, Data = "No consignment Found" });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add-single-lot")]
        public async Task<IActionResult> AddSingleLot([FromBody] MixSaleAddLotRequest request)
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

        [HttpGet("print-view/{invNo}")]
        public async Task<IActionResult> PrintView(string invNo)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetPrintViewAsync(invNo, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Invoice not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("current-status/{invNo}")]
        public async Task<IActionResult> CurrentStatus(string invNo)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "MixForm", "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetCurrentStatusAsync(invNo, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Invoice not found." });

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
                if (string.IsNullOrWhiteSpace(CompanyId))
                    return Ok(new { success = false, Data = "Company not found in token." });

                var data = await _service.GetLotsAsync(searchby, CompanyId);
                return Ok(new { success = true, Data = data ?? new List<object>() });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
