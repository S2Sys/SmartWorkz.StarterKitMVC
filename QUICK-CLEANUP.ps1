#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SmartWorkz StarterKit MVC - Quick Database Cleanup
    Removes all data while preserving schema structure

.DESCRIPTION
    Cleans all data from database:
    - Disables foreign key constraints
    - Deletes all data from all tables (in dependency order)
    - Resets identity seeds to 0
    - Re-enables all constraints

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

.PARAMETER ReseedIdentity
    Reset identity seeds to 0 (default: true)

.EXAMPLE
    # Clean database with integrated auth
    .\QUICK-CLEANUP.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

    # Clean remote database with SQL authentication
    .\QUICK-CLEANUP.ps1 -ServerName "115.124.106.158" -Username "zenthil" -Password "PinkPanther#1"

.NOTES
    Author: SmartWorkz
    Date: 2026-03-31
    WARNING: This will delete ALL data from the database!
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

    [Parameter(Mandatory = $false, HelpMessage = "Reset identity seeds")]
    [switch]$ReseedIdentity = $true
)

# ============================================
# Configuration
# ============================================
$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$databasePath = Join-Path $scriptPath "database"
$cleanupScriptPath = Join-Path $databasePath "009_CleanupAllData.sql"

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

function Get-UserConfirmation {
    param([string]$Message)
    $response = Read-Host "$Message (yes/no)"
    return $response -eq "yes" -or $response -eq "y"
}

function Invoke-SqlCleanup {
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
Write-Header "SmartWorkz StarterKit MVC - Database Cleanup"

# Step 1: Validate parameters
Write-Info "Validating parameters..."
if (-not $IntegratedAuth -and (-not $Username -or -not $Password)) {
    Write-Error "SQL authentication mode requires -Username and -Password parameters"
    exit 1
}
Write-Success "Parameters validated"

# Step 2: Confirm cleanup
Write-Header "⚠️  WARNING - THIS WILL DELETE ALL DATA"
Write-Warning "Database: $DatabaseName"
Write-Warning "Server: $ServerName"
Write-Warning "Action: DELETE ALL DATA (schema preserved, identities reset)"
Write-Host ""

if (-not (Get-UserConfirmation "Are you sure you want to DELETE ALL DATA from $DatabaseName?")) {
    Write-Info "Cleanup cancelled by user"
    exit 0
}

Write-Host ""
if (-not (Get-UserConfirmation "Type 'yes' again to confirm (this cannot be undone)")) {
    Write-Info "Cleanup cancelled by user"
    exit 0
}

# Step 3: Execute cleanup
Write-Header "Executing Database Cleanup"

if (-not (Test-Path $cleanupScriptPath)) {
    Write-Error "Cleanup script not found: $cleanupScriptPath"
    exit 1
}

Write-Info "Running cleanup script: $cleanupScriptPath"
Write-Host ""

if (Invoke-SqlCleanup -ServerName $ServerName -DatabaseName $DatabaseName -FilePath $cleanupScriptPath -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth) {
    Write-Success "Database cleanup completed successfully"
}
else {
    Write-Error "Database cleanup failed"
    exit 1
}

# Step 4: Summary
Write-Header "Cleanup Summary"
Write-Success "All data removed from all tables"
Write-Success "Schema structures preserved"
Write-Success "All constraints re-enabled"
Write-Success "All identity seeds reset"

# Step 5: Next steps
Write-Header "Next Steps"
Write-Info "1. To reload initial seed data:"
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/007_SeedData.sql" -ForegroundColor $colors.Info
Write-Info ""
Write-Info "2. To reload test users:"
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/008_SeedTestUsers.sql" -ForegroundColor $colors.Info
Write-Info ""
Write-Info "3. Or run full deployment:"
Write-Host "   .\QUICK-DEPLOY.ps1 -ServerName '$ServerName' -Username '$Username' -Password '***'" -ForegroundColor $colors.Info

Write-Success "`nDatabase is ready for fresh start! ✨"
