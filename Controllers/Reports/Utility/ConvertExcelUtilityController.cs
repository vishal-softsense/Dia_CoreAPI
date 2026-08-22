using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Reports.Utility
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConvertExcelUtilityController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IConvertExcelUtilityService _service;

        public ConvertExcelUtilityController(IConvertExcelUtilityService service)
        {
            _service = service;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? User.FindFirstValue("usertypeid") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? User.FindFirstValue("CompanyId") ?? User.FindFirstValue("companyid") ?? string.Empty;

        /// <summary>Matches MVC GET dashbord/uploadexcel — permission gate for page open (select).</summary>
        [HttpGet("uploadexcel")]
        public async Task<IActionResult> GetUploadExcelPage()
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "ConvertExcelUtility", "select", CompanyId);
                if (permission == 1)
                    return new JsonResult(new { success = true, mode = "Add" }, PascalCaseJson);

                return StatusCode(403, new { success = false, message = "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Convert Excel Utility.", error = ex.Message });
            }
        }

        /// <summary>Matches MVC POST dashboard/Upload</summary>
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadBothTables(IFormFile file)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(UserTypeId, "ConvertExcelUtility", "select", CompanyId);
                if (permission != 1)
                    return StatusCode(403, new { success = false, error = "You are not authorized to perform this task." });

                var result = await _service.ProcessUploadBothTablesAsync(file, CompanyId);
                if (!result.Success)
                    return BadRequest(new { error = result.Error, message = result.Message });

                return new JsonResult(new
                {
                    originalData = result.OriginalData,
                    mappedData = result.MappedData,
                    file = result.File,
                    fileName = result.FileName
                }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Upload failed.", message = ex.Message });
            }
        }
    }
}
