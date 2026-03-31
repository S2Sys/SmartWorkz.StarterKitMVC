# Simple test to verify script syntax
# Run: .\TEST-DEPLOYMENT.ps1

Write-Host "SmartWorkz v4 Database Deployment Test" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Host "Script location: $scriptPath" -ForegroundColor Yellow

# Test 2: List SQL files
Write-Host ""
Write-Host "SQL Files found:" -ForegroundColor Yellow
$sqlFiles = Get-ChildItem -Path $scriptPath -Filter "*.sql" | Select-Object Name
foreach ($file in $sqlFiles) {
    Write-Host "  + $($file.Name)" -ForegroundColor Green
}

# Test 3: Check SQL Server connectivity
Write-Host ""
Write-Host "Testing SQL Server connectivity..." -ForegroundColor Yellow

try {
    $testConnection = New-Object System.Data.SqlClient.SqlConnection
    $testConnection.ConnectionString = "Server=localhost;Initial Catalog=master;Integrated Security=true;"
    $testConnection.Open()
    Write-Host "  + Connected to: localhost" -ForegroundColor Green

    $sqlCommand = $testConnection.CreateCommand()
    $sqlCommand.CommandText = "SELECT @@VERSION as Version"
    $result = $sqlCommand.ExecuteReader()

    if ($result.Read()) {
        $version = $result['Version']
        Write-Host "  + SQL Server Version: $version" -ForegroundColor Green
    }

    $result.Close()
    $sqlConnection.Close()
} catch {
    Write-Host "  X Cannot connect to SQL Server" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure SQL Server is running!" -ForegroundColor Yellow
}

# Test 4: Show deployment command
Write-Host ""
Write-Host "Ready to deploy! Run this command:" -ForegroundColor Cyan
Write-Host ""
Write-Host ".\Deploy-Database.ps1 -ServerName 'localhost' -DatabaseName 'SmartWorkz_v4' -IntegratedSecurity" -ForegroundColor Yellow
Write-Host ""
Write-Host "Or for SQL Server Authentication:" -ForegroundColor Cyan
Write-Host ""
Write-Host ".\Deploy-Database.ps1 -ServerName 'localhost' -DatabaseName 'SmartWorkz_v4' -Username 'sa' -Password 'YourPassword'" -ForegroundColor Yellow
Write-Host ""
