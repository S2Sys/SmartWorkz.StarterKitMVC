#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SmartWorkz StarterKit MVC - Quick Deployment Script
    Automates database setup and application deployment

.DESCRIPTION
    This script handles:
    - Database creation
    - Running all migration scripts in order
    - Seeding test data and users
    - Building the application
    - Starting the web server

.PARAMETER ServerName
    SQL Server instance name or IP address (required)
    Example: "localhost", ".\SQLEXPRESS", "115.124.106.158"

.PARAMETER DatabaseName
    Database name (default: "Boilerplate")

.PARAMETER Username
    SQL Server username (for SQL auth mode)

.PARAMETER Password
    SQL Server password (for SQL auth mode)

.PARAMETER IntegratedAuth
    Use Windows Integrated Authentication instead of SQL auth

.PARAMETER SkipBuild
    Skip dotnet build step

.PARAMETER StartApp
    Start the application after deployment

.EXAMPLE
    # Deploy to local SQL Server Express with integrated auth
    .\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

    # Deploy to remote server with SQL authentication
    .\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"

    # Deploy and start application
    .\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth -StartApp

.NOTES
    Author: SmartWorkz
    Date: 2026-03-31
    Requires: SQL Server, PowerShell 7+, .NET 9 SDK
#>

param(
    [Parameter(Mandatory = $true, HelpMessage = "SQL Server instance name or IP")]
    [string]$ServerName,

    [Parameter(Mandatory = $false, HelpMessage = "Database name")]
    [string]$DatabaseName = "Boilerplate",

    [Parameter(Mandatory = $false, HelpMessage = "SQL Server username")]
    [string]$Username,

    [Parameter(Mandatory = $false, HelpMessage = "SQL Server password")]
    [string]$Password,

    [Parameter(Mandatory = $false, HelpMessage = "Use Windows Integrated Authentication")]
    [switch]$IntegratedAuth,

    [Parameter(Mandatory = $false, HelpMessage = "Skip dotnet build")]
    [switch]$SkipBuild,

    [Parameter(Mandatory = $false, HelpMessage = "Start the application")]
    [switch]$StartApp
)

# ============================================
# Configuration
# ============================================
$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$databasePath = Join-Path $scriptPath "database"
$projectRoot = $scriptPath

# Color output
$colors = @{
    Success = 'Green'
    Error   = 'Red'
    Warning = 'Yellow'
    Info    = 'Cyan'
    Header  = 'Magenta'
}

# ============================================
# Helper Functions
# ============================================
function Write-Header {
    param([string]$Message)
    Write-Host "`n" -NoNewline
    Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor $colors.Header
    Write-Host "║ $Message" -ForegroundColor $colors.Header
    Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor $colors.Header
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $colors.Success
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $colors.Error
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $colors.Warning
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $colors.Info
}

