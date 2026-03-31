#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SmartWorkz StarterKit MVC - Drop All Tables
    Completely removes all tables and schemas from database

.DESCRIPTION
    Drops all database objects:
    - All 43 tables across 5 schemas
    - All 5 schemas (Master, Shared, Auth, Transaction, Report)
    - Drops all foreign key constraints first
    - Leaves database empty and ready for new schema

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

.EXAMPLE
    # Drop all tables with integrated auth
    .\QUICK-DROP.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

    # Drop all tables on remote server
    .\QUICK-DROP.ps1 -ServerName "115.124.106.158" -Username "zenthil" -Password "PinkPanther#1"

.NOTES
    Author: SmartWorkz
    Date: 2026-03-31
    WARNING: This will DROP ALL TABLES and SCHEMAS!
    This cannot be undone without restoring from backup.
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
    [switch]$IntegratedAuth
)

# ============================================
# Configuration
# ============================================
$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$databasePath = Join-Path $scriptPath "database"
$dropScriptPath = Join-Path $databasePath "010_DropAllTables.sql"

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
    $response = Read-Host "$Message (type 'DROP' to confirm)"
    return $response -eq "DROP"
}

function Invoke-SqlDrop {
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
Write-Header "SmartWorkz StarterKit MVC - Drop All Tables"

# Step 1: Validate parameters
Write-Info "Validating parameters..."
if (-not $IntegratedAuth -and (-not $Username -or -not $Password)) {
    Write-Error "SQL authentication mode requires -Username and -Password parameters"
    exit 1
}
Write-Success "Parameters validated"

# Step 2: Confirm drop (multiple confirmations for safety)
Write-Header "⚠️  CRITICAL WARNING - THIS WILL DROP ALL TABLES"
Write-Warning "Database: $DatabaseName"
Write-Warning "Server: $ServerName"
Write-Warning "Action: DROP ALL 43 TABLES AND 5 SCHEMAS"
Write-Warning ""
Write-Warning "This action CANNOT be undone without restoring from backup!"
Write-Host ""

if (-not (Get-UserConfirmation "Are you absolutely sure you want to DROP ALL TABLES from $DatabaseName?")) {
    Write-Info "Drop cancelled by user"
    exit 0
}

Write-Host ""
Write-Warning "FINAL CONFIRMATION REQUIRED"
Write-Warning "Type 'DROP' (in uppercase) to permanently drop all tables"

if (-not (Get-UserConfirmation "Type 'DROP' to confirm this action")) {
    Write-Info "Drop cancelled by user"
    exit 0
}

# Step 3: Execute drop
Write-Header "Executing Table Drop"

if (-not (Test-Path $dropScriptPath)) {
    Write-Error "Drop script not found: $dropScriptPath"
    exit 1
}

Write-Info "Running drop script: $dropScriptPath"
Write-Host ""

if (Invoke-SqlDrop -ServerName $ServerName -DatabaseName $DatabaseName -FilePath $dropScriptPath -Username $Username -Password $Password -IntegratedAuth $IntegratedAuth) {
    Write-Success "All tables dropped successfully"
}
else {
    Write-Error "Failed to drop tables"
    exit 1
}

# Step 4: Summary
Write-Header "Drop Summary"
Write-Success "All 43 tables removed"
Write-Success "All 5 schemas removed (Master, Shared, Auth, Transaction, Report)"
Write-Success "Database is empty and ready for new schema"

# Step 5: Next steps
Write-Header "Next Steps"
Write-Info "To recreate the database schema, run migration scripts in order:"
Write-Host "   .\QUICK-DEPLOY.ps1 -ServerName '$ServerName' -DatabaseName '$DatabaseName'" -ForegroundColor $colors.Info

Write-Info ""
Write-Info "Or run individual migration scripts:"
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/001_InitializeDatabase.sql" -ForegroundColor $colors.Info
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/002_CreateTables_Master.sql" -ForegroundColor $colors.Info
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/003_CreateTables_Shared.sql" -ForegroundColor $colors.Info
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/004_CreateTables_Transaction.sql" -ForegroundColor $colors.Info
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/005_CreateTables_Report.sql" -ForegroundColor $colors.Info
Write-Host "   sqlcmd -S $ServerName -d $DatabaseName -i database/006_CreateTables_Auth.sql" -ForegroundColor $colors.Info

Write-Success "`nAll tables have been permanently dropped! 🗑️"
