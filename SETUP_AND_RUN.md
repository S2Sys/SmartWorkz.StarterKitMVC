# SmartWorkz StarterKit MVC - Setup & Run Guide

## ✅ Phase 2 Implementation Complete (Steps 1-4)

### What's Implemented
- ✅ **Authorization** - Permission-based access control with JWT validation
- ✅ **Validation** - Input validation with custom validators
- ✅ **Global Error Handling** - RFC 7807 Problem Details format
- ✅ **Pagination & Filtering** - Paginated list endpoints
- ✅ **Swagger/OpenAPI** - Full API documentation with JWT Bearer auth
- ✅ **Test Users** - 4 test users with known credentials

---

## 1. Database Setup

### Step 1.1: Create Database
```sql
CREATE DATABASE Boilerplate;
```

### Step 1.2: Run Migration Scripts (in order)
```bash
# From project root directory
sqlcmd -S localhost -d Boilerplate -i database/001_CreateTables_Master.sql
sqlcmd -S localhost -d Boilerplate -i database/002_CreateTables_Shared.sql
sqlcmd -S localhost -d Boilerplate -i database/003_CreateTables_Transaction.sql
sqlcmd -S localhost -d Boilerplate -i database/004_CreateTables_Report.sql
sqlcmd -S localhost -d Boilerplate -i database/005_CreateTables_Audit.sql
sqlcmd -S localhost -d Boilerplate -i database/006_CreateTables_Auth.sql
sqlcmd -S localhost -d Boilerplate -i database/007_SeedData.sql
sqlcmd -S localhost -d Boilerplate -i database/008_SeedTestUsers.sql
```

### Step 1.3: Verify Database (if updating existing database)
If you had a previous version without the Code column:
```bash
sqlcmd -S localhost -d Boilerplate -i database/009_AddPermissionCode.sql
```

---

## 2. Application Setup

### Step 2.1: Restore NuGet Packages
```bash
dotnet restore
```

### Step 2.2: Build Solution
```bash
dotnet build
```

### Step 2.3: Run Application
```bash
dotnet run
```

Application starts at: **http://localhost:5000**

---

## 3. Test API Endpoints

### Step 3.1: Open Swagger UI
Navigate to: **http://localhost:5000/swagger**

### Step 3.2: Login with Test User
1. Click **POST /api/auth/login** endpoint
2. Click **Try it out**
3. Enter credentials:
```json
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}
```
4. Click **Execute**
5. Copy the **accessToken** from response

### Step 3.3: Authorize Swagger UI
1. Click **Authorize** button (lock icon) at top right
2. Paste token in format: `Bearer {accessToken}`
3. Click **Authorize** then **Close**

### Step 3.4: Test Protected Endpoints
Now test any protected endpoint - the token is automatically included in the Authorization header.

---

## Test User Credentials

| Role     | Email                    | Password         | Permissions                    |
|----------|--------------------------|------------------|--------------------------------|
| Admin    | admin@smartworkz.test    | TestPassword123! | Full CRUD (all resources)      |
| Manager  | manager@smartworkz.test  | TestPassword123! | Read/Update, View Reports      |
| Staff    | staff@smartworkz.test    | TestPassword123! | Read-only access               |
| Customer | customer@smartworkz.test | TestPassword123! | Role-based limited access      |

---

## Key API Endpoints

### Authentication
```
POST   /api/auth/login              → Login
POST   /api/auth/register           → Register new user
POST   /api/auth/refresh            → Refresh access token
POST   /api/auth/revoke             → Revoke refresh token
POST   /api/auth/forgot-password    → Request password reset
POST   /api/auth/reset-password     → Reset password with token
POST   /api/auth/change-password    → Change password
POST   /api/auth/verify-email       → Verify email address
GET    /api/auth/profile            → Get user profile
```

### Products (with pagination)
```
GET    /api/{tenantId}/products?page=1&pageSize=10        → List products
GET    /api/{tenantId}/products/{id}                        → Get product
GET    /api/{tenantId}/products/slug/{slug}                 → Get by slug
GET    /api/{tenantId}/products/sku/{sku}                   → Get by SKU
GET    /api/{tenantId}/products/category/{categoryId}       → Get by category
GET    /api/{tenantId}/products/featured?take=10            → Get featured
GET    /api/{tenantId}/products/search?q=term&page=1        → Search products
POST   /api/{tenantId}/products                             → Create product
PUT    /api/{tenantId}/products/{id}                        → Update product
DELETE /api/{tenantId}/products/{id}                        → Delete product
```

