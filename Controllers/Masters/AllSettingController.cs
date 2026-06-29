using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Models.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dia_CoreAPI.Controllers.Masters
{
    [ApiController]
    [Route("master")]
    [Authorize]
    public class AllSettingController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public AllSettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private int? GetUserTypeId() => int.TryParse(HttpContext.User.FindFirst("UserTypeId")?.Value, out int ut) ? ut : (int?)null;
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";
        private string GetUserName() => HttpContext.User.FindFirst("Username")?.Value ?? "Unknown";

        [HttpGet("get-setting-data")]
        public async Task<IActionResult> GetSettingData()
        {
            try
            {
                var settings = await _settingService.GetSettingListAsync(GetCompanyId(), GetUserName());
                var data = settings.Select(row => new
                {
                    Id = row.Id,
                    Name = row.Name,
                    Value = row.Value,
                    Description = row.Description,
                    COMMENT = row.COMMENT
                }).ToList();

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpGet("filter-data")]
        public async Task<IActionResult> FilterData([FromQuery] string groupId)
        {
            try
            {
                var settings = await _settingService.FilterSettingListAsync(groupId, GetCompanyId(), GetUserName());
                var data = settings.Select(row => new
                {
                    Id = row.Id,
                    Name = row.Name,
                    Value = row.Value,
                    Description = row.Description,
                    COMMENT = row.COMMENT
                }).ToList();

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("submit-setting-data")]
        public async Task<IActionResult> SubmitSettingData([FromBody] CompanySettingMasterDto model)
        {
            if (!ModelState.IsValid)
            {
                string errorMsg = string.Join("\n\n", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                return BadRequest(new { success = false, data = errorMsg });
            }

            try
            {
                var (success, message) = await _settingService.SubmitSettingDataAsync(model, GetUserTypeId(), GetCompanyId());
                if (success)
                {
                    return Ok(new { success = true, data = message });
                }
                else
                {
                    return BadRequest(new { success = false, data = message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpGet("get-setting")]
        public async Task<IActionResult> GetSetting([FromQuery] int id, [FromQuery] string name)
        {
            try
            {
                var setting = await _settingService.GetSettingByIdAndNameAsync(id, name, GetCompanyId());
                if (setting != null)
                {
                    var data = new
                    {
                        Id = setting.Id,
                        Name = setting.Name,
                        Value = setting.Value,
                    };
                    return Ok(new { success = true, data = data });
                }
                else
                {
                    return NotFound(new { success = false, data = "Selected record not found." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("delete-setting")]
        public async Task<IActionResult> DeleteSetting([FromQuery] int id)
        {
            try
            {
                var (success, message) = await _settingService.DeleteSettingAsync(id, GetUserTypeId(), GetCompanyId());
                if (success)
                {
                    return Ok(new { success = true, data = message });
                }
                else
                {
                    return BadRequest(new { success = false, data = message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        //All print format
        [HttpGet("get-print-format-data")]
        public async Task<IActionResult> GetPrintFormatData()
        {
            try
            {
                var settings = await _settingService.FilterSettingListAsync("PRINT FORMAT", GetCompanyId(), GetUserName());
                var data = settings.Select(row => new
                {
                    Id = row.Id,
                    Name = row.Name,
                    Value = row.Value,
                    Description = row.Description,
                    COMMENT = row.COMMENT
                }).ToList();

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("~/print/update-print")]
        public async Task<IActionResult> UpdatePrint([FromBody] CompanySettingMasterDto model)
        {
            return await SubmitSettingData(model);
        }

        [HttpPost("~/print/delete-print-setting")]
        public async Task<IActionResult> DeletePrintSetting([FromQuery] int ID)
        {
            return await DeleteSetting(ID);
        }

        [HttpGet("~/print/get-Invoice-format-setting")]
        public async Task<IActionResult> GetInvoiceFormatSetting([FromQuery] int Id, [FromQuery] string Name)
        {
            try
            {
                var (setting, formats) = await _settingService.GetInvoiceFormatSettingAsync(Id, Name, GetCompanyId());
                if (setting != null)
                {
                    var data = new
                    {
                        Id = setting.Id,
                        Name = setting.Name,
                        Value = setting.Value
                    };

                    var selectListItems = formats.Select(x => new
                    {
                        Disabled = false,
                        Group = (object?)null,
                        Selected = false,
                        Text = x.FormateName,
                        Value = x.FormateName
                    }).ToList();

                    return Ok(new { success = true, data = data, invoiceFormat = selectListItems });
                }
                else
                {
                    return BadRequest(new { success = false, data = "Selected record not found." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("~/print/printPrev")]
        public async Task<IActionResult> PrintPreview([FromBody] CompanySettingMasterDto model)
        {
            try
            {
                var filePath = await _settingService.GetInvoicePreviewPathAsync(model.Name, model.Value, GetCompanyId());
                return Ok(new { success = true, path = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpGet("get-ColumnAbbreviations-data")]
        public async Task<IActionResult> GetColumnAbbreviationsData()
        {
            try
            {
                var list = await _settingService.GetColumnAbbrListAsync(GetCompanyId(), GetUserName());
                var data = list.Select(row => new
                {
                    Id = row.Id,
                    ColumnName = row.ColumnName,
                    Abbreviation = row.Abbreviation
                }).ToList();
                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("update-ColumnAbbr")]
        public async Task<IActionResult> UpdateColumnAbbr([FromBody] List<ColumnAbbrMaster> rows)
        {
            try
            {
                var (success, message) = await _settingService.UpdateColumnAbbrListAsync(rows, GetUserTypeId(), GetCompanyId());
                if (success)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest(new { success = false, data = message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, data = ex.Message });
            }
        }

        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadCompanyLoginLogo([FromForm] LogoUploadModel model)
        {
            try
            {
                var image = model?.Image;
                if (image == null || image.Length == 0)
                    return BadRequest(new { success = false, message = "No file selected" });

                string extension = System.IO.Path.GetExtension(image.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    return BadRequest(new { success = false, message = "Only JPG, JPEG, or PNG images allowed" });

                string companyId = GetCompanyId();
                string originalFileName = System.IO.Path.GetFileNameWithoutExtension(image.FileName);
                string cleanFileName = System.Text.RegularExpressions.Regex.Replace(originalFileName, @"[^a-zA-Z0-9_\-]", "");
                string uniqueFileName = $"{cleanFileName}_{companyId}_{DateTime.Now.Ticks}{extension}";
                string relativeFolder = "Upload";
                string webRootPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                string folderPath = System.IO.Path.Combine(webRootPath, relativeFolder);

                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);

                string fullPath = System.IO.Path.Combine(folderPath, uniqueFileName);
                using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                string imagePath = "/" + relativeFolder + "/" + uniqueFileName;

                return Ok(new
                {
                    success = true,
                    path = imagePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class LogoUploadModel
    {
        public Microsoft.AspNetCore.Http.IFormFile Image { get; set; }
    }
}
