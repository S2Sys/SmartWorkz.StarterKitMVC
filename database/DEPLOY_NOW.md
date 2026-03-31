# 🚀 DEPLOY NOW - SmartWorkz v4 Database

**Status:** ✅ Ready to Deploy
**Target Server:** 115.124.106.158
**Database:** Boilerplate
**User:** zenthil

---

## ⚡ Quick Deploy (1-2 minutes)

Open PowerShell in the database folder and run:

```powershell
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

**That's it!** The script will:
1. ✅ Verify all 8 SQL files exist
2. ✅ Test connection to 115.124.106.158
3. ✅ Check/Create "Boilerplate" database
4. ✅ Drop existing tables & schemas (cleanup)
5. ✅ Deploy all 8 migration scripts
6. ✅ Verify 43 tables created

**Note:** Database is KEPT, only tables & schemas are dropped

---

## 📋 What Gets Created

**Database:** Boilerplate

**Schemas (5):**
- Master (18 tables) - Configuration, Products, Menus, Customers
- Shared (7 tables) - SEO, Tags, Translations, Audit
- Transaction (1 table) - Financial transactions
- Report (4 tables) - Reports, Analytics
- Auth (13 tables) - Users, Roles, Permissions

**Total:** 43 tables across 5 schemas

**Seed Data:**
- 2 Tenants (DEFAULT, DEMO)
- 8 Languages
- 12 Countries
- 6 Roles
- 3 Menus with 13 Menu Items
- 5 Categories
- 12 Permissions

---

## 🎯 Step-by-Step Deployment

### 1. Navigate to Database Folder

```powershell
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"
```

### 2. Run Deployment Script

```powershell
.\QUICK-DEPLOY.ps1
```

### 3. Watch Output

You should see:
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
...

Deployment Complete
============================================

Deployed: 8/8 files
Status: SUCCESS!

Tables created: 43 (expected: 43)

Database: Boilerplate
Server: 115.124.106.158
```

---

## 📊 Expected Results

After deployment, verify in SQL Server Management Studio:

```sql
-- Check schemas (should return 5 rows)
SELECT name FROM sys.schemas
WHERE name IN ('Master','Shared','Transaction','Report','Auth');

-- Check table count (should return 43)
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Master','Shared','Transaction','Report','Auth');

-- Check seed data (should return 2)
SELECT COUNT(*) FROM Master.Tenants;

-- Check roles (should return 6)
SELECT COUNT(*) FROM Auth.Roles;
```

---

## 🔧 Alternative Deployment Methods

### Option A: Manual (SSMS)

1. Open SQL Server Management Studio
2. Connect to 115.124.106.158 (User: zenthil, Password: PinkPanther#1)
3. Create database: `CREATE DATABASE Boilerplate;`
4. Run each SQL file in order (001-008)

### Option B: Deploy-Database-v2.ps1 (More Detailed)

```powershell
$username = "zenthil"
$password = "PinkPanther#1"
.\Deploy-Database-v2.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username $username -Password $password
```

Shows detailed progress for each step.

### Option C: Deploy-Database.ps1 (Full Featured)

```powershell
$username = "zenthil"
$password = "PinkPanther#1"
.\Deploy-Database.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username $username -Password $password
```

Most comprehensive with error handling and validation.

---

## ✅ Deployment Scripts Provided

| Script | Purpose | Best For |
|--------|---------|----------|
| **QUICK-DEPLOY.ps1** | Simplest, hardcoded for your server | Quick deployment |
| **Deploy-Database-v2.ps1** | Enhanced with better path resolution | Detailed output |
| **Deploy-Database.ps1** | Full featured with error handling | Production |

---

## 🔐 Connection String

For your C# application (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=115.124.106.158;Initial Catalog=Boilerplate;User Id=zenthil;Password=PinkPanther#1;"
  }
}
```

---

## ⚠️ Troubleshooting

### Error: "Missing SQL scripts"
- ✅ All 8 files are in the database folder
- Run `dir | grep "\.sql"` to verify

### Error: "Cannot connect"
- Verify network access to 115.124.106.158
- Verify username/password correct
- Check firewall allows port 1433

### Error: "Permission denied"
- User "zenthil" needs CREATE DATABASE permission
- Contact your SQL Server admin

### Error: "Timeout"
- Network latency - try again or use longer timeout

---

## 📁 Files in Database Folder

```
S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\
├── 001_InitializeDatabase.sql        ← SQL files
├── 002_CreateTables_Master.sql
├── 003_CreateTables_Shared.sql
├── 004_CreateTables_Transaction.sql
├── 005_CreateTables_Report.sql
├── 006_CreateTables_Auth.sql
├── 007_SeedData.sql
├── 008_CreateIndexes.sql
│
├── QUICK-DEPLOY.ps1                  ← RUN THIS (simplest)
├── Deploy-Database-v2.ps1            ← Or this (detailed)
├── Deploy-Database.ps1               ← Or this (full featured)
│
├── README_DEPLOYMENT.md              ← Documentation
├── QUICK_START.md
├── DEPLOYMENT_SUMMARY.txt
└── POWERSHELL_TROUBLESHOOTING.md
```

---

## 🎯 Next Steps After Deployment

1. ✅ **Verify database** - Run SQL queries above
2. ✅ **Update appsettings.json** - Add connection string
3. ✅ **Step 2** - Create 43 domain entities (C# classes)
4. ✅ **Step 3** - Configure EF Core DbContexts
5. ✅ **Continue Phase 1** - Steps 4-7

---

## 📊 Phase 1 Progress

- ✅ **Step 1:** Database Scripts - COMPLETE
- ⏳ **Step 2:** Domain Entities (5-7h) - Next
- ⏳ **Step 3:** DbContexts (6-8h)
- ⏳ **Step 4:** Services (5-7h)
- ⏳ **Step 5:** API Endpoints (6-8h)
- ⏳ **Step 6:** DTOs (2-3h)
- ⏳ **Step 7:** Configuration (1-2h)

**Total Phase 1:** 34-45 hours

---

## ✨ Summary

All files are ready. Database deployment is a **single command**:

```powershell
cd "S:\02_Projects\Starter\03_Development\MVC\SmartWorkz.StarterKitMVC\database\"
.\QUICK-DEPLOY.ps1
```

**Expected Duration:** 5-10 minutes
**Expected Result:** 43 tables in "Boilerplate" database
**Status:** ✅ READY TO DEPLOY

---

**Go ahead and run it! 🚀**
