$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$content = Get-Content $dest -Raw

$perm = '(?s)\s*int permission = CheckUserPermission\([^\)]+\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*'
for ($i = 0; $i -lt 25; $i++) {
    if ($content -notmatch 'CheckUserPermission') { break }
    $content = [regex]::Replace($content, $perm, "`r`n                    ", 1)
}

# Remove unauthorized else blocks
$content = $content -replace '(?s)\}\s*else\s*\{\s*return new ImportGradingResult\(false, "You are not authorized to perform this task\."\);\s*\}\s*', "`r`n                    "
$content = $content -replace '(?s)\}\s*else\s*return new ImportGradingResult\(false, "You are not authorized[^}]+\}\s*', "`r`n                    "

# Fix broken method declarations
$content = $content -replace '//\}public async Task<ImportGradingResult> InsUpdaccountitemAsync', 'public async Task<ImportGradingResult> InsUpdaccountitemAsync'
$content = $content -replace '//Created By Sanjay Upload Jew Stockpublic async Task', 'public async Task'
$content = $content -replace '//Created By Sanjay Upload Jew Diamond Stockpublic async Task', 'public async Task'

# Fix formatting
$content = $content -replace '\}public async Task', "`r`n        public async Task"
$content = $content -replace '\{    public class', "{`r`n    public class"

Set-Content -Path $dest -Value $content -Encoding UTF8
Write-Host "Second pass done. Remaining CheckUserPermission:" ([regex]::Matches($content, 'CheckUserPermission')).Count
