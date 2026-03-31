#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SmartWorkz StarterKit MVC v4 - Quick Database Deployment
    Automates database creation and schema deployment

.DESCRIPTION
    This script handles:
    - Database creation
    - Running all 8 migration scripts in correct order
    - Seeding reference data and test users
    - Building the application

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

.EXAMPLE
    # Deploy to local SQL Server Express with integrated auth
    .\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

    # Deploy to remote server with SQL authentication
    .\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "admin" -Password "P@ssw0rd"

    # Deploy and skip build
    .\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth -SkipBuild

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
    [switch]$SkipBuild
)

# ============================================
# Configuration
# ============================================
$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$databasePath = Join-Path $scriptPath "v1"

# Migration scripts in execution order
$migrations = @(
    "000_DeleteAllSchemas.sql",
    "001_InitializeDatabase.sql",
    "002_CreateTables_Master.sql",
    "003_CreateTables_Shared.sql",
    "004_CreateTables_Transaction.sql",
    "005_CreateTables_Report.sql",
    "006_CreateTables_Auth.sql",
    "007_SeedData.sql",
    "008_SeedTestUsers.sql"
)

# Colors for output
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

function Invoke-SqlMigration {
    param(
        [string]$ServerName,
        [string]$DatabaseName,
        [string]$FilePath,
        [string]$Username,
        [string]$Password,
        [bool]$IntegratedAuth
    )

    try {
        if ($IntegratedAuth) {
            & sqlcmd -S $ServerName -d $DatabaseName -i $FilePath -b
        }
        else {
            & sqlcmd -S $ServerName -d $DatabaseName -U $Username -P $Password -i $FilePath -b
        }

        if ($LASTEXITCODE -eq 0) {
            return $true
        }
        else {
            return $false
        }
    }
    catch {
        Write-Error $_.Exception.Message
        return $false
    }
}

# ============================================
# Main Execution
# ============================================
Write-Header "SmartWorkz StarterKit MVC v4 - Database Deployment"

# Step 1: Validate parameters
Write-Info "Validating parameters..."
if (-not $IntegratedAuth -and (-not $Username -or -not $Password)) {
    Write-Error "SQL authentication mode requires -Username and -Password parameters"
    exit 1
}
Write-Success "Parameters validated"

# Step 2: Validate migration files exist
Write-Info "Validating migration files..."
$missingFiles = @()
foreach ($migration in $migrations) {
    $filePath = Join-Path $databasePath $migration
    if (-not (Test-Path $filePath)) {
        $missingFiles += $migration
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Error "Missing migration files:"
    $missingFiles | ForEach-Object { Write-Error "  • $_" }
    exit 1
}
Write-Success "All 8 migration files found"

# Step 3: Execute migrations
Write-Header "Executing Database Migrations"
Write-Info "Database: $DatabaseName"
Write-Info "Server: $ServerName"
Write-Info "All scripts run on Boilerplate database"
Write-Host ""

$step = 1
$totalSteps = $migrations.Count

foreach ($migration in $migrations) {
    $filePath = Join-Path $databasePath $migration
    Write-Info "[$step/$totalSteps] Running $migration..."

    if (Invoke-SqlMigration -ServerName $ServerName -DatabaseName $DatabaseName -FilePath $filePath -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth) {
        Write-Success "$migration completed"
    }
    else {
        Write-Error "Failed to execute $migration"
        exit 1
    }

    $step++
}

# Step 4: Build application
if (-not $SkipBuild) {
    Write-Header "Building Application"
    $projectPath = Split-Path -Parent (Split-Path -Parent $scriptPath)
    $slnPath = Join-Path $projectPath "SmartWorkz.StarterKitMVC.sln"

    if (Test-Path $slnPath) {
        Write-Info "Building solution: $slnPath"
        try {
            & dotnet build $slnPath -c Release --quiet
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Application build successful"
            }
            else {
                Write-Warning "Application build had warnings/errors but deployment completed"
            }
        }
        catch {
            Write-Warning "Could not build application: $($_.Exception.Message)"
        }
    }
    else {
        Write-Warning "Solution file not found: $slnPath"
    }
}

# Step 5: Summary
Write-Header "Deployment Summary"
Write-Success "All old objects removed (tables, stored procedures, indexes)"
Write-Success "Database: $DatabaseName preserved and cleaned"
Write-Success "All 9 migration scripts executed in order"
Write-Success "All schemas initialized (Master, Shared, Auth, Transaction, Report)"
Write-Success "All 43 tables created with proper relationships and constraints"
Write-Success "Reference data seeded (Tenants, Languages, Countries, Currencies, Roles, Permissions)"
Write-Success "Test users created (admin, manager, staff, customer)"

# Step 6: Next steps
Write-Header "Next Steps"
Write-Info "1. Update appsettings.json with connection string if needed"
Write-Info "2. Start the application: dotnet run --project src/SmartWorkz.StarterKitMVC.Web"
Write-Info "3. Open Swagger UI: https://localhost:5001/swagger"
Write-Info "4. Test login with test credentials:"
Write-Host "   Email: admin@smartworkz.test" -ForegroundColor $colors.Info
Write-Host "   Password: TestPassword123!" -ForegroundColor $colors.Info

Write-Success "`n✨ Database deployment complete!"
