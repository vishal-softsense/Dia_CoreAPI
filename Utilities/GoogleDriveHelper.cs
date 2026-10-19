using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Dia_CoreAPI.Utilities
{
    /// <summary>
    /// Jewellery media on Google Drive only (no local project folder storage).
    /// Image folder: Setting Master AddJewImages / AddJewlImages
    /// Video folder: Setting Master AddJewVideo / AddJewlVideo
    /// Ported from MVC GoogleDriveHelper — same resolve/list/download behavior.
    /// </summary>
    public static class GoogleDriveHelper
    {
        private static readonly object PublicFolderCacheLock = new object();
        private static readonly Dictionary<string, Dictionary<string, string>> PublicFolderCache =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, DateTime> PublicFolderCacheAt =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> PublicFolderStatusCache =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string? _contentRootPath;
        private static string? _configuredCredentialPath;

        /// <summary>Call once at startup (Program.cs) with content root + optional appsettings path.</summary>
        public static void Configure(string? contentRootPath, string? credentialPath = null)
        {
            _contentRootPath = contentRootPath;
            _configuredCredentialPath = credentialPath;
        }

        public static string? ExtractFolderId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();
            var match = Regex.Match(value, @"folders\/([a-zA-Z0-9_-]+)");
            if (match.Success)
                return match.Groups[1].Value;

            return value;
        }

        public static string? ExtractFileId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();
            var match = Regex.Match(value, @"(?:\/d\/|id=|open\?id=|thumbnail\?id=)([a-zA-Z0-9_-]{20,})", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;

            if (Regex.IsMatch(value, @"^[a-zA-Z0-9_-]{20,}$") && !value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return value;

            return null;
        }

        /// <summary>Shareable Drive link (same format as Drive "Copy link").</summary>
        public static string ToShareLink(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return string.Empty;
            return "https://drive.google.com/file/d/" + fileId.Trim() + "/view?usp=drive_link";
        }

        /// <summary>Direct image CDN URL used for ImageDriveLinks in API responses.</summary>
        public static string ToImageCdnLink(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return string.Empty;
            return "https://lh3.googleusercontent.com/d/" + fileId.Trim() + "=w1000";
        }

        /// <summary>
        /// Option B: full HTTP video stream URL via this API (never Drive UI, never lh3 image CDN).
        /// Example: https://{apiHost}/api/JewelleryStockMaster/jew-drive-image?id={fileId}&amp;mediaType=video
        /// </summary>
        public static string ToVideoApiStreamLink(string fileId, string? apiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return string.Empty;

            string id = Uri.EscapeDataString(fileId.Trim());
            string path = "/api/JewelleryStockMaster/jew-drive-image?id=" + id + "&mediaType=video";
            string baseUrl = (apiBaseUrl ?? "").Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
                return path;
            return baseUrl + path;
        }

        /// <summary>In-app &lt;video&gt; playback URL (API stream). Not used in VideoDriveLinks response.</summary>
        public static string ToVideoStreamLink(string fileId, string? apiBaseUrl = null)
            => ToVideoApiStreamLink(fileId, apiBaseUrl);

        /// <summary>
        /// VideoDriveLinks builder — Google Drive share link (same as Drive "Copy link").
        /// Example: https://drive.google.com/file/d/{fileId}/view?usp=drive_link
        /// </summary>
        public static string ToVideoPreviewLink(string fileId, string? apiBaseUrl = null)
            => ToShareLink(fileId);

        /// <summary>
        /// Resolve public API base for Option B video links.
        /// Prefer configured GoogleDrive:PublicApiBaseUrl, else current request host.
        /// </summary>
        public static string? ResolvePublicApiBaseUrl(
            HttpRequest? request,
            IConfiguration? configuration = null)
        {
            string? configured = configuration?["GoogleDrive:PublicApiBaseUrl"]
                ?? configuration?["App:PublicApiBaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured))
                return configured.Trim().TrimEnd('/');

            if (request == null)
                return null;

            // Respect reverse-proxy forwarded headers when present
            string? scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(scheme))
                scheme = request.Scheme;
            string? host = request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(host))
                host = request.Host.Value;
            if (string.IsNullOrWhiteSpace(host))
                return null;
            return (scheme ?? "https").Trim() + "://" + host.Trim().TrimEnd('/');
        }

        /// <summary>
        /// Resolve Setting Master AddJewImages / AddJewVideo folder ID.
        /// settings: Name/Value pairs from TabSettingMaster.
        /// </summary>
        public static string? ResolveFolderIdFromSettings(
            IEnumerable<KeyValuePair<string, string?>> settings,
            bool isVideo,
            out string settingName)
        {
            string[] names = isVideo
                ? new[] { "AddJewVideo", "AddJewlVideo" }
                : new[] { "AddJewImages", "AddJewlImages", "AddJewImage", "AddJewlImage" };

            settingName = isVideo ? "AddJewVideo" : "AddJewImages";
            if (settings == null)
                return null;

            var list = settings.ToList();
            foreach (var n in names)
            {
                var value = list
                    .Where(a => string.Equals(a.Key, n, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Value)
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                if (!string.IsNullOrWhiteSpace(value))
                {
                    settingName = n;
                    return ExtractFolderId(value);
                }
            }

            return null;
        }

        /// <summary>
        /// Convert stored ImagePath/VideoPath (file names or IDs, pipe-separated) → links.
        /// Used for API response only — DB continues to store file names.
        /// useImageCdn → lh3 image CDN;
        /// useVideoPreview → Google Drive share link for VideoDriveLinks;
        /// otherwise share view link.
        /// </summary>
        public static List<string> ResolvePathsToShareLinks(
            string? folderId,
            string? pathValue,
            string? settingName = null,
            bool useImageCdn = false,
            bool useVideoPreview = false,
            string? apiBaseUrl = null)
        {
            var links = new List<string>();
            if (string.IsNullOrWhiteSpace(pathValue))
                return links;

            settingName = string.IsNullOrWhiteSpace(settingName) ? "AddJewImages / AddJewVideo" : settingName.Trim();
            var parts = pathValue
                .Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => (p ?? "").Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p) && !string.Equals(p, "null", StringComparison.OrdinalIgnoreCase));

            foreach (var part in parts)
            {
                string BuildLink(string id)
                {
                    // Images → lh3 CDN; videos / default → Google Drive share link
                    if (useImageCdn) return ToImageCdnLink(id);
                    return ToShareLink(id); // VideoDriveLinks + share links
                }

                // Already a Drive URL / bare file id
                string? existingId = ExtractFileId(part);
                if (!string.IsNullOrWhiteSpace(existingId)
                    && (part.IndexOf("drive.google.com", StringComparison.OrdinalIgnoreCase) >= 0
                        || part.IndexOf("googleusercontent.com", StringComparison.OrdinalIgnoreCase) >= 0
                        || (part.IndexOf('.') < 0 && !part.StartsWith("http", StringComparison.OrdinalIgnoreCase))))
                {
                    links.Add(BuildLink(existingId));
                    continue;
                }

                // File name → resolve in folder
                if (!string.IsNullOrWhiteSpace(folderId)
                    && TryResolveFileByName(folderId, part, settingName, out var file, out _)
                    && file != null && !string.IsNullOrEmpty(file.Id))
                {
                    links.Add(BuildLink(file.Id));
                    continue;
                }

                // Last resort: extract id from any other form
                if (!string.IsNullOrWhiteSpace(existingId))
                    links.Add(BuildLink(existingId));
            }

            return links;
        }

        public static string JoinShareLinks(IEnumerable<string> links)
        {
            return string.Join("|", (links ?? Array.Empty<string>()).Where(l => !string.IsNullOrWhiteSpace(l)));
        }

        public static string? GetCredentialPath()
        {
            // Absolute path from appsettings
            if (!string.IsNullOrWhiteSpace(_configuredCredentialPath)
                && Path.IsPathRooted(_configuredCredentialPath)
                && System.IO.File.Exists(_configuredCredentialPath))
            {
                return _configuredCredentialPath;
            }

            string relative = string.IsNullOrWhiteSpace(_configuredCredentialPath)
                ? "App_Data/GoogleDriveServiceAccount.json"
                : _configuredCredentialPath.Trim().TrimStart('~', '/', '\\').Replace('/', Path.DirectorySeparatorChar);

            if (!string.IsNullOrEmpty(_contentRootPath))
            {
                string candidate = Path.Combine(_contentRootPath, relative);
                if (System.IO.File.Exists(candidate))
                    return candidate;

                string appData = Path.Combine(_contentRootPath, "App_Data", "GoogleDriveServiceAccount.json");
                if (System.IO.File.Exists(appData))
                    return appData;
            }

            // Fallback next to running assembly
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string candidate = Path.Combine(baseDir, "App_Data", "GoogleDriveServiceAccount.json");
                if (System.IO.File.Exists(candidate))
                    return candidate;
            }
            catch { }

            return null;
        }

        public static bool HasCredentials()
        {
            return !string.IsNullOrEmpty(GetCredentialPath());
        }

        /// <summary>
        /// Resolve Drive file by name without throwing.
        /// Order: public shared folder listing → service account API.
        /// </summary>
        public static bool TryResolveFileByName(string folderId, string fileName, out DriveFile file, out string error)
        {
            return TryResolveFileByName(folderId, fileName, null, out file, out error);
        }

        public static bool TryResolveFileByName(string folderId, string fileName, string settingName, out DriveFile file, out string error)
        {
            file = null;
            error = null;
            settingName = string.IsNullOrWhiteSpace(settingName) ? "AddJewImages / AddJewVideo" : settingName.Trim();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                error = "File name is required.";
                return false;
            }

            fileName = Path.GetFileName(fileName.Trim());

            folderId = ExtractFolderId(folderId);
            if (string.IsNullOrWhiteSpace(folderId))
            {
                error = "Google Drive folder ID (" + settingName + ") is missing in Setting Master. Re-login after saving settings.";
                return false;
            }

            // Build name variants (Abc.png/mp4, Abc1, Abc 1, Abc (1))
            var namesToTry = new List<string> { fileName };
            foreach (var alt in ExpandStockMediaNameVariants(fileName))
            {
                if (!namesToTry.Contains(alt, StringComparer.OrdinalIgnoreCase))
                    namesToTry.Add(alt);
            }

            string folderStatus = "error";
            foreach (var tryName in namesToTry)
            {
                string publicId = LookupPublicFolderFileId(folderId, tryName, out folderStatus);
                if (!string.IsNullOrWhiteSpace(publicId))
                {
                    file = new DriveFile
                    {
                        Id = publicId,
                        Name = tryName,
                        MimeType = GuessMimeType(tryName)
                    };
                    return true;
                }
            }

            // 2) Service account API (optional, for private folders)
            if (HasCredentials())
            {
                try
                {
                    var service = GetDriveService();
                    foreach (var tryName in namesToTry)
                    {
                        string safeName = tryName.Replace("'", "\\'");
                        string q = string.Format(
                            "name = '{0}' and '{1}' in parents and trashed = false",
                            safeName, folderId);

                        var listReq = service.Files.List();
                        listReq.Q = q;
                        listReq.Fields = "files(id, name, mimeType, webViewLink)";
                        listReq.PageSize = 5;
                        listReq.SupportsAllDrives = true;
                        listReq.IncludeItemsFromAllDrives = true;

                        var result = listReq.Execute();
                        file = result.Files != null ? result.Files.FirstOrDefault() : null;
                        if (file != null && !string.IsNullOrEmpty(file.Id))
                            return true;
                    }

                    error = "File \"" + fileName + "\" not found in Setting " + settingName + " Drive folder.";
                    return false;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            if (string.Equals(folderStatus, "private", StringComparison.OrdinalIgnoreCase))
            {
                error =
                    "Setting " + settingName + " Drive folder is private / not listable. " +
                    "Open that folder in Google Drive → Share → Anyone with the link → Viewer, then try again.";
                return false;
            }

            error =
                "File \"" + fileName + "\" not found in Setting " + settingName + " folder. " +
                "Use exact Drive file name, and share the folder as Anyone with the link.";
            return false;
        }

        /// <summary>
        /// From Abc.png / Abc.mp4 / Abc1 / Abc 1 / Abc (1) produce sibling name styles.
        /// </summary>
        private static IEnumerable<string> ExpandStockMediaNameVariants(string fileName)
        {
            fileName = DecodeDriveNameEscapes((fileName ?? "").Trim());
            if (string.IsNullOrWhiteSpace(fileName))
                yield break;
            if (!IsImageExtension(fileName) && !IsVideoExtension(fileName))
                yield break;

            // Split "StockID" + optional index style + extension
            var m = Regex.Match(
                fileName,
                @"^(.+?)(?:(\d+)| (\d+)| \((\d+)\)|\((\d+)\))?(\.(?:png|jpe?g|gif|bmp|webp|mp4|mov|avi|mkv|webm))$",
                RegexOptions.IgnoreCase);
            if (!m.Success)
                yield break;

            string baseId = m.Groups[1].Value.TrimEnd();
            string ext = m.Groups[6].Value;
            string num =
                m.Groups[2].Success ? m.Groups[2].Value
                : m.Groups[3].Success ? m.Groups[3].Value
                : m.Groups[4].Success ? m.Groups[4].Value
                : m.Groups[5].Success ? m.Groups[5].Value
                : null;

            if (string.IsNullOrEmpty(num))
            {
                yield return baseId + ext;
                yield break;
            }

            int idx;
            if (!int.TryParse(num, out idx) || idx <= 0)
                yield break;

            yield return baseId + idx + ext;
            yield return baseId + " " + idx + ext;
            yield return baseId + " (" + idx + ")" + ext;
            yield return baseId + "(" + idx + ")" + ext;
        }

        /// <summary>Backward-compatible wrapper. Prefer TryResolveFileByName. </summary>
        public static DriveFile FindFileByName(string folderId, string fileName)
        {
            DriveFile file;
            string error;
            if (!TryResolveFileByName(folderId, fileName, out file, out error))
                throw new InvalidOperationException(error ?? "Drive file not found.");
            return file;
        }

        /// <summary>
        /// Index for Stock ID image name styles, or -1 if not a match.
        /// Abc.png → 0 | Abc1.png / Abc 1.png / Abc (1).png → 1
        /// </summary>
        public static int GetStockImageNameIndex(string fileName, string stockId)
        {
            fileName = DecodeDriveNameEscapes((fileName ?? "").Trim());
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(stockId))
                return -1;

            string escaped = Regex.Escape(stockId);
            // Abc.png | Abc1.png | Abc 1.png | Abc (1).png | Abc(1).png | Abc_1.png | Abc-1.png
            var re = new Regex(
                "^" + escaped + "(?:[_\\-]?(\\d+)|\\s+(\\d+)|\\s*\\((\\d+)\\))?\\.(png|jpe?g|gif|bmp|webp)$",
                RegexOptions.IgnoreCase);
            var m = re.Match(fileName);
            if (!m.Success)
                return -1;

            string num = m.Groups[1].Success ? m.Groups[1].Value
                : (m.Groups[2].Success ? m.Groups[2].Value
                : (m.Groups[3].Success ? m.Groups[3].Value : null));
            if (string.IsNullOrEmpty(num))
                return 0;
            int idx;
            return int.TryParse(num, out idx) ? idx : -1;
        }

        /// <summary>
        /// Candidate Drive names for a Stock ID index (exact / space / parentheses).
        /// </summary>
        public static IEnumerable<string> GetStockImageFileNameCandidates(string stockId, int index)
        {
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(stockId))
                yield break;

            if (index < 0) index = 0;
            string[] exts = { "png", "jpg", "jpeg", "webp", "gif", "bmp" };
            foreach (var ext in exts)
            {
                if (index == 0)
                {
                    yield return stockId + "." + ext;
                }
                else
                {
                    yield return stockId + index + "." + ext;
                    yield return stockId + " " + index + "." + ext;
                    yield return stockId + " (" + index + ")." + ext;
                    yield return stockId + "(" + index + ")." + ext;
                }
            }
        }

        /// <summary>
        /// List image file names for a Stock ID in Drive folder (sorted by index).
        /// Accepts: Abc.png, Abc1.png, Abc 1.png, Abc (1).png (and jpg/gif/bmp/webp).
        /// Uses public folder map + service-account folder listing when available.
        /// </summary>
        public static List<string> ListStockImageFileNames(string folderId, string stockId, out string error)
        {
            error = null;
            var result = new List<string>();
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(stockId))
            {
                error = "Stock ID is required.";
                return result;
            }

            folderId = ExtractFolderId(folderId);
            if (string.IsNullOrWhiteSpace(folderId))
            {
                error = "Google Drive folder ID is missing.";
                return result;
            }

            var nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string status = null;

            // 1) Public shared folder listing (use cache first — avoid slow re-scrape)
            var map = GetPublicFolderNameMap(folderId, false, out status);
            if (map == null || map.Count == 0)
                map = GetPublicFolderNameMap(folderId, true, out status);
            if (map != null)
            {
                foreach (var key in map.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || !IsUsableFileMapId(map[key]))
                        continue;
                    string name = DecodeDriveNameEscapes(key.Trim());
                    if (GetStockImageNameIndex(name, stockId) >= 0
                        || IsLooseStockImageMatch(name, stockId))
                        nameSet.Add(name);
                }
            }

            // 2) Service account: list folder files (needed when public scrape misses spaced names)
            if (HasCredentials())
            {
                try
                {
                    foreach (var name in ListFolderFileNamesViaApi(folderId))
                    {
                        if (GetStockImageNameIndex(name, stockId) >= 0)
                            nameSet.Add(name);
                    }
                }
                catch (Exception ex)
                {
                    if (nameSet.Count == 0)
                        error = ex.Message;
                }
            }

            // 3) Soft match: any image whose name starts with Stock ID (covers odd spacing)
            if (map != null && nameSet.Count == 0)
            {
                foreach (var key in map.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || !IsUsableFileMapId(map[key]))
                        continue;
                    string name = DecodeDriveNameEscapes(key.Trim());
                    if (!IsImageExtension(name))
                        continue;
                    if (name.StartsWith(stockId, StringComparison.OrdinalIgnoreCase)
                        && GetStockImageNameIndex(name, stockId) >= 0)
                        nameSet.Add(name);
                }
            }

            // 4) Probe known name styles against cached public map (one folder fetch already done)
            if (nameSet.Count == 0 && map != null && map.Count > 0)
            {
                for (int i = 0; i <= 15; i++)
                {
                    bool anyForIndex = false;
                    foreach (var candidate in GetStockImageFileNameCandidates(stockId, i))
                    {
                        string id = FindNameInMap(map, candidate);
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            nameSet.Add(candidate);
                            anyForIndex = true;
                            break;
                        }
                    }
                    if (!anyForIndex && i > 0)
                        break;
                }
            }

            // 5) API exact-name probes when credentials exist and still nothing
            if (nameSet.Count == 0 && HasCredentials())
            {
                for (int i = 0; i <= 15; i++)
                {
                    bool anyForIndex = false;
                    foreach (var candidate in GetStockImageFileNameCandidates(stockId, i))
                    {
                        try
                        {
                            var service = GetDriveService();
                            string safeName = candidate.Replace("'", "\\'");
                            var listReq = service.Files.List();
                            listReq.Q = string.Format(
                                "name = '{0}' and '{1}' in parents and trashed = false",
                                safeName, folderId);
                            listReq.Fields = "files(id, name)";
                            listReq.PageSize = 1;
                            listReq.SupportsAllDrives = true;
                            listReq.IncludeItemsFromAllDrives = true;
                            var apiResult = listReq.Execute();
                            var f = apiResult.Files != null ? apiResult.Files.FirstOrDefault() : null;
                            if (f != null && !string.IsNullOrEmpty(f.Id))
                            {
                                nameSet.Add(string.IsNullOrWhiteSpace(f.Name) ? candidate : f.Name.Trim());
                                anyForIndex = true;
                                break;
                            }
                        }
                        catch { }
                    }
                    if (!anyForIndex && i > 0)
                        break;
                }
            }

            if (nameSet.Count == 0)
            {
                if (string.Equals(status, "private", StringComparison.OrdinalIgnoreCase) && !HasCredentials())
                    error = "Drive folder is private / not listable.";
                else
                    error = error ?? ("No images found for Stock ID \"" + stockId + "\".");
                return result;
            }

            result = nameSet
                .Select(n =>
                {
                    int idx = GetStockImageNameIndex(n, stockId);
                    if (idx < 0 && IsLooseStockImageMatch(n, stockId))
                        idx = 50; // after normal numbered styles
                    return Tuple.Create(idx, n);
                })
                .Where(t => t.Item1 >= 0)
                .OrderBy(t => t.Item1)
                .ThenBy(t => t.Item2, StringComparer.OrdinalIgnoreCase)
                .Select(t => t.Item2)
                .ToList();

            return result;
        }

        private static IEnumerable<string> ListFolderFileNamesViaApi(string folderId, bool videos = false)
        {
            var service = GetDriveService();
            string pageToken = null;
            do
            {
                var listReq = service.Files.List();
                listReq.Q = string.Format("'{0}' in parents and trashed = false", folderId);
                listReq.Fields = "nextPageToken, files(id, name, mimeType)";
                listReq.PageSize = 200;
                listReq.PageToken = pageToken;
                listReq.SupportsAllDrives = true;
                listReq.IncludeItemsFromAllDrives = true;

                var result = listReq.Execute();
                if (result.Files != null)
                {
                    foreach (var f in result.Files)
                    {
                        if (f == null || string.IsNullOrWhiteSpace(f.Name))
                            continue;
                        if (videos)
                        {
                            if (!IsVideoExtension(f.Name)
                                && (f.MimeType == null || f.MimeType.IndexOf("video/", StringComparison.OrdinalIgnoreCase) < 0))
                                continue;
                        }
                        else
                        {
                            if (!IsImageExtension(f.Name)
                                && (f.MimeType == null || f.MimeType.IndexOf("image/", StringComparison.OrdinalIgnoreCase) < 0))
                                continue;
                        }
                        yield return f.Name.Trim();
                    }
                }
                pageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));
        }

        /// <summary>
        /// Index for Stock ID video name styles, or -1 if not a match.
        /// Abc.mp4 → 0 | Abc1.mp4 / Abc 1.mp4 / Abc (1).mp4 → 1
        /// </summary>
        public static int GetStockVideoNameIndex(string fileName, string stockId)
        {
            fileName = DecodeDriveNameEscapes((fileName ?? "").Trim());
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(stockId))
                return -1;

            string escaped = Regex.Escape(stockId);
            var re = new Regex(
                "^" + escaped + "(?:[_\\-]?(\\d+)|\\s+(\\d+)|\\s*\\((\\d+)\\))?\\.(mp4|mov|avi|mkv|webm)$",
                RegexOptions.IgnoreCase);
            var m = re.Match(fileName);
            if (!m.Success)
                return -1;

            string num = m.Groups[1].Success ? m.Groups[1].Value
                : (m.Groups[2].Success ? m.Groups[2].Value
                : (m.Groups[3].Success ? m.Groups[3].Value : null));
            if (string.IsNullOrEmpty(num))
                return 0;
            int idx;
            return int.TryParse(num, out idx) ? idx : -1;
        }

        private static bool IsLooseStockVideoMatch(string fileName, string stockId)
        {
            fileName = DecodeDriveNameEscapes((fileName ?? "").Trim());
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(stockId))
                return false;
            if (!IsVideoExtension(fileName))
                return false;
            if (!fileName.StartsWith(stockId, StringComparison.OrdinalIgnoreCase))
                return false;
            if (fileName.Length == stockId.Length)
                return false;
            char next = fileName[stockId.Length];
            return next == '.' || next == ' ' || next == '(' || next == '_' || next == '-'
                || char.IsDigit(next);
        }

        /// <summary>
        /// Candidate Drive video names for a Stock ID index (exact / space / parentheses).
        /// Same naming styles as images: Abc.mp4, Abc1.mp4, Abc 1.mp4, Abc (1).mp4.
        /// </summary>
        public static IEnumerable<string> GetStockVideoFileNameCandidates(string stockId, int index)
        {
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(stockId))
                yield break;

            if (index < 0) index = 0;
            string[] exts = { "mp4", "mov", "avi", "mkv", "webm" };
            foreach (var ext in exts)
            {
                if (index == 0)
                {
                    yield return stockId + "." + ext;
                }
                else
                {
                    yield return stockId + index + "." + ext;
                    yield return stockId + " " + index + "." + ext;
                    yield return stockId + " (" + index + ")." + ext;
                    yield return stockId + "(" + index + ")." + ext;
                }
            }
        }

        /// <summary>
        /// List video file names for a Stock ID (Abc.mp4, Abc 1.mp4, Abc (1).mp4, …).
        /// Same resolve strategy as ListStockImageFileNames (folder map + API list + candidate probes).
        /// </summary>
        public static List<string> ListStockVideoFileNames(string folderId, string stockId, out string error)
        {
            error = null;
            var result = new List<string>();
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(stockId))
            {
                error = "Stock ID is required.";
                return result;
            }

            folderId = ExtractFolderId(folderId);
            if (string.IsNullOrWhiteSpace(folderId))
            {
                error = "Google Drive folder ID is missing.";
                return result;
            }

            var nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string status = null;

            // 1) Public shared folder listing
            var map = GetPublicFolderNameMap(folderId, false, out status);
            if (map == null || map.Count == 0)
                map = GetPublicFolderNameMap(folderId, true, out status);
            if (map != null)
            {
                foreach (var key in map.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || !IsUsableFileMapId(map[key]))
                        continue;
                    string name = DecodeDriveNameEscapes(key.Trim());
                    if (GetStockVideoNameIndex(name, stockId) >= 0 || IsLooseStockVideoMatch(name, stockId))
                        nameSet.Add(name);
                }
            }

            // 2) Service account: list folder videos
            if (HasCredentials())
            {
                try
                {
                    foreach (var name in ListFolderFileNamesViaApi(folderId, true))
                    {
                        if (GetStockVideoNameIndex(name, stockId) >= 0 || IsLooseStockVideoMatch(name, stockId))
                            nameSet.Add(name);
                    }
                }
                catch (Exception ex)
                {
                    if (nameSet.Count == 0)
                        error = ex.Message;
                }
            }

            // 3) Soft match: any video whose name starts with Stock ID
            if (map != null && nameSet.Count == 0)
            {
                foreach (var key in map.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || !IsUsableFileMapId(map[key]))
                        continue;
                    string name = DecodeDriveNameEscapes(key.Trim());
                    if (!IsVideoExtension(name))
                        continue;
                    if (name.StartsWith(stockId, StringComparison.OrdinalIgnoreCase)
                        && GetStockVideoNameIndex(name, stockId) >= 0)
                        nameSet.Add(name);
                }
            }

            // 4) Probe known name styles against cached public map
            if (nameSet.Count == 0 && map != null && map.Count > 0)
            {
                for (int i = 0; i <= 15; i++)
                {
                    bool anyForIndex = false;
                    foreach (var candidate in GetStockVideoFileNameCandidates(stockId, i))
                    {
                        string id = FindNameInMap(map, candidate);
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            nameSet.Add(candidate);
                            anyForIndex = true;
                            break;
                        }
                    }
                    if (!anyForIndex && i > 0)
                        break;
                }
            }

            // 5) API exact-name probes when credentials exist and still nothing
            if (nameSet.Count == 0 && HasCredentials())
            {
                for (int i = 0; i <= 15; i++)
                {
                    bool anyForIndex = false;
                    foreach (var candidate in GetStockVideoFileNameCandidates(stockId, i))
                    {
                        try
                        {
                            var service = GetDriveService();
                            string safeName = candidate.Replace("'", "\\'");
                            var listReq = service.Files.List();
                            listReq.Q = string.Format(
                                "name = '{0}' and '{1}' in parents and trashed = false",
                                safeName, folderId);
                            listReq.Fields = "files(id, name)";
                            listReq.PageSize = 1;
                            listReq.SupportsAllDrives = true;
                            listReq.IncludeItemsFromAllDrives = true;
                            var apiResult = listReq.Execute();
                            var f = apiResult.Files != null ? apiResult.Files.FirstOrDefault() : null;
                            if (f != null && !string.IsNullOrEmpty(f.Id))
                            {
                                nameSet.Add(string.IsNullOrWhiteSpace(f.Name) ? candidate : f.Name.Trim());
                                anyForIndex = true;
                                break;
                            }
                        }
                        catch { }
                    }
                    if (!anyForIndex && i > 0)
                        break;
                }
            }

            if (nameSet.Count == 0)
            {
                if (string.Equals(status, "private", StringComparison.OrdinalIgnoreCase) && !HasCredentials())
                    error = "Drive folder is private / not listable.";
                else
                    error = error ?? ("No videos found for Stock ID \"" + stockId + "\".");
                return result;
            }

            result = nameSet
                .Select(n =>
                {
                    int idx = GetStockVideoNameIndex(n, stockId);
                    if (idx < 0 && IsLooseStockVideoMatch(n, stockId))
                        idx = 50;
                    return Tuple.Create(idx, n);
                })
                .Where(t => t.Item1 >= 0)
                .OrderBy(t => t.Item1)
                .ThenBy(t => t.Item2, StringComparer.OrdinalIgnoreCase)
                .Select(t => t.Item2)
                .ToList();

            return result;
        }

        private static string DecodeDriveNameEscapes(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            // Drive HTML often embeds \x20 \x28 \x29 etc.
            return Regex.Replace(value, @"\\x([0-9A-Fa-f]{2})", m =>
            {
                try
                {
                    return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString();
                }
                catch
                {
                    return m.Value;
                }
            });
        }

        /// <summary>
        /// Loose match: name starts with Stock ID then . / space / digit / ( / _ / -
        /// so odd Drive copies still attach to the stock.
        /// </summary>
        private static bool IsLooseStockImageMatch(string fileName, string stockId)
        {
            fileName = DecodeDriveNameEscapes((fileName ?? "").Trim());
            stockId = (stockId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(stockId))
                return false;
            if (!IsImageExtension(fileName))
                return false;
            if (!fileName.StartsWith(stockId, StringComparison.OrdinalIgnoreCase))
                return false;
            if (fileName.Length == stockId.Length)
                return false;
            char next = fileName[stockId.Length];
            return next == '.' || next == ' ' || next == '(' || next == '_' || next == '-'
                || char.IsDigit(next);
        }

        /// <summary>Optional upload helper (same as MVC). Mounting Stock UI does not call this.</summary>
        public static string UploadFile(Stream content, string fileName, string? contentType, string folderId)
        {
            if (content == null || content.Length <= 0)
                throw new InvalidOperationException("No file selected.");

            folderId = ExtractFolderId(folderId);
            if (string.IsNullOrWhiteSpace(folderId))
                throw new InvalidOperationException("Google Drive folder ID (AddJewlImages / AddJewlVideo) is missing in Setting Master.");

            var service = GetDriveService();

            var fileMetadata = new DriveFile
            {
                Name = Path.GetFileName(fileName),
                Parents = new List<string> { folderId }
            };

            string mimeType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType;

            var request = service.Files.Create(fileMetadata, content, mimeType);
            request.Fields = "id, name, webViewLink, webContentLink";
            request.SupportsAllDrives = true;

            var uploadProgress = request.Upload();
            if (uploadProgress.Status != UploadStatus.Completed)
            {
                string err = uploadProgress.Exception != null
                    ? uploadProgress.Exception.Message
                    : "Google Drive upload failed.";
                throw new InvalidOperationException(err);
            }

            var uploaded = request.ResponseBody;
            if (uploaded == null || string.IsNullOrEmpty(uploaded.Id))
                throw new InvalidOperationException("Google Drive did not return a file id.");

            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            var permRequest = service.Permissions.Create(permission, uploaded.Id);
            permRequest.SupportsAllDrives = true;
            permRequest.Execute();

            return "https://drive.google.com/file/d/" + uploaded.Id + "/view";
        }

        /// <summary>
        /// Metadata for progressive streaming (Content-Length helps video start sooner).
        /// </summary>
        public sealed class DriveFileMeta
        {
            public string ContentType { get; set; } = "application/octet-stream";
            public long? Size { get; set; }
        }

        /// <summary>
        /// Reads mime type / size without downloading the file body.
        /// </summary>
        public static DriveFileMeta GetFileMeta(string fileIdOrPath)
        {
            var meta = new DriveFileMeta();
            string fileId = ExtractFileId(fileIdOrPath);
            if (string.IsNullOrWhiteSpace(fileId))
                throw new InvalidOperationException("Invalid Google Drive file id.");

            if (HasCredentials())
            {
                try
                {
                    var service = GetDriveService();
                    var metaReq = service.Files.Get(fileId);
                    metaReq.Fields = "id, mimeType, name, size";
                    metaReq.SupportsAllDrives = true;
                    var file = metaReq.Execute();
                    if (!string.IsNullOrWhiteSpace(file.MimeType))
                        meta.ContentType = file.MimeType;
                    if (file.Size.HasValue)
                        meta.Size = file.Size.Value;
                    return meta;
                }
                catch
                {
                    // fall through
                }
            }

            return meta;
        }

        /// <summary>
        /// Pipes Drive file bytes to <paramref name="output"/> as they arrive (no full-file buffer).
        /// </summary>
        public static async Task DownloadFileToStreamAsync(
            string fileIdOrPath,
            Stream output,
            CancellationToken cancellationToken = default)
        {
            string fileId = ExtractFileId(fileIdOrPath);
            if (string.IsNullOrWhiteSpace(fileId))
                throw new InvalidOperationException("Invalid Google Drive file id.");

            if (HasCredentials())
            {
                try
                {
                    var service = GetDriveService();
                    var getReq = service.Files.Get(fileId);
                    getReq.SupportsAllDrives = true;
                    await getReq.DownloadAsync(output, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch
                {
                    // fall through to public URL
                }
            }

            string[] urls =
            {
                "https://drive.google.com/uc?export=download&id=" + fileId,
                "https://docs.google.com/uc?export=download&id=" + fileId
            };

            Exception? last = null;
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromMinutes(10);
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            foreach (var url in urls)
            {
                try
                {
                    using var response = await http.GetAsync(
                        url,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    await using var stream = await response.Content
                        .ReadAsStreamAsync(cancellationToken)
                        .ConfigureAwait(false);

                    var buffer = new byte[64];
                    int read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                        .ConfigureAwait(false);
                    if (read <= 0)
                        continue;

                    string head = System.Text.Encoding.UTF8.GetString(buffer, 0, read).TrimStart();
                    if (head.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
                        || head.StartsWith("<html", StringComparison.OrdinalIgnoreCase)
                        || head.IndexOf("accounts.google", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new InvalidOperationException(
                            "Drive file is private. Share → Anyone with the link → Viewer. File ID: " + fileId);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    await stream.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }

            throw new InvalidOperationException(
                "Google Drive stream failed. Share file as Anyone with the link, "
                + "OR place service-account JSON and share folder with that email. "
                + (last != null ? last.Message : ""));
        }

        /// <summary>
        /// Streams image bytes from Google Drive. Does NOT save to disk.
        /// Tries service account first; falls back to public download URL.
        /// </summary>
        public static byte[] DownloadFileBytes(string fileIdOrPath, out string contentType)
        {
            contentType = "image/jpeg";
            string fileId = ExtractFileId(fileIdOrPath);
            if (string.IsNullOrWhiteSpace(fileId))
                throw new InvalidOperationException("Invalid Google Drive file id.");

            if (HasCredentials())
            {
                try
                {
                    var service = GetDriveService();
                    var metaReq = service.Files.Get(fileId);
                    metaReq.Fields = "id, mimeType, name";
                    metaReq.SupportsAllDrives = true;
                    var meta = metaReq.Execute();
                    if (!string.IsNullOrWhiteSpace(meta.MimeType))
                        contentType = meta.MimeType;

                    using (var ms = new MemoryStream())
                    {
                        var getReq = service.Files.Get(fileId);
                        getReq.SupportsAllDrives = true;
                        getReq.Download(ms);
                        return ms.ToArray();
                    }
                }
                catch
                {
                    // fall through to public URL
                }
            }

            string[] urls =
            {
                "https://drive.google.com/uc?export=download&id=" + fileId,
                "https://docs.google.com/uc?export=download&id=" + fileId
            };

            Exception last = null;
            foreach (var url in urls)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.AllowAutoRedirect = true;
                    request.UserAgent = "Mozilla/5.0";
                    request.Timeout = 60000;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var ms = new MemoryStream())
                    {
                        if (stream == null)
                            continue;

                        stream.CopyTo(ms);
                        var bytes = ms.ToArray();

                        if (bytes.Length < 200)
                            continue;
                        string head = System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 64)).TrimStart();
                        if (head.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
                            || head.StartsWith("<html", StringComparison.OrdinalIgnoreCase)
                            || head.IndexOf("accounts.google", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new InvalidOperationException(
                                "Drive file is private. Share → Anyone with the link → Viewer. File ID: " + fileId);
                        }

                        if (!string.IsNullOrWhiteSpace(response.ContentType)
                            && response.ContentType.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            contentType = response.ContentType.Split(';')[0].Trim();
                        }

                        return bytes;
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }

            throw new InvalidOperationException(
                "Google Drive image load failed. Share file as Anyone with the link, " +
                "OR place service-account JSON and share folder with that email. "
                + (last != null ? last.Message : ""));
        }

        /// <summary>
        /// Resolve file name from a publicly shared Drive folder listing.
        /// folderStatus: ok | private | empty | error
        /// </summary>
        private static string LookupPublicFolderFileId(string folderId, string fileName, out string folderStatus)
        {
            folderStatus = "error";
            try
            {
                string status;
                var map = GetPublicFolderNameMap(folderId, false, out status);
                folderStatus = status;
                string id = FindNameInMap(map, fileName);
                if (!string.IsNullOrWhiteSpace(id))
                {
                    folderStatus = "ok";
                    return id;
                }

                // Refresh only when cache empty/failed — avoid re-scraping Drive HTML on every miss
                if (map != null && map.Count > 0
                    && string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                map = GetPublicFolderNameMap(folderId, true, out status);
                folderStatus = status;
                id = FindNameInMap(map, fileName);
                if (!string.IsNullOrWhiteSpace(id))
                    folderStatus = "ok";
                return id;
            }
            catch
            {
                folderStatus = "error";
                return null;
            }
        }

        private static string FindNameInMap(Dictionary<string, string> map, string fileName)
        {
            if (map == null || map.Count == 0 || string.IsNullOrWhiteSpace(fileName))
                return null;

            fileName = DecodeDriveNameEscapes(fileName.Trim());

            string id;
            if (map.TryGetValue(fileName, out id) && IsUsableFileMapId(id))
                return id;

            var pair = map.FirstOrDefault(kv =>
                string.Equals(DecodeDriveNameEscapes(kv.Key), fileName, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(pair.Key) && IsUsableFileMapId(pair.Value))
                return pair.Value;

            return null;
        }

        private static Dictionary<string, string> GetPublicFolderNameMap(string folderId, bool forceRefresh, out string status)
        {
            lock (PublicFolderCacheLock)
            {
                DateTime cachedAt;
                Dictionary<string, string> cached;
                string cachedStatus;
                if (!forceRefresh
                    && PublicFolderCache.TryGetValue(folderId, out cached)
                    && PublicFolderCacheAt.TryGetValue(folderId, out cachedAt)
                    && PublicFolderStatusCache.TryGetValue(folderId, out cachedStatus)
                    && (DateTime.UtcNow - cachedAt).TotalMinutes < 2
                    && cached != null)
                {
                    status = cachedStatus ?? "ok";
                    return cached;
                }
            }

            string fetchStatus;
            var map = FetchPublicFolderNameMap(folderId, out fetchStatus);

            lock (PublicFolderCacheLock)
            {
                PublicFolderCache[folderId] = map ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                PublicFolderCacheAt[folderId] = DateTime.UtcNow;
                PublicFolderStatusCache[folderId] = fetchStatus ?? "error";
                status = PublicFolderStatusCache[folderId];
                return PublicFolderCache[folderId];
            }
        }

        private static Dictionary<string, string> FetchPublicFolderNameMap(string folderId, out string status)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            status = "error";
            string html = null;

            string[] urls =
            {
                "https://drive.google.com/drive/folders/" + folderId + "?usp=sharing",
                "https://drive.google.com/drive/folders/" + folderId
            };

            foreach (var url in urls)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.AllowAutoRedirect = true;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
                    request.Timeout = 30000;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream ?? Stream.Null))
                    {
                        html = reader.ReadToEnd();
                    }
                    if (!string.IsNullOrEmpty(html) && html.Length > 1000)
                        break;
                }
                catch { }
            }

            if (string.IsNullOrEmpty(html))
            {
                status = "error";
                return map;
            }

            // Private / login wall — no file listing available
            bool looksPrivate =
                html.IndexOf("accounts.google", StringComparison.OrdinalIgnoreCase) >= 0
                || html.IndexOf("Sign in", StringComparison.OrdinalIgnoreCase) >= 0
                || html.IndexOf("Request access", StringComparison.OrdinalIgnoreCase) >= 0
                || html.IndexOf("You need access", StringComparison.OrdinalIgnoreCase) >= 0;

            // data-id + tooltip (Image / Video / File)
            foreach (Match m in Regex.Matches(
                html,
                @"data-id\s*=\s*""([a-zA-Z0-9_-]{20,})""[^>]*data-tooltip\s*=\s*""([^""]+?)(?: Image| Video| File)?""",
                RegexOptions.IgnoreCase))
            {
                string id = m.Groups[1].Value;
                string name = DecodeDriveNameEscapes(WebUtility.HtmlDecode(m.Groups[2].Value)).Trim();
                if (IsUsableFileMapId(id) && !string.IsNullOrWhiteSpace(name) && name.IndexOf('.') >= 0)
                    map[name] = id;
            }

            // aria-label="STD1001.mp4 Video Shared" with nearby data-id / ssk id
            foreach (Match m in Regex.Matches(
                html,
                @"aria-label\s*=\s*""([^""]+?\.(?:png|jpe?g|gif|bmp|webp|mp4|mov|avi|mkv|webm|pdf))[^""]*""[^>]*ssk\s*=\s*'[^:]+:[^:]+:([a-zA-Z0-9_-]{20,})-",
                RegexOptions.IgnoreCase))
            {
                string name = DecodeDriveNameEscapes(WebUtility.HtmlDecode(m.Groups[1].Value)).Trim();
                string id = m.Groups[2].Value;
                if (IsUsableFileMapId(id) && !string.IsNullOrWhiteSpace(name))
                    map[name] = id;
            }

            // Embedded JSON: "FILEID",["FOLDERID"],"Name.ext","mime"  (name may use \x20 / \x28 escapes)
            string escapedFolder = Regex.Escape(folderId);
            foreach (Match m in Regex.Matches(
                html,
                @"\\x22(" + @"[a-zA-Z0-9_-]{20,}" + @")\\x22,\\x5b\\x22" + escapedFolder + @"\\x22\\x5d,\\x22((?:[^\\]|\\x[0-9A-Fa-f]{2})+?)\\x22",
                RegexOptions.IgnoreCase))
            {
                string id = m.Groups[1].Value;
                string name = DecodeDriveNameEscapes(m.Groups[2].Value).Trim();
                if (IsUsableFileMapId(id) && !string.IsNullOrWhiteSpace(name))
                    map[name] = id;
            }

            foreach (Match m in Regex.Matches(
                html,
                @"""([a-zA-Z0-9_-]{20,})"",\s*\[\s*""" + escapedFolder + @"""\s*\],\s*""([^""]+?)""",
                RegexOptions.IgnoreCase))
            {
                string id = m.Groups[1].Value;
                string name = DecodeDriveNameEscapes(m.Groups[2].Value).Trim();
                if (IsUsableFileMapId(id) && !string.IsNullOrWhiteSpace(name))
                    map[name] = id;
            }

            if (map.Count > 0)
                status = "ok";
            else if (looksPrivate || !html.Contains("data-id="))
                status = "private";
            else
                status = "empty";

            return map;
        }

        private static bool IsUsableFileMapId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return ExtractFileId(id) != null || Regex.IsMatch(id.Trim(), @"^[a-zA-Z0-9_-]{20,}$");
        }

        private static string GuessMimeType(string fileName)
        {
            string ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            switch (ext)
            {
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".bmp": return "image/bmp";
                case ".mp4": return "video/mp4";
                case ".pdf": return "application/pdf";
                default: return "image/jpeg";
            }
        }

        private static bool IsImageExtension(string fileName)
        {
            string ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" }.Contains(ext);
        }

        private static bool IsVideoExtension(string fileName)
        {
            string ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            return new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" }.Contains(ext);
        }

        private static DriveService GetDriveService()
        {
            string credentialPath = GetCredentialPath();
            if (string.IsNullOrEmpty(credentialPath))
            {
                throw new InvalidOperationException(
                    "Google Drive credentials not found. Place service-account JSON at App_Data/GoogleDriveServiceAccount.json " +
                    "and share the JewlImages folder with the service account email (Editor).");
            }

            GoogleCredential credential;
            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.Drive);
            }

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Dia_CoreAPI"
            });
        }
    }
}
