# SmartWorkz StarterKit MVC - Deployment Guide

## Quick Deployment with PowerShell

The `QUICK-DEPLOY.ps1` script automates the entire deployment process:
- Database creation
- Running all migrations in correct order
- Seeding test data and users
- Building the application

---

## Usage

### 1. Deploy to Remote Server (SQL Authentication)
```powershell
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

### 2. Deploy to Local Server (Windows Integrated Auth)
```powershell
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
```

### 3. Deploy to Named Instance
```powershell
.\QUICK-DEPLOY.ps1 -ServerName "COMPUTER\SQLEXPRESS" -IntegratedAuth
```

### 4. Deploy and Start Application
```powershell
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -Username "zenthil" -Password "PinkPanther#1" -StartApp
```

### 5. Deploy Without Building (if already built)
```powershell
.\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth -SkipBuild
```

---

## Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `-ServerName` | Yes | SQL Server name or IP address | `"115.124.106.158"`, `".\SQLEXPRESS"`, `"localhost"` |
| `-DatabaseName` | No | Database name (default: "Boilerplate") | `"MyDatabase"` |
| `-Username` | No* | SQL Server username (*required for SQL auth) | `"zenthil"` |
| `-Password` | No* | SQL Server password (*required for SQL auth) | `"PinkPanther#1"` |
| `-IntegratedAuth` | No | Use Windows Integrated Authentication | (switch flag) |
| `-SkipBuild` | No | Skip dotnet build step | (switch flag) |
| `-StartApp` | No | Start application after deployment | (switch flag) |

---

## What the Script Does

### Step 1: Validation
- ✅ Validates parameters
- ✅ Tests SQL Server connection
- ✅ Verifies migration scripts exist

### Step 2: Database Setup
- ✅ Drops existing database (if exists)
- ✅ Creates fresh database

### Step 3: Run Migrations (in order)
1. `001_CreateTables_Master.sql` - Master data tables
2. `002_CreateTables_Shared.sql` - Shared data tables
3. `003_CreateTables_Transaction.sql` - Transaction tables
4. `004_CreateTables_Report.sql` - Reporting tables
5. `005_CreateTables_Audit.sql` - Audit tables
6. `006_CreateTables_Auth.sql` - Authentication tables

### Step 4: Seed Data
1. `007_SeedData.sql` - Initial seed data (roles, permissions, menus, etc.)
2. `008_SeedTestUsers.sql` - Test user accounts

### Step 5: Build Application
- ✅ Restores NuGet packages
- ✅ Builds solution with `dotnet build`

---

## Test User Credentials

After deployment, test with these credentials:

| Role     | Email                    | Password         |
|----------|--------------------------|------------------|
| Admin    | admin@smartworkz.test    | TestPassword123! |
| Manager  | manager@smartworkz.test  | TestPassword123! |
| Staff    | staff@smartworkz.test    | TestPassword123! |
| Customer | customer@smartworkz.test | TestPassword123! |

---

## Troubleshooting

### Error: "Cannot connect to SQL Server"
**Causes:**
- SQL Server is not running
- Server name/IP is incorrect
- Username/password is wrong
- Network connectivity issue

**Solutions:**
```powershell
# Verify SQL Server is running
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Test connection separately
sqlcmd -S "115.124.106.158" -U "zenthil" -P "PinkPanther#1"
```

### Error: "Login failed for user"
**Cause:** Incorrect credentials

**Solution:**
```powershell
# Test credentials with sqlcmd
sqlcmd -S "115.124.106.158" -U "zenthil" -P "PinkPanther#1" -Q "SELECT 1"
```

### Error: "Database already exists"
The script automatically drops and recreates the database. If this fails:

```powershell
# Manual cleanup in SQL Server Management Studio
ALTER DATABASE Boilerplate SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE Boilerplate;
```

### Error: "Script not found"
The script path is incorrect. Ensure:
- You're running from the project root directory
- All SQL files exist in the `database/` folder
- File names match exactly (case-sensitive)

### PowerShell Execution Policy Error
```powershell
# Allow script execution for current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or run with explicit bypass
powershell -ExecutionPolicy Bypass -File .\QUICK-DEPLOY.ps1 -ServerName "..." ...
```

---

## After Deployment

### 1. Update Connection String (if needed)
Edit `appsettings.json` if server details differ from script parameters:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=115.124.106.158;Database=Boilerplate;User Id=zenthil;Password=PinkPanther#1;Encrypt=true;TrustServerCertificate=true;"
  }
}
```

### 2. Start Application
```bash
dotnet run
```

### 3. Test via Swagger UI
Navigate to: `http://localhost:5000/swagger`

### 4. Login with Admin Account
1. Click **POST /api/auth/login**
2. Enter credentials:
```json
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}
```

---

## Common Scenarios

### Scenario 1: Deploy to Production Server
```powershell
$params = @{
    ServerName = "prod-sql-server.example.com"
    DatabaseName = "SmartWorkz_Prod"
    Username = "sa"
    Password = "YourSecurePassword123!"
}
.\QUICK-DEPLOY.ps1 @params
```

### Scenario 2: Deploy to Local Development
```powershell
.\QUICK-DEPLOY.ps1 -ServerName "localhost" -IntegratedAuth -StartApp
```

### Scenario 3: Deploy to Multiple Environments
```powershell
# Development
.\QUICK-DEPLOY.ps1 -ServerName "dev-sql" -IntegratedAuth

# Staging
.\QUICK-DEPLOY.ps1 -ServerName "stage-sql.internal" -Username "staging" -Password "StagePass123!"

# Production
.\QUICK-DEPLOY.ps1 -ServerName "prod-sql.internal" -Username "produser" -Password "ProdPass123!"
```

---

## Script Output Example

```
╔════════════════════════════════════════════════════╗
║ SmartWorkz StarterKit MVC - Deployment             ║
╚════════════════════════════════════════════════════╝

ℹ️  Validating parameters...
✅ Parameters validated
ℹ️  Testing SQL Server connection...
✅ SQL Server connection successful

╔════════════════════════════════════════════════════╗
║ Step 1/4: Creating Database                        ║
╚════════════════════════════════════════════════════╝

ℹ️  Dropping existing database (if exists)...
✅ Database created successfully

╔════════════════════════════════════════════════════╗
║ Step 2/4: Running Migrations                       ║
╚════════════════════════════════════════════════════╝

ℹ️  (1/6) Running 001_CreateTables_Master.sql...
✅ Completed: 001_CreateTables_Master.sql
... [continues for all migrations] ...

╔════════════════════════════════════════════════════╗
║ Step 3/4: Seeding Data                             ║
╚════════════════════════════════════════════════════╝

ℹ️  (1/2) Running 007_SeedData.sql...
✅ Completed: 007_SeedData.sql
ℹ️  (2/2) Running 008_SeedTestUsers.sql...
✅ Completed: 008_SeedTestUsers.sql

╔════════════════════════════════════════════════════╗
║ Step 4/4: Building Application                     ║
╚════════════════════════════════════════════════════╝

ℹ️  Restoring NuGet packages...
✅ Packages restored
ℹ️  Building application...
✅ Application built successfully

╔════════════════════════════════════════════════════╗
║ Deployment Summary                                 ║
╚════════════════════════════════════════════════════╝

✅ Database: Boilerplate
✅ Server: 115.124.106.158
✅ Auth Mode: SQL Authentication
✅ Status: ✅ READY FOR TESTING
```

---

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review deployment logs
3. Verify SQL Server connectivity
4. Ensure all script files exist in `database/` folder

---

**Last Updated**: 2026-03-31
**Version**: 1.0
