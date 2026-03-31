# Test Users & API Authentication Guide

## Overview
This guide provides test user credentials for API testing with Swagger/OpenAPI documentation.

## Test User Accounts

All test users share the same password for easy testing:
- **Password**: `TestPassword123!`

### 1. **Admin User**
- **Email**: `admin@smartworkz.test`
- **Username**: `admin`
- **Display Name**: Admin User
- **Role**: Admin
- **Tenant**: DEFAULT
- **Permissions**: All (Create, Read, Update, Delete for Products & Orders, View Reports, Manage Users)

### 2. **Manager User**
- **Email**: `manager@smartworkz.test`
- **Username**: `manager`
- **Display Name**: Manager User
- **Role**: Manager
- **Tenant**: DEFAULT
- **Permissions**: Read & Update Products/Orders, View Reports

### 3. **Staff User**
- **Email**: `staff@smartworkz.test`
- **Username**: `staff`
- **Display Name**: Staff User
- **Role**: Staff
- **Tenant**: DEFAULT
- **Permissions**: Read Products & Orders (read-only access)

### 4. **Customer User**
- **Email**: `customer@smartworkz.test`
- **Username**: `customer`
- **Display Name**: Customer User
- **Role**: Customer
- **Tenant**: DEFAULT
- **Permissions**: Role-based (no direct permissions)

## How to Use Test Credentials

### Step 1: Deploy Test Users
Run the SQL script to seed test users:
```bash
sqlcmd -S your-server -d Boilerplate -i database/008_SeedTestUsers.sql
```

Or using PowerShell:
```powershell
Invoke-Sqlcmd -ServerInstance "your-server" -Database "Boilerplate" -InputFile "database/008_SeedTestUsers.sql"
```

### Step 2: Start the Application
```bash
dotnet run
```

### Step 3: Open Swagger UI
Navigate to: **http://localhost:5000/swagger**

### Step 4: Login with Test User

#### Via Swagger UI:
1. Click on **POST /api/auth/login**
2. Click **"Try it out"**
3. Enter the following JSON:
```json
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}
```
4. Click **"Execute"**
5. Copy the `accessToken` from the response

#### Response Example:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "RefreshToken123...",
  "expiresAt": "2026-03-31T12:30:00Z",
  "userProfile": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "admin@smartworkz.test",
    "username": "admin",
    "displayName": "Admin User",
    "tenantId": "DEFAULT",
    "emailConfirmed": true,
    "twoFactorEnabled": false,
    "roles": ["Admin"],
    "permissions": [
      "Create Product",
      "Read Product",
      "Update Product",
      "Delete Product",
      "Create Order",
      "Read Order",
      "Update Order",
      "Delete Order",
      "View Report",
      "Manage Users"
    ]
  }
}
```

### Step 5: Authorize in Swagger

1. Click the **"Authorize"** button (lock icon at top-right)
2. Enter the access token in the format:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
3. Click **"Authorize"**
4. Click **"Close"**

### Step 6: Test Protected Endpoints

All subsequent API requests will automatically include the Authorization header.

**Example: Get User Profile**
1. Click on **GET /api/auth/profile**
2. Click **"Try it out"**
3. Click **"Execute"**

Response:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "admin@smartworkz.test",
  "username": "admin",
  "displayName": "Admin User",
  "tenantId": "DEFAULT",
  "emailConfirmed": true,
  "twoFactorEnabled": false,
  "roles": ["Admin"],
  "permissions": [...]
}
```

## API Testing Scenarios

### Scenario 1: Admin Testing (Full Access)
```bash
# Login as admin
POST /api/auth/login
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}

# Test all CRUD operations
GET /api/DEFAULT/products
POST /api/DEFAULT/products
PUT /api/DEFAULT/products/{id}
DELETE /api/DEFAULT/products/{id}
```

