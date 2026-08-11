$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$content = Get-Content $dest -Raw

$content = $content -replace '(?s)private async Task<SqlTempAccess> CreateSqlTempAsync\(string companyId\)\s*\{\s*var cs = await _sqlDataAccess\.GetClientConnectionString\(companyId\);\s*return new SqlTempAccess\(cs\);\s*', @'
private async Task<SqlTempAccess> CreateSqlTempAsync(string companyId)
        {
            var cs = await _sqlDataAccess.GetClientConnectionString(companyId);
            return new SqlTempAccess(cs);
        }

'@

$content = $content -replace '(?s)public ImportGradingResult ImportGradingData\(\)\s*\{.*?\}(?=public async Task)', ''

$perm = '(?s)\s*int permission = CheckUserPermission\([^\)]+\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*'
for ($i = 0; $i -lt 30; $i++) {
    if ($content -notmatch 'int permission = CheckUserPermission') { break }
    $content = [regex]::Replace($content, $perm, "`r`n                    ", 1)
}

$content = $content -replace '(?m)^.*MasterController\.PagesLoginLog.*\r?\n', ''
$content = $content -replace '(?m)^.*return View\(\);.*\r?\n', ''
$content = $content -replace 'List<GradingList>', 'List<GradingListDto>'

$maps = @{
    'await _repository.UpdateMultiGradingStatus\(' = 'await _repository.UpdateMultiGradingStatusAsync('
    'await _repository.SelectMultiGrading\(' = 'await _repository.SelectMultiGradingAsync('
    'await _repository.GetSize\(' = 'await _repository.GetSizeAsync('
    'await _repository.UpdateImageVideoPath\(' = 'await _repository.UpdateImageVideoPathAsync('
    'await _repository.UpdateSizeIds\(\)' = 'await _repository.UpdateSizeIdsAsync(companyId)'
    'await _repository.DelRecords\(' = 'await _repository.DelRecordsAsync('
    'await _repository.UpdateCompanyLoginLogo\(' = 'await _repository.UpdateCompanyLoginLogoAsync('
    'await _repository.GetLotExist\(' = 'await _repository.GetLotExistAsync('
    'await _repository.GetMCurOperator\(' = 'await _repository.GetMCurOperatorAsync('
    'await _repository.GetSCurOperator\(' = 'await _repository.GetSCurOperatorAsync('
    'await _repository.GetMCurID\(\)' = 'await _repository.GetMCurIdAsync(companyId)'
    'await _repository.GetSCurID\(\)' = 'await _repository.GetSCurIdAsync(companyId)'
    'await _repository.UpdateAskingPurchaseRate\(' = 'await _repository.UpdateAskingPurchaseRateAsync('
    'ImportGradingHelper.GetConvertAmount\(' = 'await _repository.GetConvertAmountAsync('
    'SaveItemMasterData\(' = 'await _repository.SaveItemMasterData('
    'SaveAccGrpMasterData\(' = 'await _repository.SaveAccGrpMasterData('
}
foreach ($k in $maps.Keys) { $content = $content -replace $k, $maps[$k] }

# Append companyId to repository calls that need it (simple suffix patterns)
$content = $content -replace 'UpdateMultiGradingStatusAsync\(([^,]+),\s*"Update"\)', 'UpdateMultiGradingStatusAsync($1, "Update", companyId)'
$content = $content -replace 'SelectMultiGradingAsync\(([^,]+),\s*"Select"\)', 'SelectMultiGradingAsync($1, "Select", companyId)'
$content = $content -replace 'GetSizeAsync\(([^,]+),\s*"GetSize"\)', 'GetSizeAsync($1, "GetSize", companyId)'
$content = $content -replace 'GetConvertAmountAsync\(([^)]+)\)', 'GetConvertAmountAsync($1, companyId)'
$content = $content -replace 'DelRecordsAsync\("DeleteFlag = 1"\)', 'DelRecordsAsync("DeleteFlag = 1", companyId)'
$content = $content -replace 'UpdateCompanyLoginLogoAsync\(([^,]+),\s*"UpdateLogo"\)', 'UpdateCompanyLoginLogoAsync($1, "UpdateLogo", companyId)'
$content = $content -replace 'GetLotExistAsync\(" and LotNo=', 'GetLotExistAsync(" and LotNo='
$content = $content -replace 'GetLotExistAsync\(" and LotNo=''([^'']+)''"\)', 'GetLotExistAsync(" and LotNo=''$1''", companyId)'
$content = $content -replace 'UpdateAskingPurchaseRateAsync\(([^,]+),\s*([^,]+),\s*([^)]+)\)', 'UpdateAskingPurchaseRateAsync($1, $2, $3, companyId)'

Set-Content -Path $dest -Value $content -Encoding UTF8
Write-Host 'PS fix complete'
