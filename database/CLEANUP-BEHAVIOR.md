# QUICK-DEPLOY.ps1 - Cleanup Behavior

**Updated:** Database kept, only tables & schemas cleaned

---

## 🧹 What Gets Cleaned (Default Behavior)

### Dropped (Cleaned)
- ✅ **All tables** in schemas: Master, Shared, Transaction, Report, Auth
- ✅ **All schemas**: Master, Shared, Transaction, Report, Auth (then recreated)
- ✅ **All data** in those tables (replaced with seed data)

### Preserved (NOT Dropped)
- ✅ **Database itself** - KEPT, not dropped
- ✅ **Other databases** - untouched
- ✅ **System tables** - untouched
- ✅ **dbo schema** - untouched

---

## 📋 Cleanup Flow

```
1. Test connection
   ✅ Connected to server

2. Check database
   ├─ If NOT exists → Create it
   └─ If exists → Continue (DO NOT drop)

3. Clean tables (if cleanup enabled)
   ✅ Drop all tables in (Master, Shared, Transaction, Report, Auth)

4. Clean schemas (if cleanup enabled)
   ✅ Drop all schemas (Master, Shared, Transaction, Report, Auth)

5. Deploy new schemas & tables
   ✅ 001_InitializeDatabase.sql (create schemas)
   ✅ 002_CreateTables_Master.sql (18 tables)
   ✅ 003_CreateTables_Shared.sql (7 tables)
   ✅ 004_CreateTables_Transaction.sql (1 table)
   ✅ 005_CreateTables_Report.sql (4 tables)
   ✅ 006_CreateTables_Auth.sql (13 tables)
   ✅ 007_SeedData.sql (seed data)
   ✅ 008_CreateIndexes.sql (indexes)

6. Verify
   ✅ 43 tables created
   ✅ All indexes created
   ✅ Seed data loaded
```

---

## 🎯 Usage Examples

### Example 1: Default (Clean Tables & Schemas)
```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

**What happens:**
- ✅ Database is KEPT
- ✅ Existing tables are DROPPED
- ✅ Existing schemas are DROPPED
- ✅ Fresh schemas & tables created
- ✅ Seed data loaded

---

### Example 2: Skip Cleanup (Keep Everything)
```powershell
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1" `
  -SkipCleanup
```

**What happens:**
- ✅ Database is KEPT
- ✅ Existing tables are KEPT
- ✅ Existing schemas are KEPT
- ✅ Only NEW tables/schemas are created
- ✅ Existing data is preserved

---

## 📊 Scenarios

### Scenario 1: Fresh Installation
```powershell
# First time setup
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

**Before:**
- No database exists

**After:**
- ✅ Database created: Boilerplate
- ✅ 43 tables created
- ✅ Seed data loaded

---

### Scenario 2: Reset Schema (Keep Database)
```powershell
# Reset to fresh schema without losing database
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1"
```

**Before:**
- Database: Boilerplate (with old tables)

**After:**
- ✅ Database: Boilerplate (same name)
- ✅ Old tables: DROPPED
- ✅ New tables: CREATED
- ✅ Seed data: LOADED

---

### Scenario 3: Update Schema (Keep Data)
```powershell
# Add new schema without dropping existing
.\QUICK-DEPLOY.ps1 `
  -ServerName "115.124.106.158" `
  -DatabaseName "Boilerplate" `
  -Username "zenthil" `
  -Password "PinkPanther#1" `
  -SkipCleanup
```

**Before:**
- Database: Boilerplate (with existing data)

**After:**
- ✅ Database: Boilerplate (unchanged)
- ✅ Existing tables: PRESERVED
- ✅ Existing data: PRESERVED
- ✅ New tables: ADDED (if not exists)

---

## ⚠️ Important Notes

### What's Safe
✅ Database name is preserved
✅ Can re-run multiple times
✅ Other databases untouched
✅ dbo schema untouched
✅ System objects untouched

### What's Deleted (Default)
⚠️ All tables in Master, Shared, Transaction, Report, Auth schemas
⚠️ All data in those tables
⚠️ All indexes in those schemas

### To Preserve Data
✅ Use `-SkipCleanup` flag
✅ Or keep backup of database
✅ Or don't run default cleanup

---

## 🔍 SQL Commands Used for Cleanup

### Drop All Tables
```sql
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += 'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')
  AND TABLE_TYPE = 'BASE TABLE';

IF LEN(@sql) > 0
    EXEC sp_executesql @sql;
```

### Drop All Schemas
```sql
DECLARE @schema NVARCHAR(128);
DECLARE SchemaCursor CURSOR FOR
    SELECT name FROM sys.schemas
    WHERE name IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth');

OPEN SchemaCursor;
FETCH NEXT FROM SchemaCursor INTO @schema;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_executesql N'DROP SCHEMA [' + @schema + ']';
    FETCH NEXT FROM SchemaCursor INTO @schema;
END;

CLOSE SchemaCursor;
DEALLOCATE SchemaCursor;
```

---

## 📞 Quick Reference

| Action | Command | Result |
|--------|---------|--------|
| **Clean Deploy** | Default | Drop tables & schemas, keep DB |
| **Fresh Install** | Default | Create everything new |
| **Safe Update** | -SkipCleanup | Keep everything, add new |
| **Reset Data** | Default | Drop all, reload seed |

---

## ✅ Summary

**Default Behavior (RECOMMENDED):**
- Database: ✅ KEPT (reused)
- Schemas: ⚠️ DROPPED & RECREATED
- Tables: ⚠️ DROPPED & RECREATED
- Data: ⚠️ DELETED & RESEEDED

**With -SkipCleanup:**
- Database: ✅ KEPT
- Schemas: ✅ KEPT
- Tables: ✅ KEPT
- Data: ✅ KEPT

---

**Status:** ✅ Ready to Deploy

Choose cleanup mode based on your needs!
