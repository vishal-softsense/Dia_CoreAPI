$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
Copy-Item "d:\CoreUI\1.MVC FINAL\DiasenseSoftware\Controllers\ImportGradingController.cs" $dest -Force
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

# Session null -> unauthorized result (keep else structure)
$c = $c -replace 'return RedirectToAction\("Login", "Login"\);', 'return new ImportGradingResult(false, "Session expired");'
$c = $c -replace 'return PartialView\("Error404"\);', 'return new ImportGradingResult(false, "You are not authorized to perform this task.");'
$c = $c -replace 'return View\(\);', 'return new ImportGradingResult(true, null, null, null, "Add");'

# Permission checks
$c = $c -replace 'CheckUserPermission\(Session\["UserTypeId"\]\.ToString\(\), "Import Grading", "insert"\)\.Count\(\)', 'await _repository.CheckUserPermissionAsync(userTypeId, "Import Grading", "insert", companyId)'
$c = $c -replace 'CheckUserPermission\(Session\["UserTypeId"\]\.ToString\(\), "Import Grading", "edit"\)\.Count\(\)', 'await _repository.CheckUserPermissionAsync(userTypeId, "Import Grading", "edit", companyId)'
$c = $c -replace 'CheckUserPermission\(Session\["UserTypeId"\]\.ToString\(\), "Import Grading", "delete"\)\.Count\(\)', 'await _repository.CheckUserPermissionAsync(userTypeId, "Import Grading", "delete", companyId)'

# Remove MasterController log line
$c = $c -replace '(?m)^.*MasterController\.PagesLoginLog.*\r?\n', ''

# Attributes / signatures
$c = $c -replace '\s*\[HttpPost\]\s*', ''
$c = $c -replace '\s*\[Route\("[^"]+"\)\]\s*', ''
$c = $c -replace 'public ActionResult ImportGradingData\(\)', 'public async Task<ImportGradingResult> ImportGradingDataAsync(string userTypeId, string companyId)'
$c = $c -replace 'public ActionResult (\w+)\(ImportGrading ImportData\)', 'public async Task<ImportGradingResult> ${1}Async(ImportGradingDTO ImportData, string companyId, string userId, string userTypeId)'
$c = $c -replace 'public ActionResult UploadAndUpdateCompanyLoginLogo\(HttpPostedFileBase image\)', 'public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync(Microsoft.AspNetCore.Http.IFormFile image, string companyId, string userTypeId, string webRootPath)'
$c = $c -replace 'public ActionResult UpdateSizeIds\(\)', 'public async Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId)'
$c = $c -replace 'public ActionResult DeleteDeletedRecords\(\)', 'public async Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId, string userTypeId)'

# Json returns
$c = $c -replace 'return Json\(new \{ success = (true|false), Data = ([^}]+(?:\{[^}]*\}[^}]*)*)\ }, JsonRequestBehavior\.AllowGet\);', 'return new ImportGradingResult($1, $2);'
$c = $c -replace 'return Json\(new \{ success = (true|false), message = ([^,}]+), path = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2, $3);'
$c = $c -replace 'return Json\(new \{ success = (true|false), message = ([^}]+) \}\);', 'return new ImportGradingResult($1, null, $2);'
$c = $c -replace 'return Json\(new\s*\{\s*success = (true|false),\s*Data = ([^}]+)\s*\}\);', 'return new ImportGradingResult($1, $2);'

# Upload file
$c = $c -replace 'HttpPostedFileBase file = ImportData\.ImportFile;', @'
var importFile = ImportData.ImportFile;
                    if (importFile == null) return new ImportGradingResult(false, "No file uploaded.");
                    var file = importFile;
'@
$c = $c -replace 'ReadFromExcel\(file\)', 'ReadFromExcel(importFile.OpenReadStream())'

# Session values
$c = $c -replace 'Convert\.ToInt32\(Session\["CompanyId"\]\)', 'int.Parse(companyId)'
$c = $c -replace 'Convert\.ToInt32\(Session\["UserId"\]\)', 'int.Parse(userId)'
$c = $c -replace 'Session\["UserId"\]\.ToString\(\)', 'userId'
$c = $c -replace 'var SettingMaster = \(List<SettingMaster>\)Session\["SettingMaster"\];', 'var settingMaster = await _repository.GetSettingMasterListAsync(companyId);'
$c = $c -replace 'SettingMaster\.Where', 'settingMaster.Where'

