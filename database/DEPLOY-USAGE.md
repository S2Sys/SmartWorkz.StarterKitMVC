# QUICK-DEPLOY.ps1 - Usage Guide

**Now with parameterized deployment + automatic database cleanup!**

---

## 🚀 Quick Start

```powershell
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"

.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

**Note:** If database already exists, it will be automatically dropped and recreated from scratch.

---

## 📋 Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `-ServerName` | YES | SQL Server address/hostname | `"115.124.106.158"` |
| `-DatabaseName` | YES | Database name to create | `"Boilerplate"` |
| `-Username` | YES | SQL Server login username | `"zenthil"` |
| `-Password` | YES | SQL Server login password | `"PinkPanther#1"` |
| `-SkipCleanup` | NO | Skip dropping existing database | (omit to cleanup) |

---

## 💡 Usage Examples

### Example 1: Remote Server (Your Setup)
```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

### Example 2: Local SQL Server
```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "localhost" `
  -DatabaseName "SmartWorkz_v4" `
  -Username "sa" `
  -Password "YourPassword"
```

### Example 3: Named Instance
```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "MYCOMPUTER\SQLEXPRESS" `
  -DatabaseName "SmartWorkz_Development" `
  -Username "sa" `
  -Password "Password123"
```

### Example 4: Using Variables
```powershell
$server = "115.124.106.158"
$db = "Boilerplate"
$user = "zenthil"
$pass = "PinkPanther#1"

.\QUICK-DEPLOY.ps1 -ServerName $server -DatabaseName $db -Username $user -Password $pass
```

### Example 5: Using Environment Variables (More Secure)
```powershell
$env:SQL_SERVER = "115.124.106.158"
$env:SQL_DATABASE = "Boilerplate"
$env:SQL_USER = "zenthil"
$env:SQL_PASSWORD = "PinkPanther#1"

.\QUICK-DEPLOY.ps1 `
  -ServerName $env:SQL_SERVER `
  -DatabaseName $env:SQL_DATABASE `
  -Username $env:SQL_USER `
  -Password $env:SQL_PASSWORD
```

### Example 6: Fresh Deployment (with cleanup - default)
```powershell
# Automatically drops and recreates the database
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

### Example 7: Keep Existing Data (skip cleanup)
```powershell
# Does NOT drop existing database
# Only creates new tables if they don't exist
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1" `
  -SkipCleanup
```

---

## 🧹 Table & Schema Cleanup Feature

### How It Works

**Default Behavior (Cleanup Enabled):**
1. ✅ Checks if database exists
2. ✅ If NOT exists → Creates new database
3. ✅ **Drops existing tables** (if any)
4. ✅ **Drops existing schemas** (Master, Shared, Transaction, Report, Auth)
5. ✅ Deploys fresh schemas and tables
6. ✅ **Database itself is PRESERVED** (not dropped)

```powershell
# Default: Cleanup enabled (drop tables & schemas only, keep database)
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

**Skip Cleanup:**
```powershell
# Skip cleanup: Keep existing tables & schemas
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1" -SkipCleanup
```

### When to Use Each

| Scenario | Command | What Happens |
|----------|---------|--------------|
| **Fresh deployment** | Default | ✅ Creates DB, tables, schemas |
| **Clean tables/schemas** | Default | ✅ Drops tables & schemas, keeps DB |
| **Reset schema** | Default | ✅ Fresh tables & schemas |
| **Update without cleanup** | -SkipCleanup | ✅ Keep existing tables, add new ones |
| **Keep everything** | -SkipCleanup | ✅ Preserve all data |

---

## 🔧 Script Features

✅ **Validates all 8 SQL files exist**
✅ **Tests connection before deployment**
✅ **Creates database if not exists**
✅ **Deploys all migrations in order**
✅ **Handles GO statements correctly**
✅ **Verifies table count (should be 43)**
✅ **Shows detailed progress**
✅ **Friendly error messages**

---

## 📊 Expected Output