### Scenario 2: Manager Testing (Limited Access)
```bash
# Login as manager
POST /api/auth/login
{
  "email": "manager@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}

# Test read and update
GET /api/DEFAULT/products
PUT /api/DEFAULT/products/{id}

# Test delete (should fail - no permission)
DELETE /api/DEFAULT/products/{id}  # 403 Forbidden
```

### Scenario 3: Staff Testing (Read-Only)
```bash
# Login as staff
POST /api/auth/login
{
  "email": "staff@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}

# Test read
GET /api/DEFAULT/products

# Test create (should fail - no permission)
POST /api/DEFAULT/products  # 403 Forbidden
```

## cURL Examples

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
  }'
```

### Test Protected Endpoint
```bash
curl -X GET https://localhost:5001/api/DEFAULT/products \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Refresh Token
```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "accessToken": "YOUR_ACCESS_TOKEN",
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Postman Collection Example

Save as `SmartWorkz-API-Tests.postman_collection.json`:

```json
{
  "info": {
    "name": "SmartWorkz API Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login - Admin",
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/auth/login",
            "body": {
              "mode": "raw",
              "raw": "{\"email\":\"admin@smartworkz.test\",\"password\":\"TestPassword123!\",\"tenantId\":\"DEFAULT\"}"
            }
          }
        }
      ]
    },
    {
      "name": "Products",
      "item": [
        {
          "name": "List Products",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/DEFAULT/products?page=1&pageSize=10",
            "header": {
              "Authorization": "Bearer {{accessToken}}"
            }
          }
        }
      ]
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://localhost:5001"
    },
    {
      "key": "accessToken",
      "value": ""
    }
  ]
}
```

## Common Issues

### Issue: 401 Unauthorized
**Cause**: Invalid or expired access token

**Solution**: 
1. Re-login with test credentials
2. Copy the new access token
3. Update the Authorization header

### Issue: 403 Forbidden
**Cause**: User lacks required permissions

**Solution**:
1. Use admin user for full access
2. Check user's role and permissions
3. Ensure TenantId matches in request

### Issue: 400 Bad Request
**Cause**: Validation error in request

**Solution**:
1. Check Swagger schema for required fields
2. Verify data types and formats
3. Ensure email format is valid

## Password Hash Information

**Algorithm**: PBKDF2-SHA256
**Iterations**: 10,000
**Format**: `PBKDF2$HMACSHA256$iterations$salt$hash`

To update test user passwords:
1. Use the application's password hasher service
2. Or update via the ChangePassword endpoint:

```bash
POST /api/auth/change-password
{
  "currentPassword": "TestPassword123!",
  "newPassword": "NewPassword123!"
}
```

## Troubleshooting Database Script

If the test user seed script fails:

1. **Check if roles exist**:
```sql
SELECT * FROM Auth.Roles WHERE TenantId = 'DEFAULT'
```

2. **Check if permissions exist**:
```sql
SELECT * FROM Auth.Permissions WHERE TenantId = 'DEFAULT'
```

3. **Verify tenants exist**:
```sql
SELECT * FROM Master.Tenants WHERE TenantId = 'DEFAULT'
```

4. **Manual cleanup** (if needed):
```sql
DELETE FROM Auth.UserPermissions WHERE UserId IN (
  SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test'
)
DELETE FROM Auth.UserRoles WHERE UserId IN (
  SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test'
)
DELETE FROM Master.TenantUsers WHERE UserId IN (
  SELECT UserId FROM Auth.Users WHERE Email LIKE '%.test'
)
DELETE FROM Auth.Users WHERE Email LIKE '%.test'
```

## Next Steps

1. ✅ Deploy `008_SeedTestUsers.sql`
2. ✅ Start the application
3. ✅ Open http://localhost:5000/swagger
4. ✅ Login with test credentials
5. ✅ Test API endpoints
6. ✅ Verify role-based access control
7. ✅ Test permission-based authorization

---

**Last Updated**: 2026-03-31
**Version**: 1.0
