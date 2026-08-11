$src = 'd:\CoreUI\1.MVC FINAL\DiasenseSoftware\Controllers\ImportGradingController.cs'
$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$content = Get-Content $src -Raw

# Strip MVC usings through namespace open
$content = $content -replace '(?s)^using DevExpress.*?namespace DiasenseSoftware\.Controllers\s*\{\s*', @'
using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Helpers
{
'@

$content = $content -replace 'public class ImportGradingController : Controller', @'
    public class ImportGradingProcessor
    {
        private readonly IImportGradingRepository _repository;
        private readonly ISqlDataAccess _sqlDataAccess;

        public ImportGradingProcessor(IImportGradingRepository repository, ISqlDataAccess sqlDataAccess)
        {
            _repository = repository;
            _sqlDataAccess = sqlDataAccess;
        }

        private async Task<SqlTempAccess> CreateSqlTempAsync(string companyId)
        {
            var cs = await _sqlDataAccess.GetClientConnectionString(companyId);
            return new SqlTempAccess(cs);
        }
'@

# Remove attributes and fix method signatures for main endpoints
$content = $content -replace '\s*\[Route\("[^"]+"\)\]\s*', ''
$content = $content -replace '\s*\[HttpPost\]\s*', ''
$content = $content -replace 'public ActionResult ImportGradingData\(\)', 'public ImportGradingResult ImportGradingData()'
$content = $content -replace 'public ActionResult (\w+)\(ImportGrading ImportData\)', 'public async Task<ImportGradingResult> $1Async(ImportGradingDTO ImportData, string companyId, string userId)'
$content = $content -replace 'public ActionResult UploadAndUpdateCompanyLoginLogo\(HttpPostedFileBase image\)', 'public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync(Microsoft.AspNetCore.Http.IFormFile image, string companyId, string webRootPath)'
$content = $content -replace 'public ActionResult UpdateSizeIds\(\)', 'public async Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId)'
$content = $content -replace 'public ActionResult DeleteDeletedRecords\(\)', 'public async Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId)'

# Remove session/auth wrappers - ImportGradingData
$content = $content -replace '(?s)public ImportGradingResult ImportGradingData\(\)\s*\{\s*if \(Session\["UserId"\] == null\)\s*\{\s*return RedirectToAction\("Login", "Login"\);\s*\}\s*else\s*\{\s*int permission = CheckUserPermission\(Session\["UserTypeId"\]\.ToString\(\), "Import Grading", "insert"\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*MasterController\.PagesLoginLog\([^;]+;\s*;\s*return View\(\);\s*\}\s*else\s*return PartialView\("Error404"\);[^}]*\}\s*\}', 'public ImportGradingResult ImportGradingData() => new ImportGradingResult(true, "Add", null, "Add");'

# Generic session null + permission wrapper removal (keep inner body)
$content = $content -replace '(?s)if \(Session\["UserId"\] == null\)\s*\{\s*return RedirectToAction\("Login", "Login"\);\s*\}\s*else\s*\{\s*', ''
$content = $content -replace '(?s)int permission = CheckUserPermission\([^)]+\)\.Count\(\);\s*if \(permission == 1\)\s*\{', ''
$content = $content -replace '(?s)\}\s*else\s*return PartialView\("Error404"\);[^}]*//return Json[^}]*\}\s*', ''
$content = $content -replace '(?s)\}\s*else\s*\{\s*return Json\(new \{ success = false, Data = "You are not authorized[^}]+\}\)[^}]*\}\s*', ''
$content = $content -replace '(?s)\}\s*else\s*return Json\(new \{ success = false, Data = "You are not authorized[^}]+\}\)[^;]*;', ''

# Json returns
$content = $content -replace 'return Json\(new \{ success = (true|false), Data = ([^,}]+(?:\{[^}]*\}[^,}]*)*)\ }, JsonRequestBehavior\.AllowGet\);', 'return new ImportGradingResult($1, $2);'
$content = $content -replace 'return Json\(new \{ success = (true|false), message = ([^,}]+), path = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2, $3);'
$content = $content -replace 'return Json\(new \{ success = (true|false), message = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2);'
$content = $content -replace 'return Json\(new\s*\{', 'return new ImportGradingResult('

# Types
$content = $content -replace 'ImportGrading ImportData', 'ImportGradingDTO ImportData'
$content = $content -replace 'HttpPostedFileBase file = ImportData\.ImportFile;', @'
var importFile = ImportData.ImportFile;
                    if (importFile == null) return new ImportGradingResult(false, "No file uploaded.");
                    using var fileStream = importFile.OpenReadStream();
                    var file = importFile;
'@
$content = $content -replace 'HttpPostedFileBase file = ImportData\.ImportFile;\s*\n\s*DataTable dt', @'
var importFile = ImportData.ImportFile;
                    if (importFile == null) return new ImportGradingResult(false, "No file uploaded.");
                    using var fileStream = importFile.OpenReadStream();
                    DataTable dt
'@
$content = $content -replace 'ReadFromExcel\(file\)', 'ReadFromExcel(importFile.OpenReadStream())'
$content = $content -replace 'dt = ReadFromExcel\(importFile\.OpenReadStream\(\)\)', 'using (var stream = importFile.OpenReadStream()) { dt = ReadFromExcel(stream); }'

$content = $content -replace 'SqlDataAccess\.', '_repository.'
$content = $content -replace 'using \(SqlTempAccess sql = new SqlTempAccess\(\)\)', 'using (var sql = await CreateSqlTempAsync(companyId))'
$content = $content -replace 'Convert\.ToInt32\(Session\["CompanyId"\]\)', 'int.Parse(companyId)'
$content = $content -replace 'int companyid = Convert\.ToInt32\(Session\["CompanyId"\]\);', 'int companyid = int.Parse(companyId);'
$content = $content -replace 'int CompanyID = Convert\.ToInt32\(Session\["CompanyId"\]\);', 'int CompanyID = int.Parse(companyId);'

$content = $content -replace 'ItemMasterProcessor\.UpdateTable', '_repository.UpdateAskingPurchaseRate'

# Upload logo
$content = $content -replace 'Server\.MapPath\("~/" \+ relativeFolder\)', 'Path.Combine(webRootPath, relativeFolder)'
$content = $content -replace 'image\.SaveAs\(fullPath\);', @'
using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        await image.CopyToAsync(fs);
                    }
'@
$content = $content -replace 'if \(image == null \|\| image\.ContentLength == 0\)', 'if (image == null || image.Length == 0)'

# ReadFromExcel signature
$content = $content -replace 'public DataTable ReadFromExcel\(HttpPostedFileBase file\)', 'public static DataTable ReadFromExcel(Stream stream)'
$content = $content -replace 'Stream stream = file\.InputStream;', ''
$content = $content -replace 'IExcelDataReader excelReader = ExcelReaderFactory\.CreateOpenXmlReader\(stream\);', @'
IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(stream);
'@

# Remove BackupDatabase and commented code at end if present
$content = $content -replace '(?s)\[HttpPost\][^\{]*BackupDatabase\(\)[\s\S]*$', ''

Set-Content -Path $dest -Value $content -Encoding UTF8
Write-Host "Processor: $((Get-Item $dest).Length) bytes, lines: $((Get-Content $dest).Count)"
