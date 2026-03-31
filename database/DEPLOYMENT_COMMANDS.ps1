# ============================================
# SmartWorkz v4 - Database Deployment Commands
# Quick reference for all deployment scenarios
# Copy-paste ready PowerShell commands
# ============================================

# ============================================
# SCENARIO 1: Local SQL Server with Integrated Security (EASIEST)
# ============================================

Write-Host "========== Scenario 1: Local SQL Server + Integrated Security ==========" -ForegroundColor Cyan

# Navigate to database folder
cd "C:\path\to\SmartWorkz.StarterKitMVC\database"

# Deploy to localhost
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity

# ============================================
# SCENARIO 2: Named Instance (SQLEXPRESS, LocalDB)
# ============================================

Write-Host "========== Scenario 2: SQL Server Express ==========" -ForegroundColor Cyan

cd "C:\path\to\SmartWorkz.StarterKitMVC\database"

# Deploy to SQLEXPRESS
.\Deploy-Database.ps1 -ServerName "MYCOMPUTER\SQLEXPRESS" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity

# Or use LocalDB
.\Deploy-Database.ps1 -ServerName "(localdb)\mssqllocaldb" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity

# ============================================
# SCENARIO 3: SQL Server with SQL Authentication (sa user)
# ============================================

Write-Host "========== Scenario 3: SQL Server Authentication ==========" -ForegroundColor Cyan

cd "C:\path\to\SmartWorkz.StarterKitMVC\database"

# Set credentials
$username = "sa"
$password = "YourPassword123"

# Deploy with SQL auth
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -Username $username -Password $password

# ============================================
# SCENARIO 4: Remote SQL Server
# ============================================

Write-Host "========== Scenario 4: Remote SQL Server ==========" -ForegroundColor Cyan

cd "C:\path\to\SmartWorkz.StarterKitMVC\database"

# Replace with your server IP/name
$serverName = "192.168.1.100"
$username = "sa"
$password = "RemotePassword123"

.\Deploy-Database.ps1 -ServerName $serverName -DatabaseName "SmartWorkz_v4" -Username $username -Password $password

# ============================================
# SCENARIO 5: Azure SQL Server
# ============================================

Write-Host "========== Scenario 5: Azure SQL Server ==========" -ForegroundColor Cyan

cd "C:\path\to\SmartWorkz.StarterKitMVC\database"

$serverName = "myserver.database.windows.net"
$username = "azureuser"
$password = "AzurePassword123!"

.\Deploy-Database.ps1 -ServerName $serverName -DatabaseName "SmartWorkz_v4" -Username $username -Password $password

# ============================================
# VERIFICATION COMMANDS
# ============================================

Write-Host "========== Verification Queries ==========" -ForegroundColor Cyan

# After deployment, verify with these SQL queries

$connectionString = "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"

$sqlConnection = New-Object System.Data.SqlClient.SqlConnection
$sqlConnection.ConnectionString = $connectionString
$sqlConnection.Open()

# Check Schemas
Write-Host "Checking Schemas (should be 5)..." -ForegroundColor Yellow
$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT COUNT(*) as SchemaCount FROM sys.schemas WHERE name IN ('Master','Shared','Transaction','Report','Auth')"
$result = $sqlCommand.ExecuteScalar()
Write-Host "  Schemas found: $result" -ForegroundColor Green

# Check Total Tables
Write-Host "Checking Total Tables (should be 43)..." -ForegroundColor Yellow
$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA IN ('Master','Shared','Transaction','Report','Auth')"
$result = $sqlCommand.ExecuteScalar()
Write-Host "  Tables found: $result" -ForegroundColor Green

# Check Tenants
Write-Host "Checking Tenants (should be 2)..." -ForegroundColor Yellow
$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT COUNT(*) as TenantCount FROM Master.Tenants"
$result = $sqlCommand.ExecuteScalar()
Write-Host "  Tenants found: $result" -ForegroundColor Green

# Check Languages
Write-Host "Checking Languages (should be 8)..." -ForegroundColor Yellow
$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT COUNT(*) as LanguageCount FROM Master.Languages"
$result = $sqlCommand.ExecuteScalar()
Write-Host "  Languages found: $result" -ForegroundColor Green

# Check Roles
Write-Host "Checking Roles (should be 6)..." -ForegroundColor Yellow
$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT COUNT(*) as RoleCount FROM Auth.Roles"
$result = $sqlCommand.ExecuteScalar()
Write-Host "  Roles found: $result" -ForegroundColor Green

$sqlConnection.Close()

# ============================================
# TROUBLESHOOTING COMMANDS
# ============================================

Write-Host "========== Troubleshooting ==========" -ForegroundColor Cyan

# 1. Check if SQL Server is running
Write-Host "Checking SQL Server service..." -ForegroundColor Yellow
Get-Service | Where-Object { $_.Name -like "*SQL*" } | Select-Object Name, Status

# 2. Test connection to SQL Server
Write-Host "Testing connection..." -ForegroundColor Yellow
try {
    $testConnection = New-Object System.Data.SqlClient.SqlConnection
    $testConnection.ConnectionString = "Server=localhost;Initial Catalog=master;Integrated Security=true;"
    $testConnection.Open()
    Write-Host "  + Connection successful!" -ForegroundColor Green
    $testConnection.Close()
} catch {
    Write-Host "  X Connection failed: $_" -ForegroundColor Red
}

# 3. List existing databases
Write-Host "Listing SmartWorkz databases..." -ForegroundColor Yellow
$sqlConnection = New-Object System.Data.SqlClient.SqlConnection
$sqlConnection.ConnectionString = "Server=localhost;Initial Catalog=master;Integrated Security=true;"
$sqlConnection.Open()

$sqlCommand = $sqlConnection.CreateCommand()
$sqlCommand.CommandText = "SELECT name FROM sys.databases WHERE name LIKE 'SmartWorkz%' ORDER BY name"

$result = $sqlCommand.ExecuteReader()
while ($result.Read()) {
    Write-Host "  - $($result['name'])" -ForegroundColor Green
}
$result.Close()
$sqlConnection.Close()

# ============================================
# RECOVERY COMMANDS
# ============================================

Write-Host "========== Recovery/Reset ==========" -ForegroundColor Cyan

Write-Host "To reset and redeploy (WARNING: Deletes all data):" -ForegroundColor Yellow
Write-Host "  sqlcmd -S localhost -d master -E -Q `"DROP DATABASE SmartWorkz_v4`"" -ForegroundColor Cyan

Write-Host "  Then run deployment again:" -ForegroundColor Yellow
Write-Host "  .\Deploy-Database.ps1 -ServerName localhost -DatabaseName SmartWorkz_v4 -IntegratedSecurity" -ForegroundColor Cyan

# ============================================
# END
# ============================================

Write-Host ""
Write-Host "+ Deployment commands ready!" -ForegroundColor Green
Write-Host "Choose a scenario above and copy-paste the commands" -ForegroundColor Cyan
Write-Host ""
