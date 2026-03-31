# ============================================
# SmartWorkz v4 - QUICK DEPLOY
# Simplest deployment script with cleanup (drop tables & schemas, NOT database)
# Usage: .\QUICK-DEPLOY.ps1 -ServerName "server" -DatabaseName "db" -Username "user" -Password "pass"
# ============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,

    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,

    [Parameter(Mandatory=$true)]
    [string]$Username,

    [Parameter(Mandatory=$true)]
    [string]$Password,

    [Parameter(Mandatory=$false)]
    [switch]$SkipCleanup = $false
)

# Get script location
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SmartWorkz v4 - Quick Deploy" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Server: $ServerName" -ForegroundColor Cyan
Write-Host "  Database: $DatabaseName" -ForegroundColor Cyan
Write-Host "  User: $Username" -ForegroundColor Cyan
Write-Host "  Cleanup Tables/Schemas: $(if($SkipCleanup) { 'SKIPPED' } else { 'ENABLED' })" -ForegroundColor Cyan
Write-Host ""

# List SQL files
Write-Host "SQL Files to Deploy:" -ForegroundColor Yellow
$sqlFiles = Get-ChildItem -Path $ScriptPath -Filter "*.sql" -File | Where-Object { $_.Name -match "^00[1-8]_" } | Sort-Object Name
foreach ($file in $sqlFiles) {
    Write-Host "  + $($file.Name)" -ForegroundColor Green
}
Write-Host ""

if ($sqlFiles.Count -lt 8) {
    Write-Host "ERROR: Only $($sqlFiles.Count) SQL files found. Expected 8 files (001-008)." -ForegroundColor Red
    exit 1
}

# Test connection
Write-Host "Testing connection to $ServerName..." -ForegroundColor Yellow
try {
    $testConn = New-Object System.Data.SqlClient.SqlConnection
    $testConn.ConnectionString = "Server=$ServerName;Initial Catalog=master;User Id=$Username;Password=$Password;"
    $testConn.Open()
    Write-Host "  + Connected successfully!" -ForegroundColor Green
    $testConn.Close()
} catch {
    Write-Host "  X Connection failed!" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Create database if not exists
Write-Host "Checking/Creating database '$DatabaseName'..." -ForegroundColor Yellow
try {
    $checkConn = New-Object System.Data.SqlClient.SqlConnection
    $checkConn.ConnectionString = "Server=$ServerName;Initial Catalog=master;User Id=$Username;Password=$Password;"
    $checkConn.Open()

    $checkCmd = $checkConn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = N'$DatabaseName'"
    $dbExists = $checkCmd.ExecuteScalar()

    if ($dbExists -eq 0) {
        # Create database
        Write-Host "  + Database does not exist, creating..." -ForegroundColor Yellow
        $createCmd = $checkConn.CreateCommand()
        $createCmd.CommandText = "CREATE DATABASE [$DatabaseName]"
        $createCmd.ExecuteNonQuery() | Out-Null
        Write-Host "  + Database created" -ForegroundColor Green
    } else {
        Write-Host "  + Database exists" -ForegroundColor Green
    }

    $checkConn.Close()
} catch {
    Write-Host "  X Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Note: Cleanup is now handled in 001_InitializeDatabase.sql
if (-not $SkipCleanup) {
    Write-Host "Cleanup will be performed by 001_InitializeDatabase.sql..." -ForegroundColor Yellow
}

Write-Host ""

# Deploy SQL files
Write-Host "Deploying SQL files..." -ForegroundColor Yellow
Write-Host ""

$connectionString = "Server=$ServerName;Initial Catalog=$DatabaseName;User Id=$Username;Password=$Password;"
$deployedCount = 0
$failedCount = 0

foreach ($file in $sqlFiles) {
    Write-Host "Executing: $($file.Name)" -ForegroundColor Cyan

    try {
        $dbConn = New-Object System.Data.SqlClient.SqlConnection
        $dbConn.ConnectionString = $connectionString
        $dbConn.Open()

        $sqlContent = Get-Content -Path $file.FullName -Raw -Encoding UTF8

        # Split by GO statements
        $batches = $sqlContent -split "(?m)^\s*GO\s*`$"

        foreach ($batch in $batches) {
            if ($batch.Trim().Length -gt 0) {
                $cmd = $dbConn.CreateCommand()
                $cmd.CommandText = $batch
                $cmd.CommandTimeout = 300
                $cmd.ExecuteNonQuery() | Out-Null
            }
        }

        $dbConn.Close()
        Write-Host "  + Success" -ForegroundColor Green
        $deployedCount++

    } catch {
        Write-Host "  X Failed: $($_.Exception.Message)" -ForegroundColor Red
        $failedCount++
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Deployment Complete" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Deployed: $deployedCount/8 files" -ForegroundColor Cyan

if ($failedCount -eq 0) {
    Write-Host "Status: SUCCESS!" -ForegroundColor Green
    Write-Host ""

    # Verify
    try {
        $verifyConn = New-Object System.Data.SqlClient.SqlConnection
        $verifyConn.ConnectionString = $connectionString
        $verifyConn.Open()

        $verifyCmd = $verifyConn.CreateCommand()
        $verifyCmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA IN ('Master','Shared','Transaction','Report','Auth')"
        $tableCount = $verifyCmd.ExecuteScalar()

        Write-Host "Tables created: $tableCount (expected: 43)" -ForegroundColor Cyan
        $verifyConn.Close()
    } catch {
        Write-Host "Note: Could not verify table count" -ForegroundColor Yellow
    }
} else {
    Write-Host "Status: FAILED ($failedCount files)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Database: $DatabaseName" -ForegroundColor Cyan
Write-Host "Server: $ServerName" -ForegroundColor Cyan
Write-Host ""
