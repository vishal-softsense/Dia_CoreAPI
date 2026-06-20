using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;
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

        [HttpGet("get-setting-data")]
        public async Task<IActionResult> GetSettingData()
        {
            try
            {
                var settings = await _settingService.GetSettingListAsync();
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
                var settings = await _settingService.FilterSettingListAsync(groupId);
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
                var (success, message) = await _settingService.SubmitSettingDataAsync(model);
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
                var setting = await _settingService.GetSettingByIdAndNameAsync(id, name);
                if (setting != null)
                {
                    var data = new
                    {
                        Id = setting.Id,
                        Name = setting.Name,
                        Value = setting.Value,
                        //Description = setting.Description,
                        //COMMENT = setting.COMMENT
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
                var (success, message) = await _settingService.DeleteSettingAsync(id);
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
                var settings = await _settingService.FilterSettingListAsync("PRINT FORMAT");
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
                var (setting, formats) = await _settingService.GetInvoiceFormatSettingAsync(Id, Name);
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
                var filePath = await _settingService.GetInvoicePreviewPathAsync(model.Name, model.Value);
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
                var list = await _settingService.GetColumnAbbrListAsync();
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
                var (success, message) = await _settingService.UpdateColumnAbbrListAsync(rows);
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
    }
}