function Test-SqlConnection {
    param(
        [string]$ServerName,
        [string]$DatabaseName,
        [string]$Username,
        [string]$Password,
        [bool]$IntegratedAuth
    )

    try {
        if ($IntegratedAuth) {
            $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;"
        }
        else {
            $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;Encrypt=true;TrustServerCertificate=true;"
        }

        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Invoke-SqlScript {
    param(
        [string]$ServerName,
        [string]$DatabaseName,
        [string]$FilePath,
        [string]$Username,
        [string]$Password,
        [bool]$IntegratedAuth,
        [bool]$MasterDb = $false
    )

    $dbName = if ($MasterDb) { "master" } else { $DatabaseName }

    try {
        if ($IntegratedAuth) {
            & sqlcmd -S $ServerName -d $dbName -i $FilePath -b
        }
        else {
            & sqlcmd -S $ServerName -d $dbName -U $Username -P $Password -i $FilePath -b
        }

        if ($LASTEXITCODE -eq 0) {
            return $true
        }
        else {
            return $false
        }
    }
    catch {
        Write-Error "Failed to execute script: $FilePath"
        Write-Error $_.Exception.Message
        return $false
    }
}

# ============================================
# Main Deployment Steps
# ============================================
Write-Header "SmartWorkz StarterKit MVC - Deployment"

# Step 1: Validate parameters
Write-Info "Validating parameters..."
if (-not $IntegratedAuth -and (-not $Username -or -not $Password)) {
    Write-Error "SQL authentication mode requires -Username and -Password parameters"
    exit 1
}
Write-Success "Parameters validated"

# Step 2: Test SQL connection
Write-Info "Testing SQL Server connection..."
$connectionTest = Test-SqlConnection -ServerName $ServerName -DatabaseName "master" -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth

if (-not $connectionTest) {
    Write-Error "Cannot connect to SQL Server at $ServerName"
    Write-Warning "Verify:"
    Write-Warning "  - Server name/IP is correct: $ServerName"
    Write-Warning "  - SQL Server is running"
    Write-Warning "  - Credentials are correct"
    exit 1
}
Write-Success "SQL Server connection successful"

# Step 3: Recreate database
Write-Header "Step 1/4: Creating Database"
Write-Info "Dropping existing database (if exists)..."

$recreateScript = @"
IF DB_ID('$DatabaseName') IS NOT NULL
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DatabaseName];
    PRINT '✓ Database dropped'
END

CREATE DATABASE [$DatabaseName];
PRINT '✓ Database created'
"@

$tempScript = Join-Path $env:TEMP "recreate_db_$(Get-Random).sql"
$recreateScript | Out-File -FilePath $tempScript -Encoding UTF8

if (Invoke-SqlScript -ServerName $ServerName -DatabaseName "master" -FilePath $tempScript -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth -MasterDb $true) {
    Write-Success "Database created successfully"
    Remove-Item $tempScript -Force
}
else {
    Write-Error "Failed to create database"
    Remove-Item $tempScript -Force
    exit 1
}

# Step 4: Run migration scripts in order
Write-Header "Step 2/4: Running Migrations"

$migrationScripts = @(
    "001_CreateTables_Master.sql",
    "002_CreateTables_Shared.sql",
    "003_CreateTables_Transaction.sql",
    "004_CreateTables_Report.sql",
    "005_CreateTables_Audit.sql",
    "006_CreateTables_Auth.sql"
)

$counter = 1
foreach ($script in $migrationScripts) {
    $scriptPath = Join-Path $databasePath $script

    if (-not (Test-Path $scriptPath)) {
        Write-Error "Migration script not found: $script"
        exit 1
    }

    Write-Info "($counter/$($migrationScripts.Count)) Running $script..."

    if (Invoke-SqlScript -ServerName $ServerName -DatabaseName $DatabaseName -FilePath $scriptPath -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth) {
        Write-Success "Completed: $script"
    }
    else {
        Write-Error "Failed to execute: $script"
        exit 1
    }

    $counter++
}

# Step 5: Seed data
Write-Header "Step 3/4: Seeding Data"

$seedScripts = @(
    "007_SeedData.sql",
    "008_SeedTestUsers.sql"
)

$counter = 1
foreach ($script in $seedScripts) {
    $scriptPath = Join-Path $databasePath $script

    if (-not (Test-Path $scriptPath)) {
        Write-Error "Seed script not found: $script"
        exit 1
    }

    Write-Info "($counter/$($seedScripts.Count)) Running $script..."

    if (Invoke-SqlScript -ServerName $ServerName -DatabaseName $DatabaseName -FilePath $scriptPath -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth) {
        Write-Success "Completed: $script"
    }
    else {
        Write-Error "Failed to execute: $script"
        exit 1
    }

    $counter++
}

# Step 6: Build application
Write-Header "Step 4/4: Building Application"

if (-not $SkipBuild) {
    Write-Info "Restoring NuGet packages..."
    Push-Location $projectRoot

    if (dotnet restore) {
        Write-Success "Packages restored"
    }
    else {
        Write-Error "Failed to restore NuGet packages"
        Pop-Location
        exit 1
    }

    Write-Info "Building application..."
    if (dotnet build) {
        Write-Success "Application built successfully"
    }
    else {
        Write-Error "Failed to build application"
        Pop-Location
        exit 1
    }

    Pop-Location
}
else {
    Write-Info "Skipping build (--SkipBuild specified)"
}

# Step 7: Summary
Write-Header "Deployment Summary"
Write-Success "Database: $DatabaseName"
Write-Success "Server: $ServerName"
Write-Success "Auth Mode: $(if ($IntegratedAuth) { 'Windows Integrated' } else { 'SQL Authentication' })"
Write-Success "Status: ✅ READY FOR TESTING"

# Step 8: Display next steps
Write-Header "Next Steps"
Write-Info "1. Update appsettings.json with connection string (if needed)"
Write-Info "2. Start application:"
Write-Host "   dotnet run" -ForegroundColor $colors.Info
Write-Info "3. Open Swagger UI:"
Write-Host "   http://localhost:5000/swagger" -ForegroundColor $colors.Info
Write-Info "4. Login with test credentials:"
Write-Host "   Email: admin@smartworkz.test" -ForegroundColor $colors.Info
Write-Host "   Password: TestPassword123!" -ForegroundColor $colors.Info

# Step 9: Optional - Start application
if ($StartApp) {
    Write-Header "Starting Application"
    Push-Location $projectRoot
    Write-Info "Launching 'dotnet run'..."
    dotnet run
    Pop-Location
}

Write-Success "`nDeployment completed successfully! 🚀"
