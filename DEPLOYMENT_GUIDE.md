# SmartWorkz StarterKit MVC v4 - Deployment Guide

**Version:** 4.0.0  
**Date:** 2026-03-31  
**Status:** Ready for Production Deployment

---

## Quick Start

### Fresh Database Deployment
```powershell
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -DatabaseName "Boilerplate" -IntegratedAuth
```

### Remote Server with SQL Authentication
```powershell
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "YourPassword"
```

---

## Database Deployment Scripts

### 1. **QUICK-DEPLOY.ps1** (Main Deployment)
**Purpose:** Automated end-to-end database deployment  
**What it does:**
- Validates parameters (ServerName, credentials)
- Executes all 8 migration scripts in sequence:
  - 001_InitializeDatabase.sql
  - 002_CreateTables_Master.sql
  - 003_CreateTables_Shared.sql
  - 004_CreateTables_Transaction.sql
  - 005_CreateTables_Report.sql
  - 006_CreateTables_Auth.sql
  - 007_SeedData.sql
  - 008_SeedTestUsers.sql
- Automatically builds dotnet solution
- Provides color-coded progress output

**Usage:**
```powershell
# With Windows Integrated Authentication
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

# With SQL Server Authentication
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -Username "admin" -Password "P@ssw0rd"

# Custom database name
.\QUICK-DEPLOY.ps1 -ServerName "localhost" -DatabaseName "MyDatabase" -IntegratedAuth
```

**Time:** ~2-3 minutes  
**Output:** Fully seeded database ready for testing

---

### 2. **QUICK-CLEANUP.ps1** (Data Cleanup Only)
**Purpose:** Remove all data while preserving schema structure  
**What it does:**
- Double-confirmation prompts for safety
- Disables all foreign key constraints
- Deletes all data from 43 tables in dependency order
- Resets all IDENTITY seeds to 0
- Re-enables all constraints
- Schema structure remains intact for redeployment

**Usage:**
```powershell
# Clean database (preserves schema)
.\QUICK-CLEANUP.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

# Confirm twice when prompted
# First: "Are you sure you want to DELETE ALL DATA from Boilerplate?"
# Second: "Type 'yes' again to confirm (this cannot be undone)"
```

**When to use:**
- Between test cycles
- Before reseeding with different test data
- To reset database state without schema recreation

**Time:** ~1 minute  
**Result:** Empty database, schema intact

---

### 3. **QUICK-DROP.ps1** (Complete Schema Deletion)
**Purpose:** Completely remove all tables and schemas  
**What it does:**
- Triple-confirmation prompts requiring "DROP" keyword
- Disables all foreign key constraints
- Drops all 43 tables in reverse dependency order
- Drops all 5 schemas (Master, Shared, Auth, Transaction, Report)
- Dynamically handles Transaction and Report schemas

**Usage:**
```powershell
# Drop all tables and schemas
.\QUICK-DROP.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

# Confirmation prompts:
# 1. "Are you absolutely sure you want to DROP ALL TABLES from Boilerplate?" → Type "DROP"
# 2. "Type 'DROP' to confirm this action" → Type "DROP" again
```

**When to use:**
- Rebuilding entire database from scratch
- Starting fresh after major schema changes
- Cleanup before archival

**Time:** ~1 minute  
**Result:** Database structure completely removed

**Next Steps After Drop:**
```powershell
# Rebuild database
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
```

---

## Database Schema Overview

### Master Schema (15 Tables)
Core boilerplate infrastructure:
- **Multi-tenancy:** Tenants, TenantUsers
- **Settings:** Configuration, FeatureFlags
- **Lookups:** Countries, Currencies, Languages, TimeZones
- **Navigation:** Menus, MenuItems (with HierarchyID)
- **Content:** Categories (with HierarchyID), GeoHierarchy, GeolocationPages, CustomPages, BlogPosts

### Shared Schema (7 Tables)
Cross-tenant polymorphic tables:
- **SeoMeta** - SEO metadata with EntityType + EntityId pattern
- **Tags** - Polymorphic tagging system
- **Translations** - Multi-language content support
- **Notifications** - System notifications
- **AuditLogs** - Audit trail for all changes
- **FileStorage** - Polymorphic file attachments
- **EmailQueue** - Email delivery queue

### Auth Schema (13 Tables)
Authentication & authorization:
- **Users, Roles, Permissions** - Core RBAC
- **UserRoles, RolePermissions, UserPermissions** - Flexible permission assignment
- **RefreshTokens, PasswordResetTokens, EmailVerificationTokens** - Token management

### Transaction & Report Schemas
Placeholders for:
- Transaction: Orders, Payments, Invoices, etc.
- Report: Aggregated reporting tables

---

## Seeded Test Data

### Test Users (for API testing)
All users use password: `TestPassword123!`

| Email | Role | Permissions |
|-------|------|-------------|
| admin@smartworkz.test | Admin | All CRUD operations |
| manager@smartworkz.test | Manager | Read, Update, View Reports |
| staff@smartworkz.test | Staff | Read only |
| customer@smartworkz.test | Customer | Customer role-based |

**API Testing with Bearer Token:**
```bash
# 1. Login
POST /api/auth/login
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!"
}

# 2. Use returned JWT token
Authorization: Bearer {token}
```

### Reference Data

