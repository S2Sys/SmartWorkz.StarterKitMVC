# Deploy Test Users - Step by Step

## Prerequisites
- SQL Server instance running
- Boilerplate database created
- 007_SeedData.sql already executed (roles, permissions exist)

## Step 1: Run Test User Seed Script

### Option A: SQL Server Management Studio
1. Open **SQL Server Management Studio**
2. Connect to your SQL Server instance
3. Open **File > Open > File** and select `database/008_SeedTestUsers.sql`
4. Click **Execute** (F5)
5. Check output for success message

### Option B: sqlcmd (Command Line)
```bash
sqlcmd -S your-server-instance -d Boilerplate -i database/008_SeedTestUsers.sql
```

**Example with default local instance:**
```bash
sqlcmd -S localhost -d Boilerplate -i database/008_SeedTestUsers.sql
```

**Example with named instance:**
```bash
sqlcmd -S .\SQLEXPRESS -d Boilerplate -i database/008_SeedTestUsers.sql
```

### Option C: PowerShell
```powershell
Invoke-Sqlcmd -ServerInstance ".\SQLEXPRESS" -Database "Boilerplate" -InputFile "database\008_SeedTestUsers.sql"
```

## Step 2: Verify Test Users Created

Run this query in SQL Server Management Studio:

```sql
USE Boilerplate;

SELECT UserId, Email, Username, DisplayName, IsActive, CreatedAt
FROM Auth.Users
WHERE Email LIKE '%.test'
ORDER BY CreatedAt DESC;
```

**Expected Output:**
```
UserId                               Email                   Username   DisplayName     IsActive  CreatedAt
==================================== ======================= ========== =============== ========= ====================
[GUID]                               admin@smartworkz.test   admin      Admin User      1         [timestamp]
[GUID]                               manager@smartworkz.test manager    Manager User    1         [timestamp]
[GUID]                               staff@smartworkz.test   staff      Staff User      1         [timestamp]
[GUID]                               customer@smartworkz.test customer   Customer User   1         [timestamp]
```

## Step 3: Verify Roles Assigned

```sql
USE Boilerplate;

SELECT 
    u.Email,
    r.Name as Role,
    ur.AssignedAt
FROM Auth.Users u
INNER JOIN Auth.UserRoles ur ON u.UserId = ur.UserId
INNER JOIN Auth.Roles r ON ur.RoleId = r.RoleId
WHERE u.Email LIKE '%.test'
ORDER BY u.Email, r.Name;
```

**Expected Output:**
```
Email                       Role      AssignedAt
========================== ========= ====================
admin@smartworkz.test       Admin     [timestamp]
customer@smartworkz.test    Customer  [timestamp]
manager@smartworkz.test     Manager   [timestamp]
staff@smartworkz.test       Staff     [timestamp]
```

## Step 4: Verify Permissions Assigned

```sql
USE Boilerplate;

SELECT 
    u.Email,
    p.Name as Permission,
    up.GrantedAt
FROM Auth.Users u
INNER JOIN Auth.UserPermissions up ON u.UserId = up.UserId
INNER JOIN Auth.Permissions p ON up.PermissionId = p.PermissionId
WHERE u.Email LIKE '%.test'
ORDER BY u.Email, p.Name;
```

**Expected Output:**
```
Email                       Permission              GrantedAt
========================== ======================= ====================
admin@smartworkz.test       Create Order            [timestamp]
admin@smartworkz.test       Create Product          [timestamp]
admin@smartworkz.test       Delete Order            [timestamp]
admin@smartworkz.test       Delete Product          [timestamp]
admin@smartworkz.test       Manage Users            [timestamp]
admin@smartworkz.test       Read Order              [timestamp]
admin@smartworkz.test       Read Product            [timestamp]
admin@smartworkz.test       Update Order            [timestamp]
admin@smartworkz.test       Update Product          [timestamp]
admin@smartworkz.test       View Report             [timestamp]
manager@smartworkz.test     Read Order              [timestamp]
manager@smartworkz.test     Read Product            [timestamp]
manager@smartworkz.test     Update Order           [timestamp]
manager@smartworkz.test     Update Product         [timestamp]
manager@smartworkz.test     View Report            [timestamp]
staff@smartworkz.test       Read Order              [timestamp]
staff@smartworkz.test       Read Product           [timestamp]
```

## Step 5: Test Login

### Using Swagger UI
1. Start the application: `dotnet run`
2. Open: `http://localhost:5000/swagger`
3. Click **POST /api/auth/login**
4. Click **Try it out**
5. Enter JSON:
```json
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}
```
6. Click **Execute**
7. Should receive 200 OK with accessToken

### Using cURL
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
  }'
```

## Troubleshooting

### Error: "Operand type clash"
**Cause**: PermissionId type mismatch
**Solution**: Ensure you have the FIXED version of 008_SeedTestUsers.sql (after commit 6414f69)

### Error: "Invalid object name 'Auth.Users'"
**Cause**: Schema doesn't exist yet
**Solution**: Run 006_CreateTables_Auth.sql first

### Error: "Cannot insert duplicate key"
**Cause**: Test users already exist
**Solution**: Run cleanup script below

### Error: "Cannot find login for user"
**Cause**: Invalid TenantId
**Solution**: Verify DEFAULT tenant exists in Master.Tenants

## Cleanup (If Needed)

To delete test users and start fresh:

```sql
USE Boilerplate;

-- Delete user permissions
DELETE FROM Auth.UserPermissions 
WHERE UserId IN (SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test');

-- Delete user roles
DELETE FROM Auth.UserRoles 
WHERE UserId IN (SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test');

-- Delete tenant users mapping
DELETE FROM Master.TenantUsers 
WHERE UserId IN (SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test');

-- Delete users
DELETE FROM Auth.Users 
WHERE Email LIKE '%.test';

PRINT 'Test users deleted successfully';
```

Then re-run 008_SeedTestUsers.sql.

## Summary

✅ Test users deployed and ready for testing
✅ Roles properly assigned (Admin, Manager, Staff, Customer)
✅ Permissions correctly assigned by role
✅ Can login via Swagger UI or cURL
✅ Ready for API testing and validation

Next: Open Swagger UI and test endpoints!

---

**Last Updated**: 2026-03-31
**Version**: 1.1 (Fixed SQL data types)
