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
    public class ImportGradingController : ControllerBase
    {
        private static readonly JsonSerializerOptions PascalCaseJson = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly IImportGradingService _service;
        private readonly IWebHostEnvironment _env;

        public ImportGradingController(IImportGradingService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        private string UserTypeId => User.FindFirstValue("UserTypeId") ?? string.Empty;
        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;
        private string UserId => User.FindFirstValue("UserID") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet("importgrading")]
        public async Task<IActionResult> ImportGradingData()
        {
            try
            {
                var result = await _service.ImportGradingDataAsync(UserTypeId, CompanyId);
                if (result.Success)
                    return new JsonResult(new { success = true, mode = result.Mode ?? "Add" }, PascalCaseJson);
                return StatusCode(403, new { success = false, message = result.Data ?? "You are not authorized to perform this task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading Import Grading.", error = ex.Message });
            }
        }

        [HttpPost("update-gia-data")]
        public Task<IActionResult> UpdateGiaData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateGiaDataAsync);

        [HttpPost("update-igi-data")]
        public Task<IActionResult> UpdateIgiData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateIgiDataAsync);

        [HttpPost("update-hrd-data")]
        public Task<IActionResult> UpdateHrdData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateHrdDataAsync);

        [HttpPost("update-grader-data")]
        public Task<IActionResult> UpdateGraderData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateGraderDataAsync);

        [HttpPost("update-purchase-data")]
        public Task<IActionResult> UpdatePurchaseData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdatePurchaseDataAsync);

        [HttpPost("update-image-video-data")]
        public Task<IActionResult> UpdateImageVideoData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateImageVideoDataAsync);

        [HttpPost("update-parcelitem-data")]
        public Task<IActionResult> UpdateParcelItemData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateParcelItemDataAsync);

        [HttpPost("update-solitaireitem-data")]
        public Task<IActionResult> UpdateSolitaireItemData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateSolitaireItemDataAsync);

        [HttpPost("update-accountdata")]
        public Task<IActionResult> UpdateAccountData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UpdateAccountDataAsync);

        [HttpPost("upload-purchasedata")]
        public Task<IActionResult> UploadPurchaseData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UploadPurchaseDataAsync);

        [HttpPost("import-sale-data")]
        public Task<IActionResult> ImportSaleData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.ImportSaleDataAsync);

        [HttpPost("upload-purchasedetails")]
        public Task<IActionResult> UploadPurchaseDetails([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UploadPurchaseDetailsAsync);

        [HttpPost("upload-jewstockdata")]
        public Task<IActionResult> UploadJewStockData([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UploadJewStockDataAsync);

        [HttpPost("upload-jewstockdiamonddetailsdata")]
        public Task<IActionResult> UploadJewStockDiamondDetails([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UploadJewStockDiamondDetailsAsync);

        [HttpPost("update-SizeId")]
        public async Task<IActionResult> UpdateSizeIds()
        {
            try
            {
                var result = await _service.UpdateSizeIdsAsync(CompanyId, UserTypeId);
                return ToJsonResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-deletedrecords")]
        public async Task<IActionResult> DeleteDeletedRecords()
        {
            try
            {
                var result = await _service.DeleteDeletedRecordsAsync(CompanyId, UserTypeId);
                return ToJsonResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("UploadLogo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(IFormFile image)
        {
            try
            {
                var result = await _service.UploadLogoAsync(image, CompanyId, UserTypeId, _env.WebRootPath ?? string.Empty);
                if (!string.IsNullOrEmpty(result.Path))
                    return new JsonResult(new { success = result.Success, message = result.Message, path = result.Path }, PascalCaseJson);
                return new JsonResult(new { success = result.Success, message = result.Message ?? result.Data }, PascalCaseJson);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message }, PascalCaseJson);
            }
        }

        [HttpPost("upload-jewellery-full")]
        public Task<IActionResult> UploadJewelleryFull([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.UploadJewelleryFullAsync);

        [HttpPost("Inventory/ImportAskingRate")]
        public Task<IActionResult> ImportAskingRate([FromForm] ImportGradingDTO importData) => PostImportAsync(importData, _service.ImportAskingRateAsync);

        private async Task<IActionResult> PostImportAsync(
            ImportGradingDTO importData,
            Func<ImportGradingDTO, string, string, string, Task<ImportGradingResult>> action)
        {
            try
            {
                var result = await action(importData, CompanyId, UserId, UserTypeId);
                return ToJsonResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, Data = ex.Message });
            }
        }

        private IActionResult ToJsonResult(ImportGradingResult result)
        {
            if (!result.Success && result.Data == "You are not authorized to perform this task.")
                return StatusCode(403, new { success = false, Data = result.Data });
            return new JsonResult(new { success = result.Success, Data = result.Data }, PascalCaseJson);
        }
    }
}
