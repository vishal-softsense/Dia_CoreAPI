$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$content = Get-Content $dest -Raw

$content = $content -replace '__repository', '_sqlDataAccess'
$content = $content -replace '(?s)\}\s*\{\s*// GET: GetLabData', "`r`n        // GET: GetLabData"
$content = $content -replace '(?s)public ImportGradingResult ImportGradingData\(\)\s*\{[^}]+\}\s*public async Task', 'public async Task'

# Remove permission gate blocks at method start
$content = $content -replace '(?s)\s*int permission = CheckUserPermission\([^\)]+\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*', "`r`n                    "

# Remove broken trailing permission closers
$content = $content -replace '(?s)\s*, JsonRequestBehavior\.AllowGet\);\s*\}\s*', "`r`n                    "

# Session replacements
$content = $content -replace 'Convert\.ToInt32\(Session\["UserId"\]\)', 'int.Parse(userId)'
$content = $content -replace 'Session\["UserId"\]\.ToString\(\)', 'userId'
$content = $content -replace 'var SettingMaster = \(List<SettingMaster>\)Session\["SettingMaster"\];', 'var settingMaster = await _repository.GetSettingMasterListAsync(companyId);'
$content = $content -replace 'SettingMaster\.Where', 'settingMaster.Where'

# BasicFunctionController
$content = $content -replace 'BasicFunctionController\.GetConvertAmount', 'ImportGradingHelper.GetConvertAmount'

# Fix Upload logo permission block
$content = $content -replace '(?s)public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync[^\{]+\{\s*', "public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync(Microsoft.AspNetCore.Http.IFormFile image, string companyId, string webRootPath)`r`n        {`r`n"

# Remove remaining session check in UploadJewelleryFull
$content = $content -replace '(?s)if \(Session\["UserId"\] == null\)\s*\{\s*return new ImportGradingResult\([^\)]+\);\s*\}\s*', ''

# Repository async - add await for methods that will be async
$syncRepoCalls = @(
    'UpdateMultiGradingStatus',
    'SelectMultiGrading',
    'GetSize',
    'UpdateImageVideoPath',
    'UpdateSizeIds',
    'DelRecords',
    'UpdateCompanyLoginLogo',
    'GetLotExist',
    'GetMCurOperator',
    'GetSCurOperator',
    'GetMCurID',
    'GetSCurID',
    'UpdateAskingPurchaseRate',
    'SaveItemMasterData',
    'SaveAccGrpMasterData',
    'GetSettingMasterListAsync'
)
foreach ($call in $syncRepoCalls) {
    $content = $content -replace "_repository\.$call\(", "await _repository.$call("
}

Set-Content -Path $dest -Value $content -Encoding UTF8
Write-Host "Cleanup done"