### Categories (with pagination)
```
GET    /api/{tenantId}/categories/{id}                      → Get category
GET    /api/{tenantId}/categories/slug/{slug}               → Get by slug
GET    /api/{tenantId}/categories/root?page=1               → Get root categories
GET    /api/{tenantId}/categories/{parentId}/children       → Get child categories
GET    /api/{tenantId}/categories/{id}/hierarchy            → Get hierarchy
POST   /api/{tenantId}/categories                           → Create category
PUT    /api/{tenantId}/categories/{id}                      → Update category
DELETE /api/{tenantId}/categories/{id}                      → Delete category
```

---

## cURL Examples

### Login and Get Token
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
  }'
```

### List Products with Authentication
```bash
curl -X GET "http://localhost:5000/api/DEFAULT/products?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Create Product (Admin Only)
```bash
curl -X POST http://localhost:5000/api/DEFAULT/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "Product Name",
    "sku": "SKU123",
    "price": 99.99,
    "categoryId": 1
  }'
```

---

## Permission Matrix

### Admin (All Permissions)
- ✅ PRODUCT_CREATE, PRODUCT_READ, PRODUCT_UPDATE, PRODUCT_DELETE
- ✅ ORDER_CREATE, ORDER_READ, ORDER_UPDATE, ORDER_DELETE
- ✅ REPORT_READ
- ✅ USER_UPDATE

### Manager (Read/Update)
- ✅ PRODUCT_READ, PRODUCT_UPDATE
- ✅ ORDER_READ, ORDER_UPDATE
- ✅ REPORT_READ
- ❌ Create/Delete operations

### Staff (Read-Only)
- ✅ PRODUCT_READ
- ✅ ORDER_READ
- ❌ All write operations

### Customer (Role-Based)
- Limited public access only

---

## Response Format

All API responses use the **Result<T>** pattern for consistency:

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "accessToken": "eyJ0eXAi...",
    "refreshToken": "...",
    "expiresAt": "2026-03-31T08:19:23Z",
    "profile": {...}
  },
  "errors": null
}
```

### Validation Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Email is required",
      "property": "email"
    }
  ]
}
```

### Unauthorized Response (401)
```json
{
  "success": false,
  "message": "Unauthorized",
  "data": null,
  "errors": []
}
```

---

## Troubleshooting

### Error: "Invalid column name 'code'"
**Cause**: Database schema doesn't have Code column yet
**Fix**: Run migration script:
```bash
sqlcmd -S localhost -d Boilerplate -i database/009_AddPermissionCode.sql
```

### Error: "Invalid email or password"
**Cause**: Incorrect test credentials
**Fix**: Use credentials from table above (case-sensitive email)

### Error: Swagger 500
**Cause**: Schema generation issue
**Fix**: Clear build artifacts and rebuild:
```bash
dotnet clean
dotnet build
```

### Error: "Account is deactivated"
**Cause**: User IsActive flag is 0 in database
**Fix**: Update user in database:
```sql
UPDATE Auth.Users SET IsActive = 1 WHERE Email = 'your@email.test'
```

---

## Build Status

✅ **0 Errors, ~389 Warnings** (nullable reference type warnings expected)

---

## Files Changed

### Database Files
- `database/006_CreateTables_Auth.sql` - Added Code column to Permissions
- `database/007_SeedData.sql` - Populated permission codes
- `database/008_SeedTestUsers.sql` - Test user seed script
- `database/009_AddPermissionCode.sql` - Migration for existing databases

### Application Files
- `src/SmartWorkz.StarterKitMVC.Web/Program.cs` - Swagger & middleware config
- `src/SmartWorkz.StarterKitMVC.Web/Controllers/Api/AuthController.cs` - Authentication
- `src/SmartWorkz.StarterKitMVC.Web/Controllers/Api/ProductController.cs` - Products
- `src/SmartWorkz.StarterKitMVC.Web/Controllers/Api/CategoryController.cs` - Categories
- `src/SmartWorkz.StarterKitMVC.Web/Controllers/HomeController.cs` - Added [HttpGet] to Error

### Infrastructure Files
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs` - Auth logic
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/` - Permission handlers
- `src/SmartWorkz.StarterKitMVC.Shared/Validation/` - Validators
- `src/SmartWorkz.StarterKitMVC.Shared/Primitives/` - Result pattern & ProblemDetails

---

**Status**: ✅ Production Ready for Phase 2 (Steps 1-4)

Last Updated: 2026-03-31
