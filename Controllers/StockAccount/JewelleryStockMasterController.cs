using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.StockAccount
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JewelleryStockMasterController : ControllerBase
    {
        private readonly IJewStockMasterService _service;

        public JewelleryStockMasterController(IJewStockMasterService service)
        {
            _service = service;
        }

        private string GetUserId() => User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => User.FindFirst("UserTypeId")?.Value ?? "0";

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value
                ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        // Mounting-specific endpoints

        [HttpGet("jewelleryM-stock-list")]
        public async Task<IActionResult> GetMountingStock()
        {
            int permission =
                await _service.CheckUserPermissionAsync(GetUserTypeId(), "Jew Stock Master", "select", GetCompanyId());

            if (permission != 1)
            {
                return Ok(new
                {
                    Success = false,
                    Data = "You are not authorized to perform this task."
                });
            }

            return Ok(new
            {
                Success = true
            });
        }

        [HttpGet("mounting-stock")]
        public async Task<IActionResult> GetMountingStockList()
        {
            try
            {
                var companyId = GetCompanyId();

                int permission = await _service.CheckUserPermissionAsync(
                    GetUserTypeId(), "Jew Stock Master", "select", companyId);

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "You are not authorized to perform this task."
                    });
                }

                var result = await _service.GetMountingStockAsync(companyId, "");

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

        [HttpGet("list-jew-metal")]
        public async Task<IActionResult> GetJewMetalMaster()
        {
            try
            {
                var result = await _service.GetJewMetalMasterAsync(GetCompanyId(),"");

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

        [HttpGet("list-jew-color")]
        public async Task<IActionResult> GetJewColorMaster()
        {
            try
            {
                var result = await _service.GetJewColorMasterAsync(GetCompanyId(), "");

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

        [HttpGet("list-jew-purity")]
        public async Task<IActionResult> GetJewPurity(string metalType)
        {
            try
            {
                var result = await _service.GetJewPurityAsync(GetCompanyId(),metalType);

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

        [HttpGet("get-pure-ratio")]
        public async Task<IActionResult> GetPureRatio(string metalType, string metalPurity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(metalType) ||
                    string.IsNullOrWhiteSpace(metalPurity))
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "MetalType and MetalPurity are required."
                    });
                }

                var ratio = await _service.GetPureRatioAsync(
                    GetCompanyId(),
                    metalType,
                    metalPurity);

                return Ok(new
                {
                    Success = true,
                    Data = ratio
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

        [HttpGet("list-jew-category")]
        public async Task<IActionResult> GetJewCategory()
        {
            try
            {
                var result = await _service.GetJewCategoryMasterAsync(GetCompanyId(),"");

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

        [HttpGet("list-jew-subcategory")]
        public async Task<IActionResult> GetJewSubCategory()
        {
            try
            {
                var result = await _service.GetJewSubCategoryMasterAsync(GetCompanyId(),"");

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

        [HttpGet("jew-setting-type")]
        public async Task<IActionResult> GetJewSettingType()
        {
            try
            {
                var result = await _service.GetJewSettingValueAsync(GetCompanyId());

                // Filter to Making Type values only — same as MVC:
                // SettingMaster.Where(a => a.Name == "Jew Setting Type").Select(x => x.Value).Distinct()
                var makingTypes = result
                    .Where(x => x.Name == "Jew Setting Type")
                    .Select(x => x.Value)
                    .Distinct()
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    Data = makingTypes   // returns List<string> e.g. ["Per Gm","Per Pcs","Lumsum","Per%"]
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

        [HttpGet("add-stock-mounting")]
        public async Task<IActionResult> GetAddMountingStockData()
        {
            try
            {
                int permission =
                    await _service.CheckUserPermissionAsync(GetUserTypeId(), "Jew Stock Master", "insert", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "You are not authorized to perform this task."
                    });
                }

                var result = await _service.GetAddMountingStockDataAsync(GetCompanyId());

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

        [HttpGet("edit-stock-mounting/{id}")]
        public async Task<IActionResult> GetEditMountingStockData(int id)
        {
            try
            {
                int permission =
                    await _service.CheckUserPermissionAsync(GetUserTypeId(), "Jew Stock Master", "edit", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "You are not authorized to perform this task."
                    });
                }

                var result = await _service.GetAddMountingStockDataAsync(GetCompanyId());
                var stockData = await _service.GetMountingStockByIdAsync(id, GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        Dropdowns = result,
                        StockData = stockData
                    }
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

        [HttpGet("mounting-detail/{id}")]
        public async Task<IActionResult> GetMountingDetail(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                (JewStockMasterDto? data, List<DiamondDetails> diamondList) = await _service.GetAllMountingDetailsAsync(id, companyId);

                if (data == null)
                {
                    return NotFound(new { Success = false, Message = "Mounting stock not found." });
                }

                return Ok(new
                {
                    Success = true,
                    Data = data,
                    DiamondList = diamondList
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

        [HttpGet("get-next-mountid")]
        public async Task<IActionResult> GetNextMountingStockId([FromQuery] string designId)
        {
            try
            {
                var stockId = await _service.GetNextMountingStockIdAsync(designId, GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    stockId = stockId
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

        [HttpGet("mounting-stock-dropdown")]
        public async Task<IActionResult> GetMountingStockDropdown([FromQuery] string category, [FromQuery] string actionType = "")
        {
            try
            {
                var result = await _service.GetMountingStockDropdownAsync(category, actionType, GetCompanyId());

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

        [HttpGet("get-alllotno-listall")]
        public async Task<IActionResult> GetAllLotNoListAll([FromQuery] string? searchby, [FromQuery] string? stoneType)
        {
            try
            {
                var data = await _service.GetAllLotNoListAllAsync(searchby ?? "", stoneType ?? "", GetCompanyId());
                var modifiedData = data.Select(x => new
                {
                    id = !string.IsNullOrWhiteSpace(x.Name) ? x.Name : x.ID,
                    text = !string.IsNullOrWhiteSpace(x.TextName)
                        ? x.TextName
                        : (!string.IsNullOrWhiteSpace(x.Name) ? x.Name : x.ID)
                });

                return Ok(modifiedData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("get-jewlotno-data")]
        public async Task<IActionResult> GetJewLotNoData([FromBody] LotNoRequest item)
        {
            try
            {
                if (string.IsNullOrEmpty(item.LotNo))
                {
                    return Ok(new { success = false, message = "LotNo is required." });
                }

                var data = await _service.GetJewLotNoDataAsync(item.LotNo, GetCompanyId());
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("add-mounting-item")]
        public async Task<IActionResult> AddMountingItemInGrid([FromBody] AddJewLotNo item)
        {
            try
            {
                // Note: In a real implementation, you would need to manage the grid state
                // This could be done via session, cache, or client-side state management
                // For now, we'll pass an empty list and expect the client to manage state
                (bool success, string message, JewGridData? data) = await _service.AddMountingItemInGridAsync(item, new List<JewDiamondDetails>());

                if (success)
                {
                    return Ok(new { Success = true, Message = message, Data = data });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("delete-mounting-item")]
        public async Task<IActionResult> DeleteMountingItemFromGrid([FromBody] int index)
        {
            try
            {
                // Note: Same as above - grid state management needed
                (bool success, string message, JewGridData? data) = await _service.DeleteMountingItemFromGridAsync(index, new List<JewDiamondDetails>());

                if (success)
                {
                    return Ok(new { Success = true, Message = message, Data = data });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("save-mounting-stock")]
        public async Task<IActionResult> SaveMountingStock([FromBody] JewStockMasterSaveRequest request)
        {
            try
            {
                (bool success, string message) = await _service.SaveMountingStockAsync(
                    request.Model,
                    request.Diamond,
                    request.Setting,
                    request.ActionType,
                    GetUserTypeId(),
                    GetCompanyId());

                if (success)
                {
                    return Ok(new { Success = true, Message = message });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("delete-mounting-stock/{id}")]
        public async Task<IActionResult> DeleteMountingStock(int id)
        {
            try
            {
                int permission = await _service.CheckUserPermissionAsync(
                    GetUserTypeId(), "Jew Stock Master", "delete", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "You are not authorized to perform this task."
                    });
                }

                (bool success, string message) = await _service.DeleteMountingStockMasterAsync(id, GetCompanyId());

                if (success)
                {
                    return Ok(new { Success = true, Message = message });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }

    // Request DTO for save operation
    public class JewStockMasterSaveRequest
    {
        public JewStockMasterDto Model { get; set; } = new JewStockMasterDto();
        public List<DiamondDetails>? Diamond { get; set; }
        public List<SettingDetails>? Setting { get; set; }
        public string ActionType { get; set; } = "";
    }
}
