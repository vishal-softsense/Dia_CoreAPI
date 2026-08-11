$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$c = Get-Content $dest -Raw

$header = @'
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

$c = $c -replace '(?s)^.*?public class ImportGradingController : Controller\s*\{', $header

# Remove session redirect wrappers
$c = $c -replace '(?s)\s*if \(Session\["UserId"\] == null\)\s*\{\s*return RedirectToAction\("Login", "Login"\);\s*\}\s*else\s*\{\s*', "`r`n                    "

# Remove permission gates (insert/edit/delete variants)
$actions = @('insert','edit','delete')
foreach ($a in $actions) {
    $c = $c -replace "(?s)\s*int permission = CheckUserPermission\(Session\[`"UserTypeId`"\]\.ToString\(\), `"Import Grading`", `"$a`"\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*", "`r`n                    "
}

# Remove else unauthorized returns
$c = $c -replace '(?s)\s*else\s*return PartialView\("Error404"\);\s*//return Json[^\}]*\}\s*', "`r`n                    "
$c = $c -replace '(?s)\s*else\s*\{\s*return Json\(new \{ success = false, Data = "You are not authorized[^}]+\}\)[^;]*;\s*\}\s*', "`r`n                    "
$c = $c -replace '(?s)\s*else\s*return Json\(new \{ success = false, Data = "You are not authorized[^}]+\}\)[^;]*;', ''

# ImportGradingData - not used in processor (handled by service)
$c = $c -replace '(?s)\[Route\("data/importgrading"\)\]\s*public ActionResult ImportGradingData\(\)\s*\{.*?\}\s*', ''

# Attributes and signatures
$c = $c -replace '\s*\[HttpPost\]\s*', ''
$c = $c -replace '\s*\[Route\("[^"]+"\)\]\s*', ''
$c = $c -replace 'public ActionResult (\w+)\(ImportGrading ImportData\)', 'public async Task<ImportGradingResult> ${1}Async(ImportGradingDTO ImportData, string companyId, string userId)'
$c = $c -replace 'public ActionResult UploadAndUpdateCompanyLoginLogo\(HttpPostedFileBase image\)', 'public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync(Microsoft.AspNetCore.Http.IFormFile image, string companyId, string webRootPath)'
$c = $c -replace 'public ActionResult UpdateSizeIds\(\)', 'public async Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId)'
$c = $c -replace 'public ActionResult DeleteDeletedRecords\(\)', 'public async Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId)'

# Json returns
$c = $c -replace 'return Json\(new \{ success = (true|false), Data = ([^}]+(?:\{[^}]*\}[^}]*)*)\ }, JsonRequestBehavior\.AllowGet\);', 'return new ImportGradingResult($1, $2);'
$c = $c -replace 'return Json\(new \{ success = (true|false), message = ([^,}]+), path = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2, $3);'
$c = $c -replace 'return Json\(new \{ success = (true|false), message = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2);'
$c = $c -replace 'return Json\(new\s*\{\s*success = (true|false),\s*Data = ([^}]+)\s*\}\);', 'return new ImportGradingResult($1, $2);'

# File upload
$c = $c -replace 'HttpPostedFileBase file = ImportData\.ImportFile;', @'
var importFile = ImportData.ImportFile;
                    if (importFile == null) return new ImportGradingResult(false, "No file uploaded.");
                    var file = importFile;
'@
$c = $c -replace 'HttpPostedFileBase file = ImportData\.ImportFile;\s*\r?\n\s*DataTable dt', @'
var importFile = ImportData.ImportFile;
                    if (importFile == null) return new ImportGradingResult(false, "No file uploaded.");
                    DataTable dt
'@
$c = $c -replace 'ReadFromExcel\(file\)', 'ReadFromExcel(importFile.OpenReadStream())'

# Data access
$c = $c -replace 'SqlDataAccess\.', '_repository.'
$c = $c -replace 'using \(SqlTempAccess sql = new SqlTempAccess\(\)\)', 'using (var sql = await CreateSqlTempAsync(companyId))'
$c = $c -replace 'Convert\.ToInt32\(Session\["CompanyId"\]\)', 'int.Parse(companyId)'
$c = $c -replace 'int companyid = Convert\.ToInt32\(Session\["CompanyId"\]\);', 'int companyid = int.Parse(companyId);'
$c = $c -replace 'int CompanyID = Convert\.ToInt32\(Session\["CompanyId"\]\);', 'int CompanyID = int.Parse(companyId);'
$c = $c -replace 'Convert\.ToInt32\(Session\["UserId"\]\)', 'int.Parse(userId)'
$c = $c -replace 'Session\["UserId"\]\.ToString\(\)', 'userId'
$c = $c -replace 'var SettingMaster = \(List<SettingMaster>\)Session\["SettingMaster"\];', 'var settingMaster = await _repository.GetSettingMasterListAsync(companyId);'
$c = $c -replace 'SettingMaster\.Where', 'settingMaster.Where'
$c = $c -replace 'ItemMasterProcessor\.UpdateTable', '_repository.UpdateAskingPurchaseRateAsync'
$c = $c -replace 'BasicFunctionController\.GetConvertAmount', '_repository.GetConvertAmountAsync'

