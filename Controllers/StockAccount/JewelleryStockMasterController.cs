using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Dia_CoreAPI.Utilities;
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

            var result = await _service.GetMountingStockAsync(GetCompanyId(), "");

            return Ok(new
            {
                Success = true,
                Data = MapMountingStockList(result)
            });
        }

        private static IEnumerable<MountingduplicateMaster> MapMountingStockList(IEnumerable<JewStockMasterDto> rows)
        {
            return rows.Select(row => new MountingduplicateMaster
            {
                JewCode = row.JewCode,
                Stock_ID = row.Stock_ID ?? string.Empty,
                Pcs = row.Pcs,
                MountCategory = row.MountCategory ?? row.DesignName ?? string.Empty,
                MetalType = row.MetalType ?? string.Empty,
                Purity = row.Purity ?? string.Empty,
                TotalDiamondCarat = row.TotalDiamondCarat,
                GrossWeight = row.GrossWeight,
                NetWeight = row.NetWeight,
                TotalMetalAmount = row.TotalMetalAmount,
                FinalAmount = row.FinalAmount
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
                    Data = MapMountingStockList(result)
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
                (JewStockMasterDto? stockData, List<DiamondDetails> diamondList) =
                    await _service.GetAllMountingDetailsAsync(id, GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        Dropdowns = result,
                        StockData = stockData,
                        DiamondList = diamondList
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
                // Same as MVC: CreatedBy / ModifyBy = logged-in user
                var userId = GetUserId();
                if (request.Model != null)
                {
                    if (string.IsNullOrWhiteSpace(request.Model.CreatedBy))
                        request.Model.CreatedBy = userId;
                    request.Model.ModifyBy = userId;
                }

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

        // ── Google Drive (MVC: /master/jew-drive-*) ──────────────────────────

        /// <summary>
        /// Streams a Google Drive image or video (Option B HTTP link).
        /// AllowAnonymous so full https links work in browser / &lt;video src&gt; without Bearer token.
        /// Prefer ?id=FILE_ID (used in videoDriveLinks). ?name=… still needs auth (company folder).
        /// Correct path: /api/JewelleryStockMaster/jew-drive-image?id=…&amp;mediaType=video
        /// </summary>
        [AllowAnonymous]
        [HttpGet("jew-drive-image")]
        public async Task<IActionResult> JewDriveImage(string? id, string? path, string? name, string? mediaType)
        {
            try
            {
                string? key = !string.IsNullOrWhiteSpace(id) ? id : path;
                bool isVideo = !string.IsNullOrWhiteSpace(mediaType)
                    && mediaType.Equals("video", StringComparison.OrdinalIgnoreCase);

                // Name lookup needs company folder settings → requires logged-in user
                if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(name))
                {
                    string? companyId = User.FindFirst("CompanyID")?.Value;
                    if (string.IsNullOrWhiteSpace(companyId))
                        return Unauthorized("Login required to resolve media by file name. Use ?id=FILE_ID.");

                    string settingName = GetJewDriveSettingName(mediaType);
                    string folderId = await GetJewDriveFolderIdAsync(mediaType, companyId);

                    if (!GoogleDriveHelper.TryResolveFileByName(folderId, name, settingName, out var file, out var resolveError)
                        || file == null || string.IsNullOrEmpty(file.Id))
                    {
                        if (isVideo)
                        {
                            try
                            {
                                string imgSetting = GetJewDriveSettingName("image");
                                string imgFolder = await GetJewDriveFolderIdAsync("image", companyId);
                                if (GoogleDriveHelper.TryResolveFileByName(imgFolder, name, imgSetting, out var imgFile, out _)
                                    && imgFile != null && !string.IsNullOrEmpty(imgFile.Id))
                                {
                                    key = imgFile.Id;
                                }
                            }
                            catch { }
                        }
                        if (string.IsNullOrWhiteSpace(key))
                            return NotFound(resolveError ?? "File not found");
                    }
                    else
                    {
                        key = file.Id;
                    }
                }

                if (string.IsNullOrWhiteSpace(key))
                    return BadRequest("Missing file id or name");

                // Extract bare Drive id if a full URL was passed
                key = GoogleDriveHelper.ExtractFileId(key) ?? key;

                // Stream videos as chunks arrive (do not buffer entire file first).
                // Mid-file Range seeks still use the buffered path so 206 works.
                bool isSeekRequest = false;
                var rangeHeader = Request.GetTypedHeaders().Range;
                if (rangeHeader?.Ranges != null && rangeHeader.Ranges.Count > 0)
                {
                    var r = rangeHeader.Ranges.First();
                    isSeekRequest = r.From.GetValueOrDefault(0) > 0;
                }

                if (isVideo && !isSeekRequest && GoogleDriveHelper.HasCredentials())
                {
                    try
                    {
                        var meta = GoogleDriveHelper.GetFileMeta(key);
                        string streamType = meta.ContentType;
                        if (string.IsNullOrWhiteSpace(streamType)
                            || streamType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                        {
                            streamType = "video/mp4";
                        }

                        HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()
                            ?.DisableBuffering();

                        Response.StatusCode = StatusCodes.Status200OK;
                        Response.ContentType = streamType;
                        Response.Headers.CacheControl = "public,max-age=21600";
                        Response.Headers["Accept-Ranges"] = "bytes";
                        if (meta.Size.HasValue && meta.Size.Value > 0)
                            Response.ContentLength = meta.Size.Value;

                        await GoogleDriveHelper.DownloadFileToStreamAsync(
                            key,
                            Response.Body,
                            HttpContext.RequestAborted).ConfigureAwait(false);
                        return new EmptyResult();
                    }
                    catch
                    {
                        // fall through to buffered download
                    }
                }

                byte[] bytes = GoogleDriveHelper.DownloadFileBytes(key, out string contentType);
                if (bytes == null || bytes.Length == 0)
                    return NotFound(isVideo ? "Video not found" : "Image not found");

                if (string.IsNullOrWhiteSpace(contentType)
                    || contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = isVideo ? "video/mp4" : "image/jpeg";
                }

                Response.Headers.CacheControl = "public,max-age=21600";
                Response.Headers["Accept-Ranges"] = "bytes";
                return File(bytes, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Lookup Drive file by name in AddJewImages / AddJewVideo folder.
        /// </summary>
        [HttpGet("jew-drive-find-by-name")]
        public async Task<IActionResult> JewDriveFindByName(string? name, string? mediaType)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Ok(new { success = false, Data = "File name is required." });

            bool isVideo = !string.IsNullOrWhiteSpace(mediaType)
                && mediaType.Equals("video", StringComparison.OrdinalIgnoreCase);

            try
            {
                string settingName = GetJewDriveSettingName(mediaType);
                string folderId = await GetJewDriveFolderIdAsync(mediaType, GetCompanyId());
                string? resolveError = null;

                if (GoogleDriveHelper.TryResolveFileByName(folderId, name, settingName, out var file, out resolveError)
                    && file != null && !string.IsNullOrEmpty(file.Id))
                {
                    return Ok(new
                    {
                        success = true,
                        fileId = file.Id,
                        fileName = file.Name,
                        fileLink = GoogleDriveHelper.ToShareLink(file.Id),
                        mimeType = file.MimeType,
                        settingName,
                        folderId
                    });
                }

                if (isVideo)
                {
                    try
                    {
                        string imgSetting = GetJewDriveSettingName("image");
                        string imgFolder = await GetJewDriveFolderIdAsync("image", GetCompanyId());
                        if (GoogleDriveHelper.TryResolveFileByName(imgFolder, name, imgSetting, out var imgFile, out _)
                            && imgFile != null && !string.IsNullOrEmpty(imgFile.Id))
                        {
                            return Ok(new
                            {
                                success = true,
                                fileId = imgFile.Id,
                                fileName = imgFile.Name,
                                fileLink = GoogleDriveHelper.ToShareLink(imgFile.Id),
                                mimeType = imgFile.MimeType,
                                settingName = imgSetting,
                                folderId = imgFolder
                            });
                        }
                    }
                    catch { }
                }

                return Ok(new { success = false, Data = resolveError });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        /// <summary>List Drive image file names for a Stock ID (Abc.png, Abc1.png, …).</summary>
        [HttpGet("jew-drive-list-stock-images")]
        public async Task<IActionResult> JewDriveListStockImages(string? stockId)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                return Ok(new { success = false, Data = "Stock ID is required.", files = Array.Empty<string>() });

            try
            {
                string settingName = GetJewDriveSettingName("image");
                string folderId = await GetJewDriveFolderIdAsync("image", GetCompanyId());
                var files = GoogleDriveHelper.ListStockImageFileNames(folderId, stockId, out string? error);

                if (files == null || files.Count == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        Data = error ?? ("No images found for Stock ID \"" + stockId.Trim() + "\"."),
                        files = Array.Empty<string>(),
                        settingName,
                        folderId
                    });
                }

                return Ok(new { success = true, files, settingName, folderId });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message, files = Array.Empty<string>() });
            }
        }

        /// <summary>List Drive video file names for a Stock ID (with AddJewImages fallback).</summary>
        [HttpGet("jew-drive-list-stock-videos")]
        public async Task<IActionResult> JewDriveListStockVideos(string? stockId)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                return Ok(new { success = false, Data = "Stock ID is required.", files = Array.Empty<string>() });

            string? settingName = null;
            string? folderId = null;
            string? error = null;
            List<string>? files = null;

            try
            {
                settingName = GetJewDriveSettingName("video");
                folderId = await GetJewDriveFolderIdAsync("video", GetCompanyId());
                files = GoogleDriveHelper.ListStockVideoFileNames(folderId, stockId, out error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (files == null || files.Count == 0)
            {
                try
                {
                    string imgSetting = GetJewDriveSettingName("image");
                    string imgFolder = await GetJewDriveFolderIdAsync("image", GetCompanyId());
                    var imgFiles = GoogleDriveHelper.ListStockVideoFileNames(imgFolder, stockId, out var imgError);
                    if (imgFiles != null && imgFiles.Count > 0)
                    {
                        return Ok(new { success = true, files = imgFiles, settingName = imgSetting, folderId = imgFolder });
                    }
                    if (string.IsNullOrWhiteSpace(error))
                        error = imgError;
                }
                catch { }
            }

            if (files == null || files.Count == 0)
            {
                return Ok(new
                {
                    success = false,
                    Data = error ?? ("No videos found for Stock ID \"" + stockId.Trim() + "\"."),
                    files = Array.Empty<string>(),
                    settingName,
                    folderId
                });
            }

            return Ok(new { success = true, files, settingName, folderId });
        }

        private static string GetJewDriveSettingName(string? mediaType)
        {
            bool isVideo = !string.IsNullOrWhiteSpace(mediaType)
                && mediaType.Equals("video", StringComparison.OrdinalIgnoreCase);
            return isVideo ? "AddJewVideo" : "AddJewImages";
        }

        /// <summary>
        /// Image → Setting Master AddJewImages; Video → AddJewVideo (same aliases as MVC).
        /// </summary>
        private async Task<string> GetJewDriveFolderIdAsync(string? mediaType, string companyId)
        {
            var settings = await _service.GetJewSettingValueAsync(companyId);
            if (settings == null || settings.Count == 0)
                throw new Exception("Setting Master not loaded. Please re-login.");

            bool isVideo = !string.IsNullOrWhiteSpace(mediaType)
                && mediaType.Equals("video", StringComparison.OrdinalIgnoreCase);

            var pairs = settings.Select(s => new KeyValuePair<string, string?>(s.Name ?? "", s.Value));
            string settingName;
            string? folderId = GoogleDriveHelper.ResolveFolderIdFromSettings(pairs, isVideo, out settingName);
            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new Exception(isVideo
                    ? "Setting Master missing: AddJewVideo. Add folder link and re-login."
                    : "Setting Master missing: AddJewImages. Add folder link and re-login.");
            }

            return folderId;
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
