using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Utility
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GetLabDataController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IGetLabDataService _service;

        public GetLabDataController(IGetLabDataService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;

        /// <summary>Matches MVC GET data/getlabdata — permission gate for page open (select).</summary>
        [HttpGet("getlabdata")]
        public async Task<IActionResult> GetLabDataPage()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "Get Lab Data", "select", CompanyId);
                if (permission == 1)
                    return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);

                return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Get Lab Data.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST getlab/get-item-from-list</summary>
        [HttpPost("get-item-from-list")]
        public async Task<IActionResult> GetItemFromList([FromBody] GetLabLotRequestDto request)
        {
            try
            {
                var result = await _service.GetItemsFromListAsync(request, UserTypeId, CompanyId);
                if (!result.Success && string.Equals(result.Data?.ToString(), "You are not authorized to perform this task.", StringComparison.Ordinal))
                    return StatusCode(403, new { success = false, Data = result.Data });

                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST data/import-cert</summary>
        [HttpPost("import-cert")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportCert([FromForm] GetLabDataDTO importData)
        {
            try
            {
                var result = await _service.ImportCertAsync(importData, UserTypeId, CompanyId);
                if (!result.Success && string.Equals(result.Data?.ToString(), "You are not authorized to perform this task.", StringComparison.Ordinal))
                    return StatusCode(403, new { success = false, Data = result.Data });

                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC common/get-gia-data</summary>
        [HttpPost("get-gia-data")]
        public async Task<IActionResult> GetGiaData([FromBody] GetCertRequestDto request)
        {
            try
            {
                var result = await _service.GetGiaDataAsync(request, CompanyId);
                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC common/get-igi-data</summary>
        [HttpPost("get-igi-data")]
        public async Task<IActionResult> GetIgiData([FromBody] GetCertRequestDto request)
        {
            try
            {
                var result = await _service.GetIgiDataAsync(request, CompanyId);
                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST excel/getlabdataprint</summary>
        [HttpPost("getlabdataprint")]
        public async Task<IActionResult> GetLabDataPrint([FromBody] GetLabDataExcelRequestDto request)
        {
            try
            {
                var result = await _service.PrintExcelAsync(request, CompanyId);
                return new JsonResult(new
                {
                    error = result.Error,
                    success = result.Success,
                    message = result.Message,
                    Headers = result.Headers,
                    Rows = result.Rows
                }, PascalCaseJson);
            }
            catch (Exception)
            {
                return new JsonResult(new { error = true, message = "Internal Error. Please Try Again." }, PascalCaseJson);
            }
        }
    }
}
