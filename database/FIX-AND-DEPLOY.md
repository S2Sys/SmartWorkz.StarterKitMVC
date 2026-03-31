# ✅ Fix & Deploy Guide

**Issue Found:** SQL files have hardcoded database name "SmartWorkz_v4"
**Solution:** Run FIX-SQL-FILES.ps1 first, then deploy

---

## 🔧 Step 1: Fix SQL Files

Run this to replace database name in all SQL files:

```powershell
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"

.\FIX-SQL-FILES.ps1 -DatabaseName "Boilerplate"
```

**Output:**
```
============================================
Fixing SQL Files
============================================

Replacing 'USE SmartWorkz_v4;' with 'USE Boilerplate;'

  + Fixed: 001_InitializeDatabase.sql
  + Fixed: 002_CreateTables_Master.sql
  + Fixed: 003_CreateTables_Shared.sql
  + Fixed: 004_CreateTables_Transaction.sql
  + Fixed: 005_CreateTables_Report.sql
  + Fixed: 006_CreateTables_Auth.sql
  + Fixed: 007_SeedData.sql
  + Fixed: 008_CreateIndexes.sql

Fixed: 8 files
```

---

## 🚀 Step 2: Deploy

Now run the deployment:

```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

---

## 📋 Complete Steps (Copy-Paste Ready)

```powershell
# Navigate to database folder
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"

# Step 1: Fix SQL files
.\FIX-SQL-FILES.ps1 -DatabaseName "Boilerplate"

# Step 2: Deploy
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

---

## ✅ Expected Result

**After fix:**
- All SQL files updated to use "Boilerplate"

**After deployment:**
```
Deployed: 8/8 files
Status: SUCCESS!

Tables created: 43 (expected: 43)

Database: Boilerplate
Server: 115.124.106.158
```

---

## 🎯 Using Different Database Names

You can use any database name:

```powershell
# Fix for different database name
.\FIX-SQL-FILES.ps1 -DatabaseName "MyDatabase"

# Then deploy
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "MyDatabase" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

---

## 🔄 What Was Fixed

### In QUICK-DEPLOY.ps1
- Fixed schema drop SQL syntax (removed invalid `+` operator)
- Changed to proper T-SQL string concatenation

### In SQL Files
- Will replace `USE SmartWorkz_v4;` with `USE Boilerplate;` (or your database name)

---

## 📊 Files Updated

- **QUICK-DEPLOY.ps1** - Fixed schema drop syntax
- **FIX-SQL-FILES.ps1** - New script to fix database names in SQL files
- **FIX-AND-DEPLOY.md** - This guide

---

## ✨ Ready to Deploy!

1. Run FIX-SQL-FILES.ps1
2. Run QUICK-DEPLOY.ps1
3. Done! ✅

```powershell
# One complete command
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"; .\FIX-SQL-FILES.ps1 -DatabaseName "Boilerplate"; .\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

---

**Status:** ✅ Ready to Fix & Deploy
