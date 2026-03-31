# ============================================
# SmartWorkz v4 - Database Deployment Script
# Date: 2026-03-31
# Deploys all SQL migration scripts to SQL Server
# ============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "localhost",

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "SmartWorkz_v4",

    [Parameter(Mandatory=$false)]
    [string]$Username = "",

    [Parameter(Mandatory=$false)]
    [string]$Password = "",

    [Parameter(Mandatory=$false)]
    [switch]$IntegratedSecurity = $false,

    [Parameter(Mandatory=$false)]
    [string]$ScriptPath = (Split-Path -Parent $MyInvocation.MyCommand.Path)
)

# ============================================
# Configuration
# ============================================

$ErrorActionPreference = "Stop"
$WarningPreference = "Continue"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SmartWorkz v4 Database Deployment" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# Validate SQL Scripts
# ============================================

Write-Host "Validating SQL scripts..." -ForegroundColor Yellow

$requiredScripts = @(
    "001_InitializeDatabase.sql",
    "002_CreateTables_Master.sql",
    "003_CreateTables_Shared.sql",
    "004_CreateTables_Transaction.sql",
    "005_CreateTables_Report.sql",
    "006_CreateTables_Auth.sql",
    "007_SeedData.sql",
    "008_CreateIndexes.sql"
)

$missingScripts = @()

foreach ($script in $requiredScripts) {
    $scriptPath = Join-Path -Path $ScriptPath -ChildPath $script
    if (-not (Test-Path $scriptPath)) {
        $missingScripts += $script
        Write-Host "  X Missing: $script" -ForegroundColor Red
    } else {
        Write-Host "  + Found: $script" -ForegroundColor Green
    }
}

if ($missingScripts.Count -gt 0) {
    Write-Host ""
    Write-Host "Error: Missing SQL scripts. Deployment cancelled." -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================
# Build Connection String
# ============================================

Write-Host "Building connection string..." -ForegroundColor Yellow

if ($IntegratedSecurity) {
    $connectionString = "Server=$ServerName;Initial Catalog=$DatabaseName;Integrated Security=true;"
    Write-Host "  + Using Integrated Security" -ForegroundColor Green
} else {
    if ([string]::IsNullOrWhiteSpace($Username) -or [string]::IsNullOrWhiteSpace($Password)) {
        Write-Host "  Error: Username and Password required when not using Integrated Security" -ForegroundColor Red
        exit 1
    }
    $connectionString = "Server=$ServerName;Initial Catalog=$DatabaseName;User Id=$Username;Password=$Password;"
    Write-Host "  + Using SQL Server Authentication" -ForegroundColor Green
}

Write-Host "  Server: $ServerName" -ForegroundColor Cyan
Write-Host "  Database: $DatabaseName" -ForegroundColor Cyan
Write-Host ""

# ============================================
# Create Database (if not exists)
# ============================================

Write-Host "Creating database '$DatabaseName'..." -ForegroundColor Yellow

try {
    $masterConnection = "Server=$ServerName;Initial Catalog=master;"

    if ($IntegratedSecurity) {
        $masterConnection += "Integrated Security=true;"
    } else {
        $masterConnection += "User Id=$Username;Password=$Password;"
    }

    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $masterConnection
    $sqlConnection.Open()

    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    CREATE DATABASE [$DatabaseName];
    PRINT 'Database created successfully'
END
ELSE
BEGIN
    PRINT 'Database already exists'
END
"@

    $sqlCommand.ExecuteNonQuery() | Out-Null
    $sqlConnection.Close()

    Write-Host "  + Database ready" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "  X Error: $_" -ForegroundColor Red
    exit 1
}

# ============================================
# Execute Migration Scripts
# ============================================

Write-Host "Executing migration scripts..." -ForegroundColor Yellow
Write-Host ""

$scriptOrder = $requiredScripts

$successCount = 0
$failedScripts = @()

foreach ($scriptFile in $scriptOrder) {
    $scriptFullPath = Join-Path -Path $ScriptPath -ChildPath $scriptFile
    $scriptIndex = [array]::IndexOf($requiredScripts, $scriptFile) + 1

    Write-Host "[$scriptIndex/$($requiredScripts.Count)] Executing: $scriptFile" -ForegroundColor Cyan

    try {
        $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $sqlConnection.ConnectionString = $connectionString
        $sqlConnection.Open()

        $sqlScript = Get-Content -Path $scriptFullPath -Raw

        # Split into batches (handle GO statements)
        $batches = $sqlScript -split "(?m)^\s*GO\s*`$"

        foreach ($batch in $batches) {
            if ($batch.Trim().Length -gt 0) {
                $sqlCommand = $sqlConnection.CreateCommand()
                $sqlCommand.CommandText = $batch
                $sqlCommand.CommandTimeout = 300

                $sqlCommand.ExecuteNonQuery() | Out-Null
            }
        }

        $sqlConnection.Close()

        Write-Host "  + Success" -ForegroundColor Green
        $successCount++

    } catch {
        Write-Host "  X Error: $_" -ForegroundColor Red
        $failedScripts += @{ Script = $scriptFile; Error = $_ }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Scripts Executed: $successCount / $($requiredScripts.Count)" -ForegroundColor Cyan

if ($failedScripts.Count -eq 0) {
    Write-Host ""
    Write-Host "+ All scripts executed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Information:" -ForegroundColor Cyan
    Write-Host "  - Server: $ServerName" -ForegroundColor Cyan
    Write-Host "  - Database: $DatabaseName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Ready for Phase 1 Implementation!" -ForegroundColor Green
    Write-Host ""
    exit 0

} else {
    Write-Host ""
    Write-Host "Failed Scripts:" -ForegroundColor Red
    foreach ($failed in $failedScripts) {
        Write-Host "  - $($failed.Script)" -ForegroundColor Red
        Write-Host "    Error: $($failed.Error)" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Deployment failed. Please check the errors above." -ForegroundColor Red
    exit 1
}