# Data layer
$c = $c -replace 'SqlDataAccess\.', '_repository.'
$c = $c -replace 'using \(SqlTempAccess sql = new SqlTempAccess\(\)\)', 'using (var sql = await CreateSqlTempAsync(companyId))'
$c = $c -replace 'ItemMasterProcessor\.UpdateTable', '_repository.UpdateAskingPurchaseRateAsync'
$c = $c -replace 'BasicFunctionController\.GetConvertAmount', '_repository.GetConvertAmountAsync'
$c = $c -replace '\bSaveItemMasterData\(', 'await _repository.SaveItemMasterData('
$c = $c -replace '\bSaveAccGrpMasterData\(', 'await _repository.SaveAccGrpMasterData('

# Logo
$c = $c -replace 'if \(image == null \|\| image\.ContentLength == 0\)', 'if (image == null || image.Length == 0)'
$c = $c -replace 'Server\.MapPath\("~/" \+ relativeFolder\)', 'Path.Combine(webRootPath, relativeFolder)'
$c = $c -replace 'image\.SaveAs\(fullPath\);', 'using (var fs = new FileStream(fullPath, FileMode.Create)) { await image.CopyToAsync(fs); }'

# ReadFromExcel
$c = $c -replace 'public DataTable ReadFromExcel\(HttpPostedFileBase file\)', 'public static DataTable ReadFromExcel(Stream stream)'
$c = $c -replace 'Stream stream = file\.InputStream;\s*', ''
$c = $c -replace 'ExcelReaderFactory\.CreateOpenXmlReader\(stream\)', 'ExcelReaderFactory.CreateReader(stream)'

$c = $c -replace 'ImportGrading ImportData', 'ImportGradingDTO ImportData'
$c = $c -replace 'List<GradingList>', 'List<GradingListDto>'

# Repository async renames
$maps = @{
    '_repository.UpdateMultiGradingStatus('='_repository.UpdateMultiGradingStatusAsync('
    '_repository.SelectMultiGrading('='_repository.SelectMultiGradingAsync('
    '_repository.GetSize('='_repository.GetSizeAsync('
    '_repository.UpdateImageVideoPath('='_repository.UpdateImageVideoPathAsync('
    '_repository.UpdateSizeIds()'='_repository.UpdateSizeIdsAsync(companyId)'
    '_repository.DelRecords('='_repository.DelRecordsAsync('
    '_repository.UpdateCompanyLoginLogo('='_repository.UpdateCompanyLoginLogoAsync('
    '_repository.GetLotExist('='_repository.GetLotExistAsync('
    '_repository.GetMCurOperator('='_repository.GetMCurOperatorAsync('
    '_repository.GetSCurOperator('='_repository.GetSCurOperatorAsync('
    '_repository.GetMCurID()'='_repository.GetMCurIdAsync(companyId)'
    '_repository.GetSCurID()'='_repository.GetSCurIdAsync(companyId)'
}
foreach ($k in $maps.Keys) { $c = $c -replace [regex]::Escape($k), $maps[$k] }

$c = $c -replace 'UpdateMultiGradingStatusAsync\(([^,]+),\s*"Update"\)', 'UpdateMultiGradingStatusAsync($1, "Update", companyId)'
$c = $c -replace 'SelectMultiGradingAsync\(([^,]+),\s*"Select"\)', 'SelectMultiGradingAsync($1, "Select", companyId)'
$c = $c -replace 'GetSizeAsync\(([^,]+),\s*"GetSize"\)', 'GetSizeAsync($1, "GetSize", companyId)'
$c = $c -replace 'GetConvertAmountAsync\(([^)]+)\)', 'GetConvertAmountAsync($1, companyId)'
$c = $c -replace 'DelRecordsAsync\("DeleteFlag = 1"\)', 'DelRecordsAsync("DeleteFlag = 1", companyId)'
$c = $c -replace 'UpdateCompanyLoginLogoAsync\(([^,]+),\s*"UpdateLogo"\)', 'UpdateCompanyLoginLogoAsync($1, "UpdateLogo", companyId)'
$c = $c -replace 'UpdateAskingPurchaseRateAsync\(([^,]+),\s*([^,]+),\s*([^)]+)\)', 'UpdateAskingPurchaseRateAsync($1, $2, $3, companyId)'

$c = $c -replace '(?s)\[HttpPost\][^\{]*BackupDatabase\(\)[\s\S]*$', ''
$c = $c.TrimEnd() + "`r`n    }`r`n}`r`n"

Set-Content -Path $dest -Value $c -Encoding UTF8
Write-Host 'Minimal port done'
