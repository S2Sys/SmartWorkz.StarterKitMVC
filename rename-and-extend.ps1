#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Rename SmartWorkz.StarterKitMVC to Starter and optionally add Mobile MAUI

.DESCRIPTION
    Renames project folders, files, and content from StarterKitMVC to Starter
    Optionally creates a Mobile MAUI project structure

.PARAMETER AddMobile
    Add Mobile MAUI project

.EXAMPLE
    .\rename-and-extend.ps1
    .\rename-and-extend.ps1 -AddMobile

#>

param(
    [switch]$AddMobile
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  Project Rename & Setup Script         ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Magenta

# Step 1: Rename Folders
Write-Host "━━━ Step 1: Renaming folders ━━━`n" -ForegroundColor Cyan

$folders = @(
    "src/SmartWorkz.StarterKitMVC.Web",
    "src/SmartWorkz.StarterKitMVC.Admin", 
    "src/SmartWorkz.StarterKitMVC.Public",
    "src/SmartWorkz.StarterKitMVC.Application",
    "src/SmartWorkz.StarterKitMVC.Domain",
    "src/SmartWorkz.StarterKitMVC.Infrastructure",
    "src/SmartWorkz.StarterKitMVC.Shared",
    "tests/SmartWorkz.StarterKitMVC.Tests.Unit",
    "tests/SmartWorkz.StarterKitMVC.Tests.Integration"
)

foreach ($folder in $folders) {
    $oldPath = Join-Path $root $folder
    $newPath = Join-Path $root ($folder -replace "StarterKitMVC", "Starter")
    
    if (Test-Path $oldPath) {
        Rename-Item -Path $oldPath -NewName (Split-Path $newPath -Leaf) -Force
        Write-Host "  ✓ $(Split-Path $folder -Leaf)" -ForegroundColor Green
    }
}

# Step 2: Update File Contents
Write-Host "`n━━━ Step 2: Updating file contents ━━━`n" -ForegroundColor Cyan

$files = Get-ChildItem -Path $root -Recurse -Include "*.cs", "*.csproj", "*.json", "*.sln", "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "bin|obj|\.git" }

$updated = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }
    
    $new = $content -replace "SmartWorkz\.StarterKitMVC", "SmartWorkz.Starter"
    if ($new -ne $content) {
        Set-Content -Path $file.FullName -Value $new -NoNewline
        $updated++
    }
}
Write-Host "  ✓ Updated $updated files`n" -ForegroundColor Green

# Step 3: Rename Solution File
Write-Host "━━━ Step 3: Renaming solution file ━━━`n" -ForegroundColor Cyan

$oldSln = Join-Path $root "SmartWorkz.StarterKitMVC.sln"
$newSln = Join-Path $root "SmartWorkz.Starter.sln"

if (Test-Path $oldSln) {
    Rename-Item -Path $oldSln -NewName "SmartWorkz.Starter.sln"
    Write-Host "  ✓ SmartWorkz.Starter.sln`n" -ForegroundColor Green
}

# Step 4: Add Mobile MAUI Project
if ($AddMobile) {
    Write-Host "━━━ Step 4: Creating Mobile MAUI project ━━━`n" -ForegroundColor Cyan
    
    $mobilePath = Join-Path $root "src/SmartWorkz.Starter.Mobile"
    
    $dirs = @(
        # UI & Pages
        "Pages/Auth", "Pages/Dashboard", "Pages/Products", "Pages/Users", "Pages/Settings",

        # MVVM Pattern
        "ViewModels", "ViewModels/Auth", "ViewModels/Dashboard", "ViewModels/Products",
        "Models", "Models/Requests", "Models/Responses",

        # Services
        "Services/Auth", "Services/API", "Services/Cache", "Services/Navigation", "Services/Logging",

        # Infrastructure
        "Infrastructure/DI", "Infrastructure/Configuration", "Infrastructure/Converters", "Infrastructure/Behaviors",

        # Resources
        "Resources", "Resources\Images", "Resources\Fonts", "Resources\Raw", "Resources\Styles",

        # Platform-specific
        "Platforms/Android", "Platforms/iOS", "Platforms/MacCatalyst", "Platforms/Windows"
    )
    
    foreach ($dir in $dirs) {
        $path = Join-Path $mobilePath $dir
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
    Write-Host "  ✓ Created project structure`n" -ForegroundColor Green
    
    # Create .csproj
    $csproj = @"
<Project Sdk="Microsoft.Maui.Sdk">
    <PropertyGroup>
        <TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0</TargetFrameworks>
        <TargetFrameworks Condition="`$([MSBuild]::GetTargetPlatformIdentifier()) == 'macos'">net9.0-maccatalyst</TargetFrameworks>
        <OutputType>Exe</OutputType>
        <UseMaui>true</UseMaui>
        <SingleProject>true</SingleProject>
        <ImplicitUsings>enable</ImplicitUsings>

        <ApplicationTitle>SmartWorkz Starter Mobile</ApplicationTitle>
        <ApplicationId>com.smartworkz.starter.mobile</ApplicationId>
        <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
        <ApplicationVersion>1</ApplicationVersion>

        <SupportedOSPlatformVersion Condition="`$([MSBuild]::GetTargetPlatformIdentifier()) == 'ios'">14.2</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="`$([MSBuild]::GetTargetPlatformIdentifier()) == 'android'">21.0</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="`$([MSBuild]::GetTargetPlatformIdentifier()) == 'windows'">10.0.19041.0</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="`$([MSBuild]::GetTargetPlatformIdentifier()) == 'maccatalyst'">13.1</SupportedOSPlatformVersion>
    </PropertyGroup>

    <ItemGroup>
        <MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundScale="0.65" Color="#512BD4" />
        <MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#512BD4" BaseSize="128,128" />
        <MauiImage Include="Resources\Images\*" />
        <MauiFont Include="Resources\Fonts\*" />
        <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.Maui.Controls" Version="9.0.0" />
        <PackageReference Include="Microsoft.Maui.Controls.Hosting" Version="9.0.0" />
        <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\SmartWorkz.Starter.Application\SmartWorkz.Starter.Application.csproj" />
        <ProjectReference Include="..\SmartWorkz.Starter.Infrastructure\SmartWorkz.Starter.Infrastructure.csproj" />
        <ProjectReference Include="..\SmartWorkz.Starter.Shared\SmartWorkz.Starter.Shared.csproj" />
    </ItemGroup>
</Project>
"@
    
    Set-Content -Path "$mobilePath\SmartWorkz.Starter.Mobile.csproj" -Value $csproj
    Write-Host "  ✓ Created .csproj file" -ForegroundColor Green
}

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✓ Setup Complete!                    ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Close and reopen solution in Visual Studio" -ForegroundColor White
Write-Host "  2. Run: dotnet build" -ForegroundColor White
Write-Host "  3. Run: dotnet test" -ForegroundColor White
Write-Host ""
