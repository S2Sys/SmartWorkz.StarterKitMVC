<# 
    SmartWorkz.StarterKitMVC - Project Rename Script (PowerShell)
    Usage: .\rename-project.ps1 -NewCompany "YourCompany" -NewProject "YourProject"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$NewCompany,
    
    [Parameter(Mandatory=$true)]
    [string]$NewProject
)

$OldCompany = "SmartWorkz"
$OldProject = "StarterKitMVC"

$RootPath = $PSScriptRoot

Write-Host "Renaming from $OldCompany.$OldProject to $NewCompany.$NewProject..." -ForegroundColor Cyan

# Rename file contents
Get-ChildItem -Path $RootPath -Recurse -Include *.cs,*.csproj,*.sln,*.json,*.md,*.cshtml,*.yml,*.yaml,*.xml -File | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match $OldCompany -or $content -match $OldProject) {
        $content = $content -replace $OldCompany, $NewCompany
        $content = $content -replace $OldProject, $NewProject
        Set-Content $_.FullName -Value $content
        Write-Host "Updated: $($_.FullName)" -ForegroundColor Green
    }
}

# Rename directories
Get-ChildItem -Path $RootPath -Recurse -Directory | Where-Object { $_.Name -match $OldCompany -or $_.Name -match $OldProject } | Sort-Object { $_.FullName.Length } -Descending | ForEach-Object {
    $newName = $_.Name -replace $OldCompany, $NewCompany
    $newName = $newName -replace $OldProject, $NewProject
    $newPath = Join-Path $_.Parent.FullName $newName
    if ($_.FullName -ne $newPath) {
        Rename-Item $_.FullName $newPath
        Write-Host "Renamed dir: $($_.FullName) -> $newPath" -ForegroundColor Yellow
    }
}

# Rename files
Get-ChildItem -Path $RootPath -Recurse -File | Where-Object { $_.Name -match $OldCompany -or $_.Name -match $OldProject } | ForEach-Object {
    $newName = $_.Name -replace $OldCompany, $NewCompany
    $newName = $newName -replace $OldProject, $NewProject
    $newPath = Join-Path $_.Directory.FullName $newName
    if ($_.FullName -ne $newPath) {
        Rename-Item $_.FullName $newPath
        Write-Host "Renamed file: $($_.FullName) -> $newPath" -ForegroundColor Yellow
    }
}

Write-Host "Done! Project renamed to $NewCompany.$NewProject" -ForegroundColor Cyan
