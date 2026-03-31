# SmartWorkz v4 - Database Deployment Guide

**Version:** 4.0.0
**Date:** 2026-03-31
**Phase:** Phase 1 Implementation - Database Setup

## Overview

This guide walks through deploying the SmartWorkz v4 database to SQL Server. The deployment consists of 8 migration scripts totaling **43 tables across 5 schemas**.

### Database Architecture

```
SmartWorkz_v4
├── Master (18 tables)
│   ├── Tenants, Countries, Currencies, Languages, TimeZones
│   ├── Configuration, FeatureFlags
│   ├── Menus, MenuItems (with HierarchyId)
│   ├── Categories, Products (with hierarchy support)
│   ├── GeoHierarchy, GeolocationPages
│   ├── CustomPages, BlogPosts
│   ├── Customers, Suppliers, Inventory
│
├── Shared (7 tables)
│   ├── SeoMeta (polymorphic SEO metadata)
│   ├── Tags (polymorphic tagging)
│   ├── Translations (multi-language support)
│   ├── Notifications
│   ├── AuditLogs
│   ├── FileStorage
│   └── EmailQueue
│
├── Transaction (1 table)
│   └── TransactionLog (financial transactions)
│
├── Report (4 tables)
│   ├── Reports
│   ├── ReportSchedules
│   ├── ReportData
│   └── Analytics
│
└── Auth (13 tables)
    ├── Users, Roles, Permissions
    ├── UserRoles, RolePermissions, UserPermissions
    ├── RefreshTokens
    ├── LoginAttempts, AuditTrail
    ├── TenantUsers
    ├── PasswordResetTokens, EmailVerificationTokens
    └── TwoFactorTokens
```

## Migration Scripts

| File | Description | Tables |
|------|-------------|--------|
| 001_InitializeDatabase.sql | Create schemas (Master, Shared, Transaction, Report, Auth) | - |
| 002_CreateTables_Master.sql | Master schema with configuration, navigation, products | 18 |
| 003_CreateTables_Shared.sql | Shared schema with SEO, Tags, Translations, Audit | 7 |
| 004_CreateTables_Transaction.sql | Transaction schema with transaction logging | 1 |
| 005_CreateTables_Report.sql | Report schema with reports, schedules, analytics | 4 |
| 006_CreateTables_Auth.sql | Auth schema with users, roles, permissions | 13 |
| 007_SeedData.sql | Seed countries, languages, roles, menus, categories | - |
| 008_CreateIndexes.sql | Performance indexes and statistics | - |

**Total:** 43 tables + Seed data + Indexes

## Prerequisites

- **SQL Server 2016+** (SQL Server 2019+ recommended)
- **PowerShell 5.0+**
- **Network access** to SQL Server instance
- **Admin credentials** for database creation

## Deployment Methods

### Method 1: PowerShell Script (Recommended)

Most straightforward and handles all steps automatically.

#### Step 1: Create the Database First (SQL Server Management Studio)

```sql
CREATE DATABASE SmartWorkz_v4;
```

Or use PowerShell to create it automatically (script can do this).

#### Step 2: Run the Deployment Script

**With Integrated Security (Windows Authentication):**

```powershell
cd C:\path\to\SmartWorkz.StarterKitMVC\database

.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

**With SQL Server Authentication:**

```powershell
cd C:\path\to\SmartWorkz.StarterKitMVC\database

$username = "sa"
$password = "YourPassword123"

.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -Username $username -Password $password
```

**With Custom SQL Server Instance (Named Instance):**

```powershell
.\Deploy-Database.ps1 -ServerName "MYCOMPUTER\SQLEXPRESS" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

#### Step 3: Verify Deployment

```powershell
# Check if all tables were created
$connectionString = "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA != 'sys'"
$result = $command.ExecuteScalar()
Write-Host "Total tables created: $result" -ForegroundColor Green
$connection.Close()
```

---

### Method 2: SQL Server Management Studio (SSMS)

Manual approach for those preferring SSMS.

#### Step 1: Create Database

1. Open **SQL Server Management Studio**
2. Right-click **Databases** → **New Database**
3. Database name: `SmartWorkz_v4`
4. Click **OK**

#### Step 2: Execute Scripts in Order

1. Right-click `SmartWorkz_v4` database → **New Query**
2. Copy-paste content from `001_InitializeDatabase.sql`
3. Execute (F5 or Ctrl+E)
4. Repeat for files 002-008 in order

**Important:** Execute them in order! Each depends on previous tables being created.

---

### Method 3: Batch File (Alternative)

Create `deploy.bat` in the database folder:

