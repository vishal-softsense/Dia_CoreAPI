using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Services.Interfaces.FinancialTransaction.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.FinancialTransaction.Payment
{
    [ApiController]
    [Route("api/PayWithExDiff")]
    [Authorize]
    public class PayWithExDiffController : ControllerBase
    {
        private readonly IPayWithExDiffService _service;

        public PayWithExDiffController(IPayWithExDiffService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? "0";

        /// <summary>MVC: GET /Payment/PaymentExDiffList → GetPaymentExDiffList</summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "select", CompanyId);
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

        /// <summary>MVC: Payment/PaymentExDiffView dropdown / rates</summary>
        [HttpGet("pagedata")]
        public async Task<IActionResult> GetPageData()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "insert", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "edit", CompanyId);
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

        /// <summary>MVC: POST /payment/get-all-inv (S+M+H CurCode filter)</summary>
        [HttpPost("pending-inv")]
        public async Task<IActionResult> GetPendingInvoices([FromBody] PayWithExDiffPendingInvRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "insert", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "edit", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetPendingInvoicesAsync(request, CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        /// <summary>MVC: GET /Payment/GetPayWithExDiff</summary>
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "edit", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetDetailAsync(id, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Payment record not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        /// <summary>MVC: POST /payments/ex-diff-payment</summary>
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] PayWithExDiffSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "insert", CompanyId);
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

        /// <summary>MVC: POST /data/get-PayWithExDiff-details</summary>
        [HttpPost("edit-details")]
        public async Task<IActionResult> GetEditDetails([FromBody] PayWithExDiffEditDetailsRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "edit", CompanyId);
                if (permission != 1)
                    permission = await _service.CheckUserPermissionAsync(UserTypeId, "select", CompanyId);
                if (permission != 1)
                    return Ok(new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetEditDetailsAsync(
                    request?.Gcode ?? "",
                    request?.AdjustmentType ?? "Payment",
                    CompanyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        /// <summary>MVC: POST /payments/edit-PayWithExDiff</summary>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] PayWithExDiffSaveRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "edit", CompanyId);
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

        /// <summary>MVC: POST /Payment/delete-ex-diff-Payment</summary>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] PayWithExDiffDeleteRequest request)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "delete", CompanyId);
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
    }
}
