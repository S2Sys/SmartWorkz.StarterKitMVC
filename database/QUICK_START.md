# SmartWorkz v4 Database - Quick Start

**TL;DR:** Deploy database in 3 steps

---

## 🚀 Fastest Deployment (PowerShell)

### Prerequisites
- SQL Server 2016+ running
- PowerShell 5.0+
- Network access to SQL Server

### 3-Step Deployment

**Step 1:** Open PowerShell as Administrator

```powershell
cd C:\path\to\SmartWorkz.StarterKitMVC\database
```

**Step 2:** Run deployment script

```powershell
# Windows Authentication (easiest)
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

**Step 3:** Wait for completion ✓

```
✓ All scripts executed successfully!
```

---

## 📋 Connection Information

Add to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
  }
}
```

---

## 📊 What Gets Created

✅ **5 Schemas:** Master, Shared, Transaction, Report, Auth
✅ **43 Tables:** Products, Categories, Menus, Users, Roles, SEO, Tags, etc.
✅ **Seed Data:** 2 Tenants, 8 Languages, 12 Countries, 6 Roles, 3 Menus
✅ **Indexes:** Performance optimized for polymorphic queries
✅ **Audit Tables:** Track all changes

---

## 🔧 Troubleshooting

| Error | Solution |
|-------|----------|
| "Cannot open database" | Database auto-created by script |
| "Login failed" | Use `-IntegratedSecurity` or verify sa password |
| "Timeout expired" | Script default is 5 min, increase if needed |
| "Schema already exists" | Drop DB: `DROP DATABASE SmartWorkz_v4` |

---

## 📁 Files Provided

```
database/
├── 001_InitializeDatabase.sql      ← Schemas
├── 002_CreateTables_Master.sql     ← 18 tables
├── 003_CreateTables_Shared.sql     ← 7 tables
├── 004_CreateTables_Transaction.sql ← 1 table
├── 005_CreateTables_Report.sql     ← 4 tables
├── 006_CreateTables_Auth.sql       ← 13 tables
├── 007_SeedData.sql                ← Initial data
├── 008_CreateIndexes.sql           ← Indexes & stats
├── Deploy-Database.ps1             ← Automation script
├── README_DEPLOYMENT.md            ← Full guide
└── QUICK_START.md                  ← This file
```

---

## ✅ Verify Deployment

```sql
-- Should return 43 tables
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Master','Shared','Transaction','Report','Auth');

-- Should return 2 tenants
SELECT * FROM Master.Tenants;

-- Should return 3 menus
SELECT * FROM Master.Menus;
```

---

## 🎯 Next Phase

After database deployment → **Step 2: Create Domain Entities**

Run PowerShell again when ready:
```powershell
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

---

**Status:** ✅ Step 1 Complete - Database Ready for Phase 1 Implementation
