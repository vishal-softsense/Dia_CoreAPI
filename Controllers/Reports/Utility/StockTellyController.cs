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
    public class StockTellyController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IStockTellyService _service;

        public StockTellyController(IStockTellyService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? User.FindFirstValue("userid") ?? "0";
        private string UserName => User.FindFirstValue("UserName") ?? User.FindFirstValue("username") ?? UserId;

        /// <summary>Matches MVC GET data/stocktellydata — permission + dropdown data.</summary>
        [HttpGet("stocktellydata")]
        public async Task<IActionResult> StockTellyData()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "StockTelly", "select", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });

                var dropdowns = await _service.LoadPageDataAsync(CompanyId);
                return new JsonResult(new { success = true, mode = "Add", Dropdowns = dropdowns }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Stock Telly.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST data/stocktellyfilter</summary>
        [HttpPost("stocktellyfilter")]
        public async Task<IActionResult> StockTellyFilter([FromBody] StockTellyFilterDto filter)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "StockTelly", "select", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });

                var data = await _service.GetStockTellyReportAsync(filter, CompanyId);
                return new JsonResult(new { success = true, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST data/import-stelly</summary>
        [HttpPost("import-stelly")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportStockTelly([FromForm] StockTellyImportDto importData)
        {
            try
            {
                var result = await _service.ImportStockTellyAsync(importData?.ImportFile, UserTypeId, CompanyId);
                if (!result.Success && string.Equals(result.Data?.ToString(), "You are not authorized to perform this task.", StringComparison.Ordinal))
                    return StatusCode(403, new { success = false, Data = result.Data });

                return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST data/Save-StockTelly</summary>
        [HttpPost("Save-StockTelly")]
        public async Task<IActionResult> SaveStockTelly([FromBody] StockTellySaveRequestDto model)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "StockTelly", "insert", CompanyId);
                if (permission != 1)
                    return new JsonResult(new { success = false, message = "You are not authorized to perform this task." }, PascalCaseJson);

                if (!int.TryParse(CompanyId, out var companyId))
                    companyId = 0;

                if (!int.TryParse(UserId, out var userId))
                    userId = 0;

                var (success, message) = await _service.SaveStockTellyAsync(model, userId, UserName, companyId, CompanyId);
                return new JsonResult(new { success, message }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
