using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Dia_CoreAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryStockReportServices : IJewelleryStockReportServices
    {
        private readonly IJewelleryStockReportRepository _repo;
        private readonly IJewStockMasterService _jewStockMasterService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public JewelleryStockReportServices(
            IJewelleryStockReportRepository repo,
            IJewStockMasterService jewStockMasterService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _repo = repo;
            _jewStockMasterService = jewStockMasterService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<IEnumerable<JewelleryStockSearchResponseDto>> GetJewelleryStockReportAsync(JewelleryStockSearchFilterDto filter, string companyId)
        {
            var rows = (await _repo.GetJewelleryStockReportAsync(filter, companyId)).ToList();
            await EnrichReportDriveLinksAsync(rows, companyId);
            return rows;
        }

        /// <summary>
        /// Adds ImageDriveLinks / VideoDriveLinks on each jewellery row (response-only).
        /// ImagePath / VideoPath remain DB file names.
        /// </summary>
        private async Task EnrichReportDriveLinksAsync(List<JewelleryStockSearchResponseDto> rows, string companyId)
        {
            if (rows == null || rows.Count == 0) return;
            try
            {
                var settings = await _jewStockMasterService.GetJewSettingValueAsync(companyId);
                var pairs = (settings ?? new List<SettingMasterDto>())
                    .Select(s => new KeyValuePair<string, string?>(s.Name ?? "", s.Value))
                    .ToList();

                string imgSetting;
                string? imgFolder = GoogleDriveHelper.ResolveFolderIdFromSettings(pairs, false, out imgSetting);
                string vidSetting;
                string? vidFolder = GoogleDriveHelper.ResolveFolderIdFromSettings(pairs, true, out vidSetting);

                // Deduplicate by JewCode so grouped diamond/stone rows resolve once
                var linkCache = new Dictionary<int, (List<string> ImageLinks, List<string> VideoLinks)>();
                foreach (var row in rows)
                {
                    var jew = row?.Jewellery;
                    if (jew == null) continue;

                    if (linkCache.TryGetValue(jew.JewCode, out var cached))
                    {
                        jew.ImageDriveLinks = cached.ImageLinks ?? new List<string>();
                        jew.VideoDriveLinks = cached.VideoLinks ?? new List<string>();
                        continue;
                    }

                    jew.ImageDriveLinks = GoogleDriveHelper.ResolvePathsToShareLinks(
                            imgFolder, jew.ImagePath, imgSetting, useImageCdn: true)
                        ?? new List<string>();

                    // Option B: full HTTP API stream URL
                    string? apiBase = GoogleDriveHelper.ResolvePublicApiBaseUrl(
                        _httpContextAccessor.HttpContext?.Request, _configuration);
                    var videoLinks = GoogleDriveHelper.ResolvePathsToShareLinks(
                        vidFolder, jew.VideoPath, vidSetting,
                        useImageCdn: false, useVideoPreview: true, apiBaseUrl: apiBase);
                    if ((videoLinks == null || videoLinks.Count == 0)
                        && !string.IsNullOrWhiteSpace(jew.VideoPath)
                        && !string.IsNullOrWhiteSpace(imgFolder))
                    {
                        videoLinks = GoogleDriveHelper.ResolvePathsToShareLinks(
                            imgFolder, jew.VideoPath, imgSetting,
                            useImageCdn: false, useVideoPreview: true, apiBaseUrl: apiBase);
                    }
                    jew.VideoDriveLinks = videoLinks ?? new List<string>();

                    linkCache[jew.JewCode] = (jew.ImageDriveLinks, jew.VideoDriveLinks);
                }
            }
            catch
            {
                // Leave drive link fields empty if unavailable
            }
        }

        public async Task<JewelleryStockDropdownsDto> GetJewelleryStockDropdownsAsync(string companyId)
        {
            return await _repo.GetJewelleryStockDropdownsAsync(companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetStockIdsAsync(string searchText, string companyId)
        {
            return await _repo.GetStockIdsAsync(searchText, companyId);
        }
    }
}
