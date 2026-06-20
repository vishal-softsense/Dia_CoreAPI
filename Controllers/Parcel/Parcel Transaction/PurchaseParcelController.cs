using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Parcel.Parcel_Transaction
{
    [ApiController]
    [Route("api/purchaseparcel")]
    public class PurchaseParcelController : ControllerBase
    {
        private readonly IPurchaseParcelService _service;

        public PurchaseParcelController(
            IPurchaseParcelService service)
        {
            _service = service;
        }

        //private string GetUserId() => HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        [HttpGet("purchaseparcel")]
        public async Task<IActionResult> PurchaseParcel()
        {
            int permission =
                await _service.CheckUserPermissionAsync(GetUserTypeId(), "Parcel Purchase", "select", GetCompanyId());

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

        [HttpGet("purchaseparcel-data")]
        public async Task<IActionResult> GetPurchaseParcelList()
        {
            var data =
                await _service.GetPurchaseParcelListAsync(1, GetCompanyId());

            return Ok(new
            {
                Success = true,
                Data = data
            });
        }

        [HttpGet("add-purchaseparcel")]
        public async Task<IActionResult> AddPurchaseParcel()
        {
            try
            {
                int permission =
                    await _service.CheckUserPermissionAsync(GetUserTypeId(),"Parcel Purchase", "insert", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "You are not authorized to perform this task."
                    });
                }

                var result =
                    await _service.GetPurchaseParcelPageDataAsync(GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("add-purchaseparcel")]
        public async Task<IActionResult> AddPurchaseParcel(
    [FromBody] PurchaseParcelRequest request)
        {
            try
            {
                var result =
                    await _service.AddPurchaseParcelAsync(
                        request.SalePur,
                        request.Items,
                        GetUserId(),
                        GetCompanyId());

                return Ok(new
                {
                    Success = result.Success,
                    Data = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("add-single-lot")]
        public async Task<IActionResult> AddItemInGrid([FromBody] AddSingleLotParcelRequest request)
        {
            try
            {
                var result =
                    await _service.AddItemInGridAsync(request.Item,request.ParcelList,GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("get-parcellot-detail")]
        public async Task<IActionResult> GetLotDetails([FromBody] LotString item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.LotNo))
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "Lot No is required."
                    });
                }

                var data =
                    await _service.GetLotDetailsAsync(item.LotNo, GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }
    }
}