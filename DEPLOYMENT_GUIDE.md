# SmartWorkz StarterKit MVC v4 - Deployment Guide

**Version:** 4.0.0  
**Date:** 2026-03-31  
**Status:** Ready for Production Deployment

---

## Quick Start

### Fresh Database Deployment
```powershell
.\database\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -DatabaseName "Boilerplate" -IntegratedAuth
```

### Remote Server with SQL Authentication
```powershell
.\database\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "admin" -Password "YourPassword"
```

---

## QUICK-DEPLOY.ps1 Script

**Location:** `database/QUICK-DEPLOY.ps1`  
**Purpose:** Automated end-to-end database deployment

**What it does:**
- Validates parameters (ServerName, credentials)
- Executes all 9 migration scripts in correct sequence:
  1. 000_DeleteAllSchemas.sql - Drops all existing tables, stored procedures, and indexes (preserves database)
  2. 001_InitializeDatabase.sql - Recreate all schemas in clean database
  3. 002_CreateTables_Master.sql - Create 15 core boilerplate tables
  4. 003_CreateTables_Shared.sql - Create 7 polymorphic tables
  5. 004_CreateTables_Transaction.sql - Initialize Transaction schema
  6. 005_CreateTables_Report.sql - Initialize Report schema
  7. 006_CreateTables_Auth.sql - Create 13 auth/RBAC tables
  8. 007_SeedData.sql - Populate reference data and lookups
  9. 008_SeedTestUsers.sql - Create 4 test users with roles and permissions
- Automatically builds dotnet solution
- Provides color-coded progress output with step numbers

**Usage Examples:**

```powershell
# With Windows Integrated Authentication (default)
.\database\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth

# With SQL Server Authentication
.\database\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "admin" -Password "P@ssw0rd"

# Custom database name
.\database\QUICK-DEPLOY.ps1 -ServerName "localhost" -DatabaseName "MyDatabase" -IntegratedAuth
```

**Execution Time:** ~2-3 minutes  
**Result:** Fully seeded database with all tables, relationships, and test data

---

## Database Schema Overview

### Master Schema (15 Tables - Core Boilerplate)
- **Multi-tenancy:** Tenants, TenantUsers
- **Configuration:** Configuration, FeatureFlags
- **Lookups:** Countries, Currencies, Languages, TimeZones
- **Navigation:** Menus, MenuItems (HierarchyID)
- **Content:** Categories (HierarchyID), GeoHierarchy, GeolocationPages, CustomPages, BlogPosts

### Shared Schema (7 Tables - Cross-Tenant Polymorphic)
- SeoMeta - SEO metadata with EntityType + EntityId
- Tags - Polymorphic tagging system
- Translations - Multi-language content support
- Notifications - System notifications
- AuditLogs - Audit trail with entity tracking
- FileStorage - Polymorphic file attachments
- EmailQueue - Email delivery queue

### Auth Schema (13 Tables - Authentication & Authorization)
- Users, Roles, Permissions (core RBAC)
- UserRoles, RolePermissions, UserPermissions (flexible assignment)
- RefreshTokens, PasswordResetTokens, EmailVerificationTokens (token management)

### Transaction & Report Schemas
Placeholder schemas for future domain-specific tables

---

## Seeded Test Data

### Test Users
Password for all users: `TestPassword123!`

| Email | Role | Permissions |
|-------|------|-------------|
| admin@smartworkz.test | Admin | All CRUD operations + User Management |
| manager@smartworkz.test | Manager | Read, Update, View Reports |
| staff@smartworkz.test | Staff | Read only |
| customer@smartworkz.test | Customer | Customer role-based |

### Reference Data
- **Tenants:** 2 (DEFAULT, DEMO)
- **Languages:** 8 (English, Spanish, French, German, Italian, Portuguese, Japanese, Chinese)
- **Countries:** 12 (US, GB, CA, AU, ES, FR, DE, IT, JP, CN, IN, BR)
- **Currencies:** 9 (USD, EUR, GBP, JPY, CNY, INR, BRL, CAD, AUD)
- **TimeZones:** 9 (UTC, EST, CST, MST, PST, GMT, CET, JST, AEST)
- **Roles:** 6 (Super Admin, Admin, Manager, Staff, Customer, Guest)
- **Permissions:** 12 (CRUD for Product, Order; View Report; Manage Users, Menus)
- **Menus:** 3 with 13 hierarchical MenuItems
- **Categories:** 13 (4 root + 9 subcategories with HierarchyID)
- **Configuration:** 8 key-value pairs
- **Feature Flags:** 6 toggles

---

## Manual Deployment (Without Script)

If you prefer to run scripts manually, execute in this exact order on Boilerplate database:

```powershell
# Step 1: Delete all existing objects (tables, SPs, indexes) - preserves database
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/000_DeleteAllSchemas.sql"

# Step 2: Create all schemas
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/001_InitializeDatabase.sql"

# Step 3: Create all tables (in order)
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/002_CreateTables_Master.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/003_CreateTables_Shared.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/004_CreateTables_Transaction.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/005_CreateTables_Report.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/006_CreateTables_Auth.sql"

# Step 4: Seed reference data and test users
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/007_SeedData.sql"
sqlcmd -S ".\SQLEXPRESS" -d "Boilerplate" -i "database/v1/008_SeedTestUsers.sql"
```

**Important:** All 9 scripts run against Boilerplate database. Script 000 cleans existing objects but preserves the database.

---

## Validation & Verification

### Verify Database Creation
```sql
USE Boilerplate;
SELECT COUNT(*) AS TableCount 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA IN ('Master', 'Shared', 'Auth', 'Transaction', 'Report');
-- Expected: 43 tables
```

### Verify Test Users
```sql
SELECT Email, DisplayName, TenantId 
FROM Auth.Users 
WHERE TenantId = 'DEFAULT';
-- Expected: 4 test users
```

### Verify Seed Data
```sql
SELECT 'Master.Tenants' AS [Table], COUNT(*) AS Count FROM Master.Tenants
UNION ALL SELECT 'Auth.Users', COUNT(*) FROM Auth.Users
UNION ALL SELECT 'Master.Categories', COUNT(*) FROM Master.Categories
UNION ALL SELECT 'Auth.Roles', COUNT(*) FROM Auth.Roles;
```

---

## Pre-Deployment Checklist

- [ ] SQL Server instance is running and accessible
- [ ] Database credentials have proper permissions (CREATE DATABASE, CREATE TABLE)
- [ ] Network access to database server is confirmed
- [ ] Database name doesn't conflict with existing databases
- [ ] Sufficient disk space available (~100 MB minimum)
- [ ] .NET 9.0 SDK is installed
- [ ] PowerShell execution policy allows script execution

---

## Troubleshooting

### Script Execution Blocked
**Error:** "Cannot be loaded because running scripts is disabled on this system"  
**Solution:** Run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Connection Timeout
**Error:** "Timeout expired" or "Cannot connect to server"  
**Solution:** 
- Verify SQL Server is running: `Get-Service -Name "MSSQL$SQLEXPRESS"`
- Check network connectivity to database server
- Verify correct server name and credentials

### Foreign Key Constraint Errors
**Error:** "The FOREIGN KEY constraint failed"  
**Cause:** Seed data execution order issue  
**Solution:** Ensure scripts run in correct order (001-008). QUICK-DEPLOY.ps1 handles this automatically.

### Database Already Exists
**Cause:** Previous deployment left database intact  
**Solution:** Drop and recreate manually via SSMS or SQL script, then run QUICK-DEPLOY.ps1

---

## API Testing with Test Users

After deployment, test the API using test user credentials:

```bash
# 1. Login to get JWT token
POST /api/auth/login
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!"
}

# 2. Use token in API requests
Authorization: Bearer {jwt_token}

# 3. Test endpoints
GET /api/menus
GET /api/categories
GET /api/users
```

---

## Database Maintenance

To reset database for testing:

1. **Drop entire database and recreate:**
   ```sql
   DROP DATABASE [Boilerplate];
   ```
   Then run QUICK-DEPLOY.ps1

2. **Via SSMS:** Right-click database → Delete → Rerun QUICK-DEPLOY.ps1

---

## Key Features & Design Patterns

✅ **Multi-Tenancy:** TenantId isolation on 35 tables  
✅ **Polymorphic Relationships:** EntityType + EntityId for flexible linking  
✅ **Hierarchical Structures:** HierarchyID for unlimited nesting  
✅ **RBAC:** Complete role-based access control  
✅ **Soft Deletes:** IsDeleted flag (no cascade deletes)  
✅ **Performance Indexes:** Optimized for common queries  
✅ **Audit Trail:** Complete change tracking in AuditLogs  

---

## Next Steps After Deployment

1. **Test Database Connection**
   - Verify all tables created: 43 tables across 5 schemas
   - Confirm test data seeded
   - Test user login with credentials

2. **Configure Application**
   - Update appsettings.json connection string
   - Configure DbContext (ReferenceDbContext, AuthDbContext)
   - Run EF Core migrations if needed

3. **Test API Endpoints**
   - Start application: `dotnet run --project src/SmartWorkz.StarterKitMVC.Web`
   - Open Swagger UI: `https://localhost:5001/swagger`
   - Test with Bearer token authentication

4. **Run Integration Tests**
   - Execute test suite: `dotnet test`
   - Verify polymorphic queries
   - Test HierarchyID hierarchies

---

**Author:** SmartWorkz Development Team  
**Last Updated:** 2026-03-31  
**Status:** Production Ready ✅
