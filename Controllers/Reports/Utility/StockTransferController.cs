using System;
using System.Linq;
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
    public class StockTransferController : ControllerBase
    {
        private const string FormName = "Stock Transfer";

        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IStockTransferService _service;

        public StockTransferController(IStockTransferService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue("UserId") ?? User.FindFirstValue("userid") ?? "0";
        private string UserName => User.FindFirstValue("UserName") ?? User.FindFirstValue("username") ?? UserId;

        /// <summary>Matches MVC GET Stock/Stock-transfer-list — select permission + list.</summary>
        [HttpGet("Stock-transfer-list")]
        public async Task<IActionResult> StockTransferList()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "select", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });

                var data = await _service.GetListAsync(CompanyId);
                return new JsonResult(new { success = true, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Stock Transfer list.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC GET Stock/Stock-transfer — insert permission for Add page.</summary>
        [HttpGet("Stock-transfer")]
        public async Task<IActionResult> StockTransferAddPage()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });

                return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Add Stock Transfer.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST Stock/Save-Stock-transfer.</summary>
        [HttpPost("Save-Stock-transfer")]
        public async Task<IActionResult> SaveStockTransfer([FromBody] StockTransferSaveDto model)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "insert", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, Data = "You are not authorized to perform this task." });

                if (!int.TryParse(CompanyId, out var companyId))
                    companyId = 0;
                if (!int.TryParse(UserId, out var userId))
                    userId = 0;

                model ??= new StockTransferSaveDto();
                model.CompanyID = companyId;

                var (success, data) = await _service.SaveAsync(model, userId, UserName, companyId, CompanyId);
                return new JsonResult(new { success, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC POST stock/Stock-transfer-delete?ID=</summary>
        [HttpPost("Stock-transfer-delete")]
        public async Task<IActionResult> DeleteStockTransfer([FromQuery] int ID)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, FormName, "delete", CompanyId);
                if (permission != 1)
                    return new JsonResult(new { success = false, Data = "You are not authorized to perform this task." }, PascalCaseJson);

                var (success, data) = await _service.DeleteAsync(ID, CompanyId);
                return new JsonResult(new { success, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC GET stock/Stock-transfer-stonedetails?LotNo=</summary>
        [HttpGet("Stock-transfer-stonedetails")]
        public async Task<IActionResult> GetLotDetail([FromQuery] string? LotNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LotNo))
                    return new JsonResult(null, PascalCaseJson);

                var data = await _service.GetLotDetailsAsync(LotNo, CompanyId);
                return new JsonResult(new { success = true, Data = data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        /// <summary>Matches MVC GET common/get-alllotno-list?searchby=</summary>
        [HttpGet("get-alllotno-list")]
        public async Task<IActionResult> GetAllLotNoList([FromQuery] string? searchby)
        {
            try
            {
                var data = await _service.GetAllLotNoListAsync(searchby ?? "", CompanyId);
                var modifiedData = data
                    .Select(x => new
                    {
                        id = !string.IsNullOrWhiteSpace(x.Name) ? x.Name : x.ID,
                        text = !string.IsNullOrWhiteSpace(x.TextName)
                            ? x.TextName
                            : (!string.IsNullOrWhiteSpace(x.Name) ? x.Name : x.ID)
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.id))
                    .ToList();
                return new JsonResult(modifiedData, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