```
============================================
SmartWorkz v4 - Quick Deploy
============================================

Configuration:
  Server: 115.124.106.158
  Database: Boilerplate
  User: zenthil

SQL Files to Deploy:
  + 001_InitializeDatabase.sql
  + 002_CreateTables_Master.sql
  + 003_CreateTables_Shared.sql
  + 004_CreateTables_Transaction.sql
  + 005_CreateTables_Report.sql
  + 006_CreateTables_Auth.sql
  + 007_SeedData.sql
  + 008_CreateIndexes.sql

Testing connection to 115.124.106.158...
  + Connected successfully!

Creating database 'Boilerplate'...
  + Database ready

Deploying SQL files...

Executing: 001_InitializeDatabase.sql
  + Success
Executing: 002_CreateTables_Master.sql
  + Success
Executing: 003_CreateTables_Shared.sql
  + Success
Executing: 004_CreateTables_Transaction.sql
  + Success
Executing: 005_CreateTables_Report.sql
  + Success
Executing: 006_CreateTables_Auth.sql
  + Success
Executing: 007_SeedData.sql
  + Success
Executing: 008_CreateIndexes.sql
  + Success

============================================
Deployment Complete
============================================

Deployed: 8/8 files
Status: SUCCESS!

Tables created: 43 (expected: 43)

Database: Boilerplate
Server: 115.124.106.158
```

---

## ❌ Common Errors & Solutions

### Error: "Positional parameter cannot be found"
**Cause:** Missing required parameter
**Solution:** Provide all 4 required parameters
```powershell
# WRONG - missing parameters
.\QUICK-DEPLOY.ps1

# CORRECT - all parameters provided
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

### Error: "Connection failed"
**Cause:** Server not reachable or wrong credentials
**Solution:** Verify credentials and network access
```powershell
# Test connection manually
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=115.124.106.158;Initial Catalog=master;User Id=zenthil;Password=PinkPanther#1;"
$conn.Open()
Write-Host "Connection OK!"
$conn.Close()
```

### Error: "SQL files not found"
**Cause:** SQL files 002-008 not in same folder
**Solution:** Verify all 8 files are present
```powershell
# Check files
dir *.sql | grep "^00[1-8]_"

# Should show all 8 files
```

### Error: "Cannot find path"
**Cause:** Not in the correct directory
**Solution:** Navigate to database folder
```powershell
# Correct path
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"

# Verify
dir QUICK-DEPLOY.ps1
```

---

## 🎯 What Gets Created

**Database:** Boilerplate (or your specified name)

**Schemas (5):**
- Master (18 tables)
- Shared (7 tables)
- Transaction (1 table)
- Report (4 tables)
- Auth (13 tables)

**Total:** 43 tables

**Seed Data:**
- 2 Tenants
- 8 Languages
- 12 Countries
- 6 Roles
- 3 Menus with 13 Menu Items
- 5 Categories
- And more...

---

## 🔐 Security Note

**Store passwords securely:**

```powershell
# Option 1: Use environment variable
$env:SQL_PASSWORD = Read-Host "Enter password" -AsSecureString

# Option 2: Use Azure Key Vault
$password = (Get-AzKeyVaultSecret -VaultName "myVault" -Name "SqlPassword").SecretValueText

# Option 3: Use Windows Credential Manager
$cred = Get-Credential
```

---

## ⚠️ WARNING: Cleanup Drops Tables & Schemas

**By default, the script drops tables and schemas!**

This means:
- ⚠️ All existing tables in (Master, Shared, Transaction, Report, Auth) will be DROPPED
- ⚠️ All existing schemas will be DROPPED
- ⚠️ All data in those tables will be DELETED
- ✅ Database itself is PRESERVED (not dropped)

Use **-SkipCleanup** to keep existing tables and schemas:

```powershell
# SAFE: Keeps existing tables & schemas
.\QUICK-DEPLOY.ps1 -ServerName "..." -DatabaseName "..." -Username "..." -Password "..." -SkipCleanup

# DESTRUCTIVE: Drops tables & schemas (default)
.\QUICK-DEPLOY.ps1 -ServerName "..." -DatabaseName "..." -Username "..." -Password "..."
```

**Key Difference:**
- Database: ✅ KEPT (not dropped)
- Schemas: ⚠️ DROPPED (recreated)
- Tables: ⚠️ DROPPED (recreated)
- Data: ⚠️ DELETED (replaced with seed data)

---

## ✅ Verification After Deployment

```powershell
# Query to verify tables
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=115.124.106.158;Initial Catalog=Boilerplate;User Id=zenthil;Password=PinkPanther#1;"
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES"
$count = $cmd.ExecuteScalar()

Write-Host "Total tables: $count (expected: 43)"
$conn.Close()
```

---

## 🚀 Complete One-Liner

```powershell
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"; .\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

---

## 📞 Still Having Issues?

1. Check all 8 SQL files exist
2. Verify server connectivity: `Test-NetConnection -ComputerName "115.124.106.158" -Port 1433`
3. Verify credentials work in SQL Server Management Studio
4. Check firewall allows port 1433
5. Review error message carefully

---

**Status:** ✅ Ready to Deploy

Run the script with your parameters and enjoy! 🚀
