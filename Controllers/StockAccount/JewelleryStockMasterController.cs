using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.StockAccount
{
    [Route("api/[controller]")]
    [ApiController]
    public class JewelleryStockMasterController : ControllerBase
    {
        private readonly IJewStockMasterService _service;

        public JewelleryStockMasterController(IJewStockMasterService service)
        {
            _service = service;
        }

        private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

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
                var result = await _service.GetMountingStockAsync(GetCompanyId(),"");

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
    }
}
