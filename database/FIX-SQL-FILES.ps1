# ============================================
# Fix SQL Files - Replace database name
# ============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Fixing SQL Files" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Replacing 'USE SmartWorkz_v4;' with 'USE $DatabaseName;'" -ForegroundColor Yellow
Write-Host ""

$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlFiles = Get-ChildItem -Path $ScriptPath -Filter "*.sql" -File | Where-Object { $_.Name -match "^00[1-8]_" } | Sort-Object Name

$count = 0
foreach ($file in $sqlFiles) {
    try {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8

        # Replace the database name
        $newContent = $content -replace "USE SmartWorkz_v4;", "USE $DatabaseName;"

        # Write back if changed
        if ($content -ne $newContent) {
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
            Write-Host "  + Fixed: $($file.Name)" -ForegroundColor Green
            $count++
        } else {
            Write-Host "  = Skipped: $($file.Name) (already correct)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  X Error: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Fixed: $count files" -ForegroundColor Green
Write-Host ""
Write-Host "Now run:"
Write-Host "  .\QUICK-DEPLOY.ps1 -ServerName `"115.124.106.158`" -DatabaseName `"$DatabaseName`" -Username `"zenthil`" -Password `"PinkPanther#1`"" -ForegroundColor Cyan
Write-Host ""
