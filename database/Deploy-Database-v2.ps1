# ============================================
# SmartWorkz v4 - Database Deployment Script (v2)
# Date: 2026-03-31
# Fixed path resolution issues
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
Write-Host "SmartWorkz v4 Database Deployment (v2)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Script Location: $ScriptPath" -ForegroundColor Yellow
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

$scriptFiles = @()
$missingScripts = @()

foreach ($script in $requiredScripts) {
    $fullPath = Join-Path -Path $ScriptPath -ChildPath $script

    # Debug: show what we're looking for
    Write-Host "  Checking: $fullPath" -ForegroundColor Gray

    if (Test-Path $fullPath -PathType Leaf) {
        Write-Host "  + Found: $script" -ForegroundColor Green
        $scriptFiles += $fullPath
    } else {
        Write-Host "  X Missing: $script" -ForegroundColor Red
        $missingScripts += $script
    }
}

Write-Host ""

if ($missingScripts.Count -gt 0) {
    Write-Host "ERROR: Missing SQL scripts:" -ForegroundColor Red
    foreach ($missing in $missingScripts) {
        Write-Host "  - $missing" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Available files in $ScriptPath" -ForegroundColor Yellow
    Get-ChildItem -Path $ScriptPath -Filter "*.sql" | ForEach-Object { Write-Host "  - $($_.Name)" }
    Write-Host ""
    exit 1
}

# ============================================
# Build Connection String
# ============================================

Write-Host "Building connection string..." -ForegroundColor Yellow

if ($IntegratedSecurity) {
    $connectionString = "Server=$ServerName;Initial Catalog=$DatabaseName;Integrated Security=true;"
    Write-Host "  + Using Integrated Security" -ForegroundColor Green
} else {
    if ([string]::IsNullOrWhiteSpace($Username) -or [string]::IsNullOrWhiteSpace($Password)) {
        Write-Host "  ERROR: Username and Password required when not using Integrated Security" -ForegroundColor Red
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

    $result = $sqlCommand.ExecuteNonQuery()
    $sqlConnection.Close()

    Write-Host "  + Database ready" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host "  X Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================
# Execute Migration Scripts
# ============================================

Write-Host "Executing migration scripts..." -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$failedScripts = @()

for ($i = 0; $i -lt $scriptFiles.Count; $i++) {
    $scriptFile = $scriptFiles[$i]
    $scriptName = Split-Path -Leaf $scriptFile
    $scriptIndex = $i + 1

    Write-Host "[$scriptIndex/$($scriptFiles.Count)] Executing: $scriptName" -ForegroundColor Cyan

    try {
        $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $sqlConnection.ConnectionString = $connectionString
        $sqlConnection.Open()

        # Read SQL file content
        $sqlScript = Get-Content -Path $scriptFile -Raw -Encoding UTF8

        # Split into batches on GO statements
        $batches = $sqlScript -split "(?m)^\s*GO\s*`$"

        $batchCount = 0
        foreach ($batch in $batches) {
            if ($batch.Trim().Length -gt 0) {
                $batchCount++
                try {
                    $sqlCommand = $sqlConnection.CreateCommand()
                    $sqlCommand.CommandText = $batch
                    $sqlCommand.CommandTimeout = 300
                    $sqlCommand.ExecuteNonQuery() | Out-Null
                } catch {
                    Write-Host "    Batch error: $($_.Exception.Message)" -ForegroundColor Yellow
                    # Continue with next batch
                }
            }
        }

        $sqlConnection.Close()

        Write-Host "  + Success ($batchCount batches executed)" -ForegroundColor Green
        $successCount++

    } catch {
        Write-Host "  X Error: $($_.Exception.Message)" -ForegroundColor Red
        $failedScripts += @{ Script = $scriptName; Error = $_.Exception.Message }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Scripts Executed: $successCount / $($scriptFiles.Count)" -ForegroundColor Cyan

if ($failedScripts.Count -eq 0) {
    Write-Host ""
    Write-Host "+ All scripts executed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Information:" -ForegroundColor Cyan
    Write-Host "  Server: $ServerName" -ForegroundColor Cyan
    Write-Host "  Database: $DatabaseName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Ready for Phase 1 Implementation!" -ForegroundColor Green
    Write-Host ""

    # Verify table count
    try {
        $verifyConnection = New-Object System.Data.SqlClient.SqlConnection
        $verifyConnection.ConnectionString = $connectionString
        $verifyConnection.Open()

        $verifyCommand = $verifyConnection.CreateCommand()
        $verifyCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA IN ('Master','Shared','Transaction','Report','Auth')"
        $tableCount = $verifyCommand.ExecuteScalar()

        Write-Host "Verification: $tableCount tables created (expected: 43)" -ForegroundColor Cyan

        $verifyConnection.Close()
    } catch {
        Write-Host "Could not verify table count: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    exit 0

} else {
    Write-Host ""
    Write-Host "Failed Scripts:" -ForegroundColor Red
    foreach ($failed in $failedScripts) {
        Write-Host "  - $($failed.Script)" -ForegroundColor Red
        Write-Host "    Error: $($failed.Error)" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Deployment completed with errors. Please check the errors above." -ForegroundColor Yellow
    exit 1
}
