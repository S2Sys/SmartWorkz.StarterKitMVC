# PowerShell Deployment - Troubleshooting Guide

**Fixed Version:** Deploy-Database.ps1 (v2 - Fixed Encoding Issues)

---

## ✅ What Was Fixed

The original script had **encoding issues** with special characters. The corrected version:

- ✅ Uses ASCII characters only (no Unicode checkmarks)
- ✅ Uses proper PowerShell -split syntax with regex
- ✅ Proper error handling and try-catch blocks
- ✅ UTF-8 encoding compatible

---

## 🚀 How to Run (Corrected Script)

### Step 1: Open PowerShell as Administrator

Press `Win + X` → Select **"Windows PowerShell (Admin)"**

Or search for **PowerShell** in Start Menu → Right-click → **Run as administrator**

### Step 2: Navigate to Database Folder

```powershell
cd "C:\path\to\SmartWorkz.StarterKitMVC\database"
```

Example:
```powershell
cd "s:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database"
```

### Step 3: Run Deployment Script

**With Integrated Security (Easiest):**
```powershell
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

**With SQL Server Authentication:**
```powershell
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -Username "sa" -Password "YourPassword"
```

**For SQLEXPRESS:**
```powershell
.\Deploy-Database.ps1 -ServerName "MYCOMPUTER\SQLEXPRESS" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

### Step 4: Wait for Completion

Should see:
```
[1/8] Executing: 001_InitializeDatabase.sql
  + Success
[2/8] Executing: 002_CreateTables_Master.sql
  + Success
...
```

---

## 🔧 Common Errors & Solutions

### Error 1: "Cannot find path"

```
Cannot find path 'C:\path\to\database\Deploy-Database.ps1'
```

**Solution:**
1. Make sure you're in the correct folder
2. Run: `pwd` to see current directory
3. Check folder exists: `dir | grep database`

**Fix:**
```powershell
# Navigate correctly
cd "s:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database"

# Verify you're in right place
dir | grep Deploy-Database
```

---

### Error 2: "PowerShell execution policy"

```
PowerShell: File cannot be loaded because running scripts is disabled on this system
```

**Solution: Run as Administrator, then:**

```powershell
# Check current policy
Get-ExecutionPolicy

# Set to allow scripts (temporarily)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Or run script with bypass:
```powershell
powershell -ExecutionPolicy Bypass -File ".\Deploy-Database.ps1" -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

---

### Error 3: "SQL Server not running"

```
X Error: A network-related or instance-specific error occurred...
```

**Solution:**
1. Check SQL Server is running: `Services.msc`
2. Look for "SQL Server (SQLEXPRESS)" or "SQL Server (MSSQLSERVER)"
3. If stopped, right-click → **Start**

**Verify:**
```powershell
Get-Service | Where-Object { $_.Name -like "*SQL*" }
```

Should show:
```
Status   Name
------   ----
Running  MSSQLSERVER
Running  SQLSERVERAGENT
```

---

### Error 4: "Login failed for user 'sa'"

```
X Error: Login failed for user 'sa'.
```

**Solution:**
1. Verify SQL Server uses **SQL Server Authentication** (not Windows-only)
2. Verify 'sa' password is correct
3. Try with **Integrated Security** instead:

```powershell
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

**If using SQL Auth, set password correctly:**
```powershell
$username = "sa"
$password = "ActualPassword123"  # Use real password

.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -Username $username -Password $password
```

---

### Error 5: "Cannot open database 'SmartWorkz_v4'"

```
X Error: Cannot open database requested in login. Login fails. Login failed for user...
```

**Solution:** Database doesn't exist yet. The script will create it. If it fails:

1. Create database manually in SQL Server Management Studio:
   ```sql
   CREATE DATABASE SmartWorkz_v4;
   ```

2. Or verify master database access:
   ```powershell
   $conn = New-Object System.Data.SqlClient.SqlConnection
   $conn.ConnectionString = "Server=localhost;Initial Catalog=master;Integrated Security=true;"
   $conn.Open()
   Write-Host "Connected to master"
   $conn.Close()
   ```

---

### Error 6: "Timeout expired"

```
X Error: Timeout expired. The timeout period elapsed prior to completion...
```

**Solution:** Tables are taking too long to create. Edit script to increase timeout:

```powershell
# In Deploy-Database.ps1, find this line (around 167):
$sqlCommand.CommandTimeout = 300

