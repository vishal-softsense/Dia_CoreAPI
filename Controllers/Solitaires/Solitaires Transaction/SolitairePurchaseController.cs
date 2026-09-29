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
    [Route("api/SolitairePurchase")]
    [Authorize]
    public class SolitairePurchaseController : ControllerBase
    {
        private readonly ISolitairePurchaseService _service;

        public SolitairePurchaseController(ISolitairePurchaseService service)
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "select", CompanyId);
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "Purchase", "insert", CompanyId);
                if (permission != 1)
                {
                    // allow edit users to load dropdowns too
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "edit", CompanyId);
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
        public async Task<IActionResult> GetDetail(int id, [FromQuery] string? mode = null)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "edit", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetDetailAsync(id, CompanyId, mode);
                if (data == null)
                    return Ok(new { success = false, Data = "Purchase not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] SolitairePurchaseSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "Purchase", "insert", CompanyId);
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
        public async Task<IActionResult> Edit([FromBody] SolitairePurchaseSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "edit", CompanyId);
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
        public async Task<IActionResult> Delete([FromBody] SolitairePurchaseDeleteRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "delete", CompanyId);
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
        public async Task<IActionResult> LotDetail([FromBody] SolitairePurchaseLotRequest request)
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

        [HttpPost("consignment")]
        public async Task<IActionResult> Consignment([FromBody] SolitaireConsignmentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.PartyCode) && string.IsNullOrWhiteSpace(request?.MemoNo))
                    return Ok(new { success = false, Data = "No consignment Found" });

                var data = await _service.GetConsignmentAsync(request.PartyCode, request.MemoNo, CompanyId);
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
        public async Task<IActionResult> AddSingleLot([FromBody] SolitairePurchaseAddLotRequest request)
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "select", CompanyId);
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
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "purchase", "select", CompanyId);
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

        /// <summary>MVC MasterProcessor.GetRapRate — fills MRP when Excel MRP is 0/blank.</summary>
        [HttpPost("get-rap-rate")]
        public async Task<IActionResult> GetRapRate([FromBody] SolitairePurchaseRapRateBatchRequest? batch)
        {
            try
            {
                var items = batch?.Items;
                if (items == null || items.Count == 0)
                    return Ok(new { success = false, Data = "Shape/Color/Clarity/Carat required." });

                var results = new System.Collections.Generic.List<SolitairePurchaseRapRateResponse>();
                foreach (var item in items)
                {
                    var mrp = await _service.GetRapRateAsync(
                        item?.Shape ?? "",
                        item?.Color ?? "",
                        item?.Clarity ?? "",
                        item?.Carat ?? 0,
                        CompanyId);
                    results.Add(new SolitairePurchaseRapRateResponse
                    {
                        Shape = item?.Shape,
                        Color = item?.Color,
                        Clarity = item?.Clarity,
                        Carat = item?.Carat ?? 0,
                        MRP = mrp
                    });
                }

                return Ok(new { success = true, Data = results });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }
    }
}