**Tenants:** 2 (DEFAULT, DEMO)  
**Languages:** 8 (English, Spanish, French, German, Italian, Portuguese, Japanese, Chinese)  
**Countries:** 12 (US, GB, CA, AU, ES, FR, DE, IT, JP, CN, IN, BR)  
**Currencies:** 9 (USD, EUR, GBP, JPY, CNY, INR, BRL, CAD, AUD)  
**TimeZones:** 9 (UTC, EST, CST, MST, PST, GMT, CET, JST, AEST)  
**Roles:** 6 (Super Admin, Admin, Manager, Staff, Customer, Guest)  
**Permissions:** 12 (CRUD for Product, Order; View Report; Manage Users, Menus)  
**Menus:** 3 with 13 hierarchical MenuItems  
**Categories:** 13 (4 root + 9 subcategories with HierarchyID)  
**Configuration:** 8 key-value pairs  
**Feature Flags:** 6 toggles  

---

## Manual Script Execution (If Not Using Quick Scripts)

### Step 1: Create Database
```sql
sqlcmd -S ".\SQLEXPRESS" -i "database/001_InitializeDatabase.sql"
```

### Step 2: Create Schemas & Tables (In Order)
```sql
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/002_CreateTables_Master.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/003_CreateTables_Shared.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/004_CreateTables_Transaction.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/005_CreateTables_Report.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/006_CreateTables_Auth.sql"
```

### Step 3: Seed Data
```sql
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/007_SeedData.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/008_SeedTestUsers.sql"
```

---

## Pre-Deployment Checklist

- [ ] SQL Server instance is running
- [ ] Credentials have proper permissions (CREATE DATABASE, CREATE TABLE)
- [ ] Network access to database server confirmed
- [ ] Database name doesn't conflict with existing databases
- [ ] Sufficient disk space available (~100 MB minimum)
- [ ] .NET 9.0 SDK installed
- [ ] PowerShell execution policy allows script execution

---

## Validation Commands

### Verify Database Creation
```sql
USE Boilerplate;
SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA IN ('Master', 'Shared', 'Auth', 'Transaction', 'Report');
```

### Verify Test Users
```sql
SELECT Email, DisplayName, TenantId FROM Auth.Users WHERE TenantId = 'DEFAULT';
```

### Verify Seed Data
```sql
SELECT 'Master.Tenants' AS [Table], COUNT(*) AS Count FROM Master.Tenants
UNION ALL SELECT 'Auth.Users', COUNT(*) FROM Auth.Users
UNION ALL SELECT 'Master.Categories', COUNT(*) FROM Master.Categories
UNION ALL SELECT 'Auth.Roles', COUNT(*) FROM Auth.Roles;
```

---

## Troubleshooting

### Script Execution Fails with "Access Denied"
**Cause:** Insufficient SQL Server permissions  
**Solution:**
```powershell
# Run as Administrator
Start-Process PowerShell -ArgumentList "-File QUICK-DEPLOY.ps1 -ServerName '.\SQLEXPRESS' -IntegratedAuth" -Verb RunAs
```

### Connection Timeout
**Cause:** SQL Server not running or network unreachable  
**Solution:**
```powershell
# Verify SQL Server is running
Get-Service -Name "MSSQL$SQLEXPRESS" | Start-Service
```

### Foreign Key Constraint Errors
**Cause:** Seed data execution order issue  
**Solution:** Ensure scripts run in correct order (001-008). QUICK-DEPLOY.ps1 handles this automatically.

### Database Already Exists
**Cause:** Previous deployment not cleaned up  
**Solution:**
```powershell
# Drop and recreate
.\QUICK-DROP.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
```

---

## Performance Notes

- **HierarchyID Queries:** MenuItems, Categories, GeoHierarchy support unlimited nesting with O(log n) performance
- **Multi-tenancy:** All tables filtered by TenantId for row-level security
- **Polymorphic Indexes:** (EntityType, EntityId) indexes on SeoMeta, Tags, Translations, AuditLogs, FileStorage
- **Soft Deletes:** IsDeleted flag on all tables (no cascade deletes)

---

## Next Steps After Deployment

1. **Test API Endpoints**
   - Start application: `dotnet run --project src/SmartWorkz.StarterKitMVC.Web`
   - Open Swagger UI: `https://localhost:5001/swagger`
   - Login with test credentials

2. **Configure DbContext**
   - Implement ReferenceDbContext (Master + Shared = 22 tables)
   - Implement AuthDbContext (Auth = 13 tables)
   - Add global query filters for multi-tenancy

3. **Run Integration Tests**
   - Execute test suite: `dotnet test`
   - Verify polymorphic relationships
   - Test HierarchyID queries

4. **Production Deployment**
   - Update connection string to production server
   - Run database migrations on production
   - Verify backup strategy
   - Monitor performance metrics

---

## Support & Documentation

- **Database Design:** See [database_schema_design_v4.md](../memory/database_schema_design_v4.md)
- **Validation Report:** See [DATABASE_VALIDATION_REPORT.md](./DATABASE_VALIDATION_REPORT.md)
- **Schema Files:** `/database/` directory
- **Deployment Scripts:** Root directory (`QUICK-*.ps1`)

---

**Author:** SmartWorkz Development Team  
**Last Updated:** 2026-03-31  
**Status:** Production Ready ✅
