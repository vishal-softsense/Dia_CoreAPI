$dest = 'd:\CoreUI\MVC_API_UI\API\Services\Helpers\ImportGradingProcessor.cs'
$c = Get-Content $dest -Raw

$c = $c -replace '(?m)^\s*, JsonRequestBehavior\.AllowGet\);\s*\r?\n', ''
$c = $c -replace '(?m)^\s*\}\s*\r?\n(?=\s*\}\s*public async Task)', ''
$c = $c -replace '\}public async Task', "`r`n        public async Task"
$c = $c -replace '(?m)^\s*\}\s*\r?\n\s*\}\s*\r?\n(?=\s*public async Task)', "`r`n"
$c = $c -replace '(?s)\s*else\s*\{\s*return Json\(new \{ success = true, Data = "Image/Video[^}]+\}\)[^}]*\}\s*', ''

# Remove orphaned closing braces before public async Task (one extra } from permission block)
$c = $c -replace '(?m)\r?\n\s*\}\s*\r?\n\s*\}\s*\r?\n\s*public async Task', "`r`n        public async Task"

# Fix UploadLogo permission - remove remaining CheckUserPermission block
$c = $c -replace '(?s)int permission = CheckUserPermission\([^\)]+\)\.Count\(\);\s*if \(permission == 1\)\s*\{\s*', ''

Set-Content -Path $dest -Value $c -Encoding UTF8
Write-Host 'Fragment cleanup done'