# Change to 600 (10 minutes):
$sqlCommand.CommandTimeout = 600
```

Or run script with more time:
```powershell
$global:SqlTimeout = 600
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

---

### Error 7: "String was not recognized as a valid Boolean"

```
X Error: String '...' was not recognized as a valid Boolean value.
```

**Solution:** -IntegratedSecurity flag syntax issue. Use exactly:

```powershell
# CORRECT
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity

# WRONG (don't do this)
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity $true
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity:$true
```

---

## ✅ Verification Checklist

After successful deployment, verify:

```powershell
# 1. Test connection
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
$conn.Open()
Write-Host "Connected successfully!"
$conn.Close()

# 2. Check table count
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES"
$count = $cmd.ExecuteScalar()

Write-Host "Total tables: $count (should be 43)"
$conn.Close()

# 3. Run TEST-DEPLOYMENT.ps1
.\TEST-DEPLOYMENT.ps1
```

---

## 🎯 Quick Test

To verify script works before full deployment:

```powershell
# Test 1: Check SQL files are present
dir | grep "001_Initialize"
dir | grep "002_CreateTables"
dir | grep "003_CreateTables"

# Test 2: Check syntax (no errors)
.\Deploy-Database.ps1 -?

# Test 3: Run full test
.\TEST-DEPLOYMENT.ps1
```

---

## 📋 Files Provided

| File | Purpose |
|------|---------|
| **Deploy-Database.ps1** | Main deployment script (FIXED VERSION) |
| **TEST-DEPLOYMENT.ps1** | Pre-deployment test (NEW) |
| **DEPLOYMENT_COMMANDS.ps1** | Copy-paste command examples |
| **QUICK_START.md** | Quick reference guide |
| **README_DEPLOYMENT.md** | Comprehensive guide |

---

## 🔍 Debug Mode

Run script with verbose output:

```powershell
$VerbosePreference = "Continue"
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity -Verbose
```

---

## 🆘 Still Having Issues?

1. **Run TEST-DEPLOYMENT.ps1 first** - checks prerequisites
2. **Verify SQL Server is running** - Services.msc
3. **Check network connectivity** - can you connect with SSMS?
4. **Check firewall** - port 1433 not blocked?
5. **Try with Integrated Security** - simplest option
6. **Check file encoding** - should be UTF-8

---

## ✨ Success Indicators

After running the script, you should see:

```
[1/8] Executing: 001_InitializeDatabase.sql
  + Success
[2/8] Executing: 002_CreateTables_Master.sql
  + Success
[3/8] Executing: 003_CreateTables_Shared.sql
  + Success
[4/8] Executing: 004_CreateTables_Transaction.sql
  + Success
[5/8] Executing: 005_CreateTables_Report.sql
  + Success
[6/8] Executing: 006_CreateTables_Auth.sql
  + Success
[7/8] Executing: 007_SeedData.sql
  + Success
[8/8] Executing: 008_CreateIndexes.sql
  + Success

============================================
Deployment Summary
============================================

Scripts Executed: 8 / 8

+ All scripts executed successfully!

Database Information:
  - Server: localhost
  - Database: SmartWorkz_v4

Ready for Phase 1 Implementation!
```

---

## 🎓 PowerShell Tips

### Show version
```powershell
$PSVersionTable
```

### List installed SQL Server instances
```powershell
$regPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server"
Get-ItemProperty $regPath
```

### Test SQL Server port
```powershell
Test-NetConnection -ComputerName localhost -Port 1433
```

### Check SQL Server services
```powershell
Get-Service | Where-Object { $_.Name -like "*SQL*" } | Format-Table Name,Status
```

---

**Status: ✅ Scripts Fixed and Ready**

All PowerShell encoding issues have been resolved. Deploy with confidence!
