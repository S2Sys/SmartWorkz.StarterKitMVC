# Project Rename & Extend Guide

## Overview

The `rename-and-extend.ps1` script helps you:
1. **Rename** the project from `SmartWorkz.StarterKitMVC` to `SmartWorkz.Starter`
2. **Add Mobile MAUI** project for cross-platform mobile development

## Prerequisites

- PowerShell 7+ (run `$PSVersionTable.PSVersion` to check)
- The script must run from the project root directory

## Usage

### Option 1: Rename Only

```powershell
cd s:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC
.\rename-and-extend.ps1
```

**What it does:**
- ✓ Renames all folders (Web → Web, Admin → Admin, etc.)
- ✓ Updates all .cs, .csproj, .json, .sln files
- ✓ Renames solution file from `SmartWorkz.StarterKitMVC.sln` to `SmartWorkz.Starter.sln`

### Option 2: Rename + Add Mobile

```powershell
.\rename-and-extend.ps1 -AddMobile
```

**What it does:**
- ✓ Does everything from Option 1
- ✓ Creates `SmartWorkz.Starter.Mobile` project structure
- ✓ Sets up MAUI targeting for: Android, iOS, macOS, Windows
- ✓ Adds Mobile project to solution file
- ✓ Configures project references to Application, Infrastructure, Shared

## What Gets Renamed

### Folders
```
SmartWorkz.StarterKitMVC.Web          → SmartWorkz.Starter.Web
SmartWorkz.StarterKitMVC.Admin        → SmartWorkz.Starter.Admin
SmartWorkz.StarterKitMVC.Public       → SmartWorkz.Starter.Public
SmartWorkz.StarterKitMVC.Application  → SmartWorkz.Starter.Application
SmartWorkz.StarterKitMVC.Domain       → SmartWorkz.Starter.Domain
SmartWorkz.StarterKitMVC.Infrastructure → SmartWorkz.Starter.Infrastructure
SmartWorkz.StarterKitMVC.Shared       → SmartWorkz.Starter.Shared
SmartWorkz.StarterKitMVC.Tests.Unit   → SmartWorkz.Starter.Tests.Unit
SmartWorkz.StarterKitMVC.Tests.Integration → SmartWorkz.Starter.Tests.Integration
```

### File Contents
All occurrences of:
- `SmartWorkz.StarterKitMVC` → `SmartWorkz.Starter`

### Solution File
- `SmartWorkz.StarterKitMVC.sln` → `SmartWorkz.Starter.sln`

## Mobile MAUI Project Structure

When using `-AddMobile`, creates:

```
SmartWorkz.Starter.Mobile/
├── Models/
├── Views/
├── ViewModels/
├── Services/
├── Pages/
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Raw/
├── SmartWorkz.Starter.Mobile.csproj
├── MauiProgram.cs
└── AppShell.xaml
```

### Mobile Project Features

- **Target Frameworks:**
  - `net9.0-android` (Android 21+)
  - `net9.0-ios` (iOS 14.2+)
  - `net9.0-maccatalyst` (macOS 13.1+)
  - `net9.0-windows10.0.19041.0` (Windows 10+)

- **Included Packages:**
  - `Microsoft.Maui.Controls` (v9.0.0)
  - `CommunityToolkit.Mvvm` (v8.2.2)

- **Project References:**
  - SmartWorkz.Starter.Application
  - SmartWorkz.Starter.Infrastructure
  - SmartWorkz.Starter.Shared

## After Running the Script

### 1. Reload Solution
```powershell
# Close and reopen the solution file in Visual Studio
# Or: Right-click solution → Reload Project
```

### 2. Build
```powershell
dotnet build
```

### 3. Test
```powershell
dotnet test
```

### 4. Verify Mobile (if added)
```powershell
# Check Mobile project loads in Visual Studio
dotnet build src/SmartWorkz.Starter.Mobile/SmartWorkz.Starter.Mobile.csproj
```

## Troubleshooting

### Script won't run
```powershell
# Fix execution policy temporarily
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\rename-and-extend.ps1 -AddMobile
```

### PowerShell version error
```powershell
# Check version
$PSVersionTable.PSVersion

# Install PowerShell 7 if needed
# https://github.com/PowerShell/PowerShell/releases
```

### Files not updating
- Ensure no files are open in Visual Studio
- Check file permissions (Administrator may be needed)
- Verify files aren't marked as read-only

### Solution file not loading
```powershell
# Delete these directories and rebuild
Remove-Item .vs -Recurse -Force
dotnet clean
dotnet build
```

## Undoing Changes

If you need to revert:

```powershell
# Option 1: Use Git (if changes aren't committed)
git reset --hard HEAD

# Option 2: Restore from backup (if you made one)
```

## Adding More Projects Later

To add more projects after running the script:

1. Create project folder: `src/SmartWorkz.Starter.YourProject`
2. Create `.csproj` file
3. Add project reference to solution file manually
4. Or edit solution file with Visual Studio

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Verify all prerequisites are met
3. Check script output for error messages
4. Ensure you're running from the correct directory
