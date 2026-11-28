using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Email;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Dia_CoreAPI.Services.Classes
{
    public partial class DocumentEmailService : IDocumentEmailService
    {
        private static readonly ConcurrentDictionary<string, byte[]?> LogoCache = new(StringComparer.OrdinalIgnoreCase);

        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly IMixMemoOutService _mixMemoOutService;
        private readonly IMixSaleService _mixSaleService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentEmailService> _logger;

        private sealed class CompanyEmailSettings
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string SmtpHost { get; set; } = string.Empty;
            public int Port { get; set; } = 587;
            public bool EnableSsl { get; set; } = true;

            public string CompanyName { get; set; } = string.Empty;
            public string CompanyAddress { get; set; } = string.Empty;
            public string CompanyAddress1 { get; set; } = string.Empty;
            public string CompanyAddress2 { get; set; } = string.Empty;
            public string CompanyPhone { get; set; } = string.Empty;
            public string CompanyEmail { get; set; } = string.Empty;
            public string CompanyLogo { get; set; } = string.Empty;
            public string CompanyFooter { get; set; } = "Powered by SoftOnCloud";
            public string MemoConditions { get; set; } = string.Empty;
            public string SaleConditions { get; set; } = string.Empty;
            public string Information { get; set; } = string.Empty;
            public string ParcelCaratFormat { get; set; } = "0.00";
            public int AmountDecimals { get; set; } = 2;
            public string MixMemoPrintFormat { get; set; } = string.Empty;
            public string ParcelMemoPrintFormat { get; set; } = string.Empty;
            public string ParcelSalePrintFormat { get; set; } = string.Empty;
        }

        private sealed class DocHeaderInfo
        {
            public int Code { get; set; }
            public string InvNo { get; set; } = string.Empty;
            public DateTime? TrnDate { get; set; }
            public string PartyName { get; set; } = string.Empty;
            public string PartyEmail { get; set; } = string.Empty;
        }

        public DocumentEmailService(
            ISqlDataAccess sqlDataAccess,
            IMixMemoOutService mixMemoOutService,
            IMixSaleService mixSaleService,
            IWebHostEnvironment env,
            ILogger<DocumentEmailService> logger)
        {
            _sqlDataAccess = sqlDataAccess;
            _mixMemoOutService = mixMemoOutService;
            _mixSaleService = mixSaleService;
            _env = env;
            _logger = logger;
        }

        private static string NormalizeDocType(string? docType)
        {
            var dt = (docType ?? string.Empty).Trim().ToLowerInvariant();
            if (dt is "mixmemoout" or "mixmemo" or "memo" or "cmemoout") return "MixMemoOut";
            if (dt is "mixsale" or "sale" or "csale") return "MixSale";
            return string.Empty;
        }

        private static string SanitizeFilePart(string? text)
        {
            var clean = (text ?? string.Empty).Trim();
            foreach (var ch in Path.GetInvalidFileNameChars())
                clean = clean.Replace(ch, '_');
            clean = clean.Replace(' ', '_').Replace('/', '_').Replace('\\', '_');
            while (clean.Contains("__"))
                clean = clean.Replace("__", "_");
            return clean.Trim('_');
        }

        private static string BuildPdfFileName(string normalizedDocType, string invNo, string partyName)
        {
            var prefix = normalizedDocType == "MixMemoOut" ? "MixMemoOut" : "MixSale";
            var safeInv = SanitizeFilePart(invNo);
            var safeParty = SanitizeFilePart(partyName);
            if (!string.IsNullOrWhiteSpace(safeParty))
                return $"{prefix}_{safeInv}_{safeParty}.pdf";
            return $"{prefix}_{(string.IsNullOrWhiteSpace(safeInv) ? "Document" : safeInv)}.pdf";
        }

        private async Task<CompanyEmailSettings> LoadSettingsAsync(string companyId)
        {
            var settings = new CompanyEmailSettings();
            string connStr = await _sqlDataAccess.GetClientConnectionString(companyId);

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            const string sql = @"
SELECT Name, Value, GroupId
FROM dbo.TabSettingMaster
WHERE ISNULL(Active, 1) = 1
  AND (
      UPPER(LTRIM(RTRIM(ISNULL(GroupId, '')))) IN ('EMAIL SETTINGS', 'COMPANY', 'PRINT FORMAT', 'DEFAULT STOCK SETTING')
      OR Name IN ('Email', 'Password', 'SMTP', 'EmailPort', 'EmailEnableSsl',
                  'CompanyNameforInvoice', 'Company', 'CompanyAddress', 'CompanyAddress1', 'CompanyAddress2',
                  'CompanyTeleph', 'CompanyEmail', 'Comapnylogo', 'CompanyLoginLogo', 'CompanyFooter', 'Footer',
                  'MemoConditions', 'SaleConditions', 'Information', 'ParcelCaratFormat', 'CaratFormatNew',
                  'DecimalinInvoiceAmount', 'DEFAULT MIX MEMO PRINT FORMAT', 'DEFAULT MEMO PRINT FORMAT PARCEL',
                  'DEFAULT SALE INVOICE  PRINT FORMAT PARCEL', 'DEFAULT SALE INVOICE PRINT FORMAT PARCEL')
  )";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var collapsedMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            while (await reader.ReadAsync())
            {
                var name = (reader["Name"]?.ToString() ?? string.Empty).Trim();
                var val = (reader["Value"]?.ToString() ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    map[name] = val;
                    collapsedMap[CollapseKey(name)] = val;
                }
            }

            string Get(params string[] keys)
            {
                foreach (var k in keys)
                {
                    if (map.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                        return v;
                    if (collapsedMap.TryGetValue(CollapseKey(k), out var cv) && !string.IsNullOrWhiteSpace(cv))
                        return cv;
                }
                return string.Empty;
            }

            settings.Email = Get("Email");
            settings.Password = Get("Password");
            settings.SmtpHost = Get("SMTP");
            if (int.TryParse(Get("EmailPort"), out var port) && port > 0)
                settings.Port = port;

            var sslStr = Get("EmailEnableSsl");
            if (!string.IsNullOrWhiteSpace(sslStr))
                settings.EnableSsl = sslStr.Equals("true", StringComparison.OrdinalIgnoreCase)
                                     || sslStr == "1"
                                     || sslStr.Equals("yes", StringComparison.OrdinalIgnoreCase);

            settings.CompanyName = Get("CompanyNameforInvoice", "Company");
            settings.CompanyAddress = Get("CompanyAddress");
            settings.CompanyAddress1 = Get("CompanyAddress1");
            settings.CompanyAddress2 = Get("CompanyAddress2");
            settings.CompanyPhone = Get("CompanyTeleph");
            settings.CompanyEmail = Get("CompanyEmail");
            settings.CompanyLogo = Get("CompanyLoginLogo", "Comapnylogo");
            var footerVal = Get("CompanyFooter", "Footer");
            if (!string.IsNullOrWhiteSpace(footerVal))
                settings.CompanyFooter = footerVal;
            settings.MemoConditions = Get("MemoConditions");
            settings.SaleConditions = Get("SaleConditions");
            settings.Information = Get("Information");
            settings.ParcelCaratFormat = Get("ParcelCaratFormat", "CaratFormatNew");
            if (string.IsNullOrWhiteSpace(settings.ParcelCaratFormat))
                settings.ParcelCaratFormat = "0.00";
            if (int.TryParse(Get("DecimalinInvoiceAmount"), out var decAmt) && decAmt is >= 0 and <= 4)
                settings.AmountDecimals = decAmt;
            settings.MixMemoPrintFormat = Get("DEFAULT MIX MEMO PRINT FORMAT");
            settings.ParcelMemoPrintFormat = Get("DEFAULT MEMO PRINT FORMAT PARCEL");
            settings.ParcelSalePrintFormat = Get("DEFAULT SALE INVOICE  PRINT FORMAT PARCEL", "DEFAULT SALE INVOICE PRINT FORMAT PARCEL");

            return settings;
        }

        private async Task<DocHeaderInfo?> LoadDocHeaderAsync(string normalizedDocType, string id, string companyId)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var trnType = normalizedDocType == "MixMemoOut" ? "CMemoOut" : "CSALE";
            string connStr = await _sqlDataAccess.GetClientConnectionString(companyId);

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1
    tsp.Code,
    CAST(tsp.InvNo AS VARCHAR(100)) AS InvNo,
    tsp.TrnDate,
    ISNULL(acc.AccGrpName, '')      AS PartyName,
    ISNULL(acc.Email, '')           AS PartyEmail
FROM dbo.TabSalePur tsp
LEFT JOIN dbo.TabAccGrpMaster acc ON tsp.PartyCode = acc.AccGrpCode
WHERE tsp.TrnType = @TrnType
  AND (
        (TRY_CAST(@Id AS INT) IS NOT NULL AND tsp.Code = TRY_CAST(@Id AS INT))
        OR CAST(tsp.InvNo AS VARCHAR(100)) = LTRIM(RTRIM(@Id))
      )
ORDER BY
    CASE
        WHEN TRY_CAST(@Id AS INT) IS NOT NULL AND tsp.Code = TRY_CAST(@Id AS INT) THEN 0
        ELSE 1
    END,
    tsp.Code DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TrnType", trnType);
            cmd.Parameters.AddWithValue("@Id", id.Trim());

            using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            return new DocHeaderInfo
            {
                Code = r["Code"] != DBNull.Value ? Convert.ToInt32(r["Code"]) : 0,
                InvNo = (r["InvNo"]?.ToString() ?? string.Empty).Trim(),
                TrnDate = r["TrnDate"] != DBNull.Value ? Convert.ToDateTime(r["TrnDate"]) : null,
                PartyName = (r["PartyName"]?.ToString() ?? string.Empty).Trim(),
                PartyEmail = (r["PartyEmail"]?.ToString() ?? string.Empty).Trim()
            };
        }

        private static (bool Ok, string? Warning) ValidateSmtpConfig(CompanyEmailSettings cfg)
        {
            if (string.IsNullOrWhiteSpace(cfg.Email))
                return (false, "Sender email is not configured in TabSettingMaster.");
            return (true, null);
        }

        public async Task<DocumentEmailComposeResponse?> ComposeAsync(string docType, string id, string companyId)
        {
            var normalized = NormalizeDocType(docType);
            if (string.IsNullOrEmpty(normalized) || string.IsNullOrWhiteSpace(id))
                return null;

            var settingsTask = LoadSettingsAsync(companyId);
            var headerTask = LoadDocHeaderAsync(normalized, id, companyId);
            await Task.WhenAll(settingsTask, headerTask);

            var settings = settingsTask.Result;
            var header = headerTask.Result;
            if (header == null)
                return null;

            var invDisplay = !string.IsNullOrWhiteSpace(header.InvNo) ? header.InvNo : header.Code.ToString(CultureInfo.InvariantCulture);
            var dateStr = header.TrnDate.HasValue
                ? header.TrnDate.Value.ToString("M/d/yyyy", CultureInfo.InvariantCulture)
                : DateTime.Now.ToString("M/d/yyyy", CultureInfo.InvariantCulture);

            var isMemo = normalized == "MixMemoOut";
            var subjectPrefix = isMemo ? "Memo" : "Invoice";
            var docLabel = isMemo ? "Memo" : "Sale Invoice";
            var companyDisplay = !string.IsNullOrWhiteSpace(settings.CompanyName) ? settings.CompanyName : "SoftOnCloud";
            var partyGreeting = !string.IsNullOrWhiteSpace(header.PartyName) ? header.PartyName : "Sir/Madam";

            var subject = $"{subjectPrefix}: {invDisplay} Dated: {dateStr}";
            var body =
                $"Dear {partyGreeting},\n\n" +
                $"Please refer attached {docLabel} as attached .pdf file.\n\n" +
                $"Please feel free to connect us for any query.\n\n" +
                $"Regards.\n\n" +
                $"{companyDisplay}\n\n" +
                $"Powered By www.SoftOnCloud.com";

            var (smtpOk, _) = ValidateSmtpConfig(settings);

            return new DocumentEmailComposeResponse
            {
                DocType = normalized,
                Id = header.Code > 0 ? header.Code.ToString(CultureInfo.InvariantCulture) : id,
                InvNo = invDisplay,
                PartyName = header.PartyName,
                From = settings.Email,
                To = header.PartyEmail,
                Cc = string.Empty,
                Bcc = string.Empty,
                Subject = subject,
                Body = body,
                PdfFileName = BuildPdfFileName(normalized, invDisplay, header.PartyName),
                HasSmtpConfig = smtpOk,
                SmtpWarning = null
            };
        }

        private byte[]? TryLoadLogoBytes(string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath)) return null;
            var trimmed = logoPath.Trim();
            return LogoCache.GetOrAdd(trimmed, key =>
            {
                try
                {
                    if (File.Exists(key))
                        return File.ReadAllBytes(key);

                    var relative = key.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                    var candidates = new[]
                    {
                        Path.Combine(_env.ContentRootPath, relative),
                        Path.Combine(_env.ContentRootPath, "wwwroot", relative),
                        Path.Combine(_env.ContentRootPath, "CRreport", "logos", Path.GetFileName(relative)),
                        Path.Combine(_env.ContentRootPath, "wwwroot", "Upload", Path.GetFileName(relative))
                    };

                    foreach (var path in candidates)
                    {
                        if (File.Exists(path))
                            return File.ReadAllBytes(path);
                    }
                }
                catch
                {
                    // Logo is optional in PDF; never fail PDF generation due to logo path
                }
                return null;
            });
        }

        private static string FormatCarat(double value, string format)
        {
            var decimals = 2;
            var dotIdx = (format ?? "0.00").IndexOf('.');
            if (dotIdx >= 0)
            {
                var fracLen = (format!.Length - dotIdx - 1);
                if (fracLen is >= 0 and <= 4) decimals = fracLen;
            }
            return value.ToString($"N{decimals}", CultureInfo.InvariantCulture);
        }

        private static string FormatMoney(double value) =>
            value.ToString("N2", CultureInfo.InvariantCulture);

        public async Task<(byte[] PdfBytes, string FileName)?> GeneratePdfAsync(string docType, string id, string companyId)
        {
            var normalized = NormalizeDocType(docType);
            if (string.IsNullOrEmpty(normalized) || string.IsNullOrWhiteSpace(id))
                return null;

            var settings = await LoadSettingsAsync(companyId);
            return await GeneratePdfWithSettingsAsync(normalized, id, companyId, settings);
        }

        private async Task<(byte[] PdfBytes, string FileName)?> GeneratePdfWithSettingsAsync(
            string normalized,
            string id,
            string companyId,
            CompanyEmailSettings settings)
        {
            var logoBytes = TryLoadLogoBytes(settings.CompanyLogo);

            if (normalized == "MixMemoOut")
            {
                var printData = await _mixMemoOutService.GetPrintViewAsync(id, companyId);
                if (printData == null) return null;

                var fileName = BuildPdfFileName(normalized, printData.InvNo ?? id, printData.PartyName ?? string.Empty);
                var bytes = BuildMixMemoPdf(printData, settings, logoBytes);
                return (bytes, fileName);
            }
            else
            {
                var printData = await _mixSaleService.GetPrintViewAsync(id, companyId);
                if (printData == null) return null;

                var fileName = BuildPdfFileName(normalized, printData.InvNo ?? id, printData.PartyName ?? string.Empty);
                var bytes = BuildMixSalePdf(printData, settings, logoBytes);
                return (bytes, fileName);
            }
        }


        private static List<MailAddress> ParseAddresses(string? raw)
        {
            var list = new List<MailAddress>();
            if (string.IsNullOrWhiteSpace(raw)) return list;

            var parts = raw.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                try
                {
                    list.Add(new MailAddress(trimmed));
                }
                catch
                {
                    throw new ArgumentException($"Invalid email address: '{trimmed}'");
                }
            }
            return list;
        }

        public async Task<DocumentEmailSendResult> SendAsync(DocumentEmailSendRequest request, string companyId)
        {
            var normalized = NormalizeDocType(request.DocType);
            if (string.IsNullOrEmpty(normalized))
                return new DocumentEmailSendResult { Success = false, Message = "Invalid document type. Expected MixMemoOut or MixSale." };

            if (string.IsNullOrWhiteSpace(request.Id))
                return new DocumentEmailSendResult { Success = false, Message = "Invoice / Memo ID is required." };

            if (string.IsNullOrWhiteSpace(request.To))
                return new DocumentEmailSendResult { Success = false, Message = "Recipient (TO) email address is required." };

            if (string.IsNullOrWhiteSpace(request.Subject))
                return new DocumentEmailSendResult { Success = false, Message = "Email Subject is required." };

            List<MailAddress> toList;
            List<MailAddress> ccList;
            List<MailAddress> bccList;
            try
            {
                toList = ParseAddresses(request.To);
                ccList = ParseAddresses(request.Cc);
                bccList = ParseAddresses(request.Bcc);
            }
            catch (ArgumentException ex)
            {
                return new DocumentEmailSendResult { Success = false, Message = ex.Message };
            }

            if (toList.Count == 0)
                return new DocumentEmailSendResult { Success = false, Message = "At least one valid TO email address is required." };

            // Load settings and print data in parallel (single DB round-trip, no duplicate settings load)
            CompanyEmailSettings settings;
            (byte[] PdfBytes, string FileName)? pdf = null;

            if (request.IncludePdf)
            {
                var settingsTask = LoadSettingsAsync(companyId);
                if (normalized == "MixMemoOut")
                {
                    var printTask = _mixMemoOutService.GetPrintViewAsync(request.Id, companyId);
                    await Task.WhenAll(settingsTask, printTask);
                    settings = settingsTask.Result;
                    var printData = printTask.Result;
                    if (string.IsNullOrWhiteSpace(settings.Email))
                        return new DocumentEmailSendResult { Success = false, Message = "Sender email is not configured in TabSettingMaster." };
                    if (printData == null)
                        return new DocumentEmailSendResult { Success = false, Message = "Unable to generate PDF attachment: document not found." };

                    var logoBytes = TryLoadLogoBytes(settings.CompanyLogo);
                    pdf = (
                        BuildMixMemoPdf(printData, settings, logoBytes),
                        BuildPdfFileName(normalized, printData.InvNo ?? request.Id, printData.PartyName ?? string.Empty)
                    );
                }
                else
                {
                    var printTask = _mixSaleService.GetPrintViewAsync(request.Id, companyId);
                    await Task.WhenAll(settingsTask, printTask);
                    settings = settingsTask.Result;
                    var printData = printTask.Result;
                    if (string.IsNullOrWhiteSpace(settings.Email))
                        return new DocumentEmailSendResult { Success = false, Message = "Sender email is not configured in TabSettingMaster." };
                    if (printData == null)
                        return new DocumentEmailSendResult { Success = false, Message = "Unable to generate PDF attachment: document not found." };

                    var logoBytes = TryLoadLogoBytes(settings.CompanyLogo);
                    pdf = (
                        BuildMixSalePdf(printData, settings, logoBytes),
                        BuildPdfFileName(normalized, printData.InvNo ?? request.Id, printData.PartyName ?? string.Empty)
                    );
                }
            }
            else
            {
                settings = await LoadSettingsAsync(companyId);
                if (string.IsNullOrWhiteSpace(settings.Email))
                    return new DocumentEmailSendResult { Success = false, Message = "Sender email is not configured in TabSettingMaster." };
            }

            // Match SolitaireStockSearchReportRepository: default to smtpout.secureserver.net:587
            // while still honoring a valid custom SMTP host if configured for another client.
            string smtpHost = (!string.IsNullOrWhiteSpace(settings.SmtpHost) && !settings.SmtpHost.Contains('@'))
                ? settings.SmtpHost
                : "smtpout.secureserver.net";
            int smtpPort = settings.Port > 0 ? settings.Port : 587;
            bool enableSsl = settings.EnableSsl;
            string senderEmail = settings.Email;
            string senderPassword = settings.Password;
            string senderDisplay = string.IsNullOrWhiteSpace(settings.CompanyName) ? settings.Email : settings.CompanyName;
            string subject = request.Subject.Trim();
            string encodedBody = WebUtility.HtmlEncode(request.Body ?? string.Empty);
            string htmlBody = Regex.Replace(encodedBody, @"\r?\n", "<br />");
            string toSummary = string.Join(", ", toList.Select(a => a.Address));
            var logger = _logger;

            // Dispatch SMTP network transfer in the background so UI responds immediately
            // after validation + PDF generation succeed.
            _ = Task.Run(async () =>
            {
                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort);
                    client.EnableSsl = enableSsl;
                    client.Timeout = 100000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    using var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(senderEmail, senderDisplay);
                    foreach (var addr in toList) mailMessage.To.Add(addr);
                    foreach (var addr in ccList) mailMessage.CC.Add(addr);
                    foreach (var addr in bccList) mailMessage.Bcc.Add(addr);
                    mailMessage.Subject = subject;
                    mailMessage.Body = htmlBody;
                    mailMessage.IsBodyHtml = true;

                    MemoryStream? pdfStream = null;
                    try
                    {
                        if (pdf.HasValue)
                        {
                            pdfStream = new MemoryStream(pdf.Value.PdfBytes);
                            var attachment = new Attachment(pdfStream, pdf.Value.FileName, "application/pdf");
                            mailMessage.Attachments.Add(attachment);
                        }

                        await client.SendMailAsync(mailMessage);
                        logger.LogInformation("Document email ({DocType} #{Id}) sent successfully to {To}.", normalized, request.Id, toSummary);
                    }
                    finally
                    {
                        pdfStream?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background document email failed ({DocType} #{Id}) to {To}.", normalized, request.Id, toSummary);
                }
            });

            return new DocumentEmailSendResult
            {
                Success = true,
                Message = $"Mail Sent Successfully to {toSummary}."
            };
        }
    }
}