# Logo upload
$c = $c -replace 'if \(image == null \|\| image\.ContentLength == 0\)', 'if (image == null || image.Length == 0)'
$c = $c -replace 'Server\.MapPath\("~/" \+ relativeFolder\)', 'Path.Combine(webRootPath, relativeFolder)'
$c = $c -replace 'image\.SaveAs\(fullPath\);', @'
using (var fs = new FileStream(fullPath, FileMode.Create)) { await image.CopyToAsync(fs); }
'@

# ReadFromExcel
$c = $c -replace 'public DataTable ReadFromExcel\(HttpPostedFileBase file\)', 'public static DataTable ReadFromExcel(Stream stream)'
$c = $c -replace 'Stream stream = file\.InputStream;\s*', ''
$c = $c -replace 'ExcelReaderFactory\.CreateOpenXmlReader\(stream\)', 'ExcelReaderFactory.CreateReader(stream)'

# Types
$c = $c -replace 'ImportGrading ImportData', 'ImportGradingDTO ImportData'
$c = $c -replace 'List<GradingList>', 'List<GradingListDto>'

# Repository async names + companyId
$c = $c -replace '_repository\.UpdateMultiGradingStatus\(', '_repository.UpdateMultiGradingStatusAsync('
$c = $c -replace '_repository\.SelectMultiGrading\(', '_repository.SelectMultiGradingAsync('
$c = $c -replace '_repository\.GetSize\(', '_repository.GetSizeAsync('
$c = $c -replace '_repository\.UpdateImageVideoPath\(', '_repository.UpdateImageVideoPathAsync('
$c = $c -replace '_repository\.UpdateSizeIds\(\)', '_repository.UpdateSizeIdsAsync(companyId)'
$c = $c -replace '_repository\.DelRecords\(', '_repository.DelRecordsAsync('
$c = $c -replace '_repository\.UpdateCompanyLoginLogo\(', '_repository.UpdateCompanyLoginLogoAsync('
$c = $c -replace '_repository\.GetLotExist\(', '_repository.GetLotExistAsync('
$c = $c -replace '_repository\.GetMCurOperator\(', '_repository.GetMCurOperatorAsync('
$c = $c -replace '_repository\.GetSCurOperator\(', '_repository.GetSCurOperatorAsync('
$c = $c -replace '_repository\.GetMCurID\(\)', '_repository.GetMCurIdAsync(companyId)'
$c = $c -replace '_repository\.GetSCurID\(\)', '_repository.GetSCurIdAsync(companyId)'
$c = $c -replace '_repository\.UpdateAskingPurchaseRateAsync\(', '_repository.UpdateAskingPurchaseRateAsync('

$c = $c -replace 'UpdateMultiGradingStatusAsync\(([^,]+),\s*"Update"\)', 'UpdateMultiGradingStatusAsync($1, "Update", companyId)'
$c = $c -replace 'SelectMultiGradingAsync\(([^,]+),\s*"Select"\)', 'SelectMultiGradingAsync($1, "Select", companyId)'
$c = $c -replace 'GetSizeAsync\(([^,]+),\s*"GetSize"\)', 'GetSizeAsync($1, "GetSize", companyId)'
$c = $c -replace 'GetConvertAmountAsync\(([^)]+)\)', 'GetConvertAmountAsync($1, companyId)'
$c = $c -replace 'DelRecordsAsync\("DeleteFlag = 1"\)', 'DelRecordsAsync("DeleteFlag = 1", companyId)'
$c = $c -replace 'UpdateCompanyLoginLogoAsync\(([^,]+),\s*"UpdateLogo"\)', 'UpdateCompanyLoginLogoAsync($1, "UpdateLogo", companyId)'
$c = $c -replace 'UpdateAskingPurchaseRateAsync\(([^,]+),\s*([^,]+),\s*([^)]+)\)', 'UpdateAskingPurchaseRateAsync($1, $2, $3, companyId)'

# SaveItemMasterData / SaveAccGrpMasterData -> repository
$c = $c -replace '\bSaveItemMasterData\(', 'await _repository.SaveItemMasterData('
$c = $c -replace '\bSaveAccGrpMasterData\(', 'await _repository.SaveAccGrpMasterData('

# Remove BackupDatabase endpoint block
$c = $c -replace '(?s)\[HttpPost\][^\{]*BackupDatabase\(\)[\s\S]*$', ''

# Close class
if ($c -notmatch '\}\s*$') { $c = $c.TrimEnd() + "`r`n    }`r`n}`r`n" }

Set-Content -Path $dest -Value $c -Encoding UTF8
Write-Host "Fresh port done. CheckUserPermission left:" ([regex]::Matches($c,'CheckUserPermission')).Count