```batch
@echo off
setlocal enabledelayedexpansion

set SERVER=localhost
set DATABASE=SmartWorkz_v4
set USERNAME=sa
set PASSWORD=YourPassword

echo Deploying SmartWorkz v4 Database...

for %%F in (001_InitializeDatabase.sql 002_CreateTables_Master.sql 003_CreateTables_Shared.sql 004_CreateTables_Transaction.sql 005_CreateTables_Report.sql 006_CreateTables_Auth.sql 007_SeedData.sql 008_CreateIndexes.sql) do (
    echo Executing %%F...
    sqlcmd -S %SERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i "%%F"
    if !errorlevel! neq 0 (
        echo Error executing %%F
        exit /b 1
    )
)

echo Deployment completed successfully!
pause
```

Then run:
```batch
deploy.bat
```

---

## Troubleshooting

### Error: "Cannot open database 'SmartWorkz_v4'"

**Solution:** Database needs to be created first. Either:
- Create it in SSMS, OR
- Modify the script to create it, OR
- Run PowerShell with auto-create enabled

### Error: "The target database contains a schema 'Master'"

**Solution:** The scripts use `IF NOT EXISTS` checks, but if schemas exist:

```sql
-- Drop existing database to start fresh
DROP DATABASE SmartWorkz_v4;

-- Then rerun deployment
```

### Error: "Timeout expired"

**Solution:** Increase timeout in PowerShell script

```powershell
$sqlCommand.CommandTimeout = 600 # 10 minutes instead of 5
```

### Error: "Login failed for user 'sa'"

**Solution:**
1. Verify SQL Server is running
2. Check username/password
3. Verify SQL Server Authentication is enabled (not Windows-only)
4. Restart SQL Server if needed

### Error: "Cannot find path" in PowerShell

**Solution:** Navigate to correct directory first

```powershell
cd "C:\path\to\SmartWorkz.StarterKitMVC\database"
.\Deploy-Database.ps1 -ServerName "localhost" -DatabaseName "SmartWorkz_v4" -IntegratedSecurity
```

---

## Verification Checklist

After deployment, verify:

```sql
-- 1. Check all schemas created
SELECT name FROM sys.schemas WHERE name IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth');

-- 2. Count tables per schema
SELECT TABLE_SCHEMA, COUNT(*) as TableCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA != 'sys'
GROUP BY TABLE_SCHEMA;

-- 3. Verify seed data
SELECT COUNT(*) as TenantCount FROM Master.Tenants;
SELECT COUNT(*) as LanguageCount FROM Master.Languages;
SELECT COUNT(*) as MenuCount FROM Master.Menus;
SELECT COUNT(*) as RoleCount FROM Auth.Roles;

-- 4. Check HierarchyId columns
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE DATA_TYPE = 'hierarchyid';

-- 5. Verify indexes
SELECT COUNT(*) as IndexCount
FROM sys.indexes
WHERE object_id > 0 AND name NOT LIKE 'PK%';
```

**Expected Results:**
- 5 schemas: Master, Shared, Transaction, Report, Auth ✓
- 43 tables total ✓
- 2 tenants (DEFAULT, DEMO) ✓
- 8 languages ✓
- 3 menus with 13 menu items ✓
- 6 roles ✓
- Multiple indexes created ✓

---

## Connection String Examples

### For C# Application (appsettings.json)

**Integrated Security:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
  }
}
```

**SQL Server Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Initial Catalog=SmartWorkz_v4;User Id=sa;Password=YourPassword;"
  }
}
```

**Named Instance (SQLEXPRESS):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Initial Catalog=SmartWorkz_v4;Integrated Security=true;"
  }
}
```

---

## Database Maintenance

### Backup Database

```sql
BACKUP DATABASE SmartWorkz_v4
TO DISK = 'C:\Backups\SmartWorkz_v4.bak'
WITH INIT;
```

### Restore Database

```sql
RESTORE DATABASE SmartWorkz_v4
FROM DISK = 'C:\Backups\SmartWorkz_v4.bak'
WITH REPLACE;
```

### Update Statistics

```sql
EXEC sp_updatestats;
```

### Rebuild Indexes

```sql
ALTER INDEX ALL ON Master.Products REBUILD;
ALTER INDEX ALL ON Shared.SeoMeta REBUILD;
```

---

## Next Steps

After successful database deployment:

1. **Step 2:** Create 43 domain entities (C# classes)
2. **Step 3:** Configure 4 EF Core DbContexts
3. **Step 4:** Implement services (Menu, SEO, Tag, Repository)
4. **Step 5:** Build REST API endpoints
5. **Step 6:** Create DTOs and AutoMapper configurations
6. **Step 7:** Configure dependency injection

See `implementation_phase1_v4.md` for detailed Phase 1 plan.

---

## Support

For deployment issues:
1. Check SQL Server is running: `services.msc` → SQL Server (SQL Server 2016+)
2. Verify connection string in appsettings.json
3. Check SQL Server logs: C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Log\
4. Run deployment script with `-Verbose` flag for detailed output

---

**Database deployed successfully! Ready for Phase 1 implementation.** ✓
