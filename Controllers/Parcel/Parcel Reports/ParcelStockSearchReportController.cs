using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Report
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParcelStockSearchReportController : ControllerBase
    {
        private readonly IParcelStockSearchReportService _service;

        public ParcelStockSearchReportController(
            IParcelStockSearchReportService service)
        {
            _service = service;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        /// <summary>
        /// Load Parcel Stock Search Report Dropdown Data
        /// </summary>
        [HttpGet("load")]
        public async Task<IActionResult> Load()
        {
            try
            {
                var companyId = GetCompanyId();

                if (companyId != "0")
                {
                    var permission = await _service.CheckUserPermissionAsync(
                        GetUserTypeId(),
                        "ParcelStockSearch",
                        "select",
                        companyId);

                    if (permission != 1)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Message = "You are not authorized to perform this task."
                        });
                    }
                }

                var data = await _service.LoadReportDataAsync(companyId);

                return Ok(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();

                var list = await _service.GetStockSearchDropdownAsync(companyId);

                return Ok(new
                {
                    Success = true,
                    Data = list
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Parcel Stock Search Report
        /// </summary>
        [HttpPost("list")]
        public async Task<IActionResult> ParcelStockSearchReport(
            [FromBody] ParcelStockSearchReportFilterDto filter)
        {
            try
            {
                

                var result = await _service
                    .GetParcelStockSearchReportAsync(filter, GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("get-parcel-lots")]
        public async Task<IActionResult> GetParcelLots([FromQuery] string? searchby)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetParcelLotNoListAsync(searchby ?? "", companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("update-location")]
        public async Task<IActionResult> UpdateLocation([FromBody] ParcelStockLocationUpdateDTO dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var res = await _service.UpdateLocationAsync(dto, companyId);
                if (res)
                {
                    return Ok(new { Success = true, Data = "Location Update Successfully" });
                }
                return BadRequest(new { Success = false, Message = "Please Select Lotnumbers" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("update-box-group")]
        public async Task<IActionResult> UpdateBoxAndGroup([FromBody] ParcelStockBoxGroupUpdateDTO dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var res = await _service.UpdateBoxAndGroupAsync(dto, companyId);
                if (res)
                {
                    return Ok(new { Success = true, Data = "Box And Group Update Successfully" });
                }
                return BadRequest(new { Success = false, Message = "Please Select Lotnumbers" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}