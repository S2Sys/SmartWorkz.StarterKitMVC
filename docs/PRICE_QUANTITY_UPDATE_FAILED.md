# 🚨 Price Quantity Update Failed - Scenario Guide

**Document Type:** Error Scenario & Recovery Guide  
**Status:** Active Issue  
**Severity:** High 🔴  
**Last Updated:** April 21, 2026

---

## Executive Summary

**Scenario:** Product price and/or quantity update operations fail during:
- Bulk price updates
- Inventory synchronization
- Multi-tenant product modifications
- Concurrent user updates
- Database transaction failures

**Impact:** Data inconsistency, inventory mismatch, revenue loss

**Root Causes:** Transaction failures, validation errors, concurrency conflicts, permission issues

---

## Problem Definition

### What Happens

```
User Action: Update Product Price/Quantity
    ↓
Request sent to API
    ↓
Validation PASSES
    ↓
Database Transaction STARTS
    ↓
❌ UPDATE FAILS (mid-transaction)
    ↓
Partial State: Price updated, Quantity NOT updated
    ↓
DATA INCONSISTENCY ❌
```

---

## Root Cause Analysis

### 1. **Transaction Failures** 🔴 CRITICAL

```csharp
// ❌ PROBLEM: No transaction handling
public async Task UpdateProductAsync(UpdateProductDto dto)
{
    product.Price = dto.Price;
    await _repo.UpdateAsync(product);
    
    inventory.Quantity = dto.Quantity;
    await _inventoryRepo.UpdateAsync(inventory);
    // If 2nd update fails, Price already changed!
}

// ✅ SOLUTION: Use transaction
public async Task UpdateProductAsync(UpdateProductDto dto)
{
    using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
        {
            product.Price = dto.Price;
            await _repo.UpdateAsync(product);
            
            inventory.Quantity = dto.Quantity;
            await _inventoryRepo.UpdateAsync(inventory);
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

### 2. **Validation Errors** 🟠 HIGH

```csharp
// ❌ NO VALIDATION
public async Task UpdateProductAsync(UpdateProductDto dto)
{
    if (dto.Price < 0)           // Missing!
        return Result.Fail("Price invalid");
    
    if (dto.Quantity < 0)        // Missing!
        return Result.Fail("Quantity invalid");
}

// ✅ PROPER VALIDATION
public async Task<Result> ValidateUpdateAsync(UpdateProductDto dto)
{
    var errors = new List<Error>();
    
    if (dto.Price < 0)
        errors.Add(new Error("INVALID_PRICE", "Price cannot be negative"));
    
    if (dto.Quantity < 0)
        errors.Add(new Error("INVALID_QUANTITY", "Quantity cannot be negative"));
    
    if (dto.Quantity > 999999)
        errors.Add(new Error("QTY_LIMIT", "Quantity exceeds maximum"));
    
    return errors.Any() 
        ? Result.Fail(errors) 
        : Result.Ok();
}
```

---

### 3. **Concurrency Conflicts** 🟠 HIGH

```csharp
// ❌ RACE CONDITION
User A reads: Price = $10, Qty = 100
User B reads: Price = $10, Qty = 100

User A updates to: Price = $15, Qty = 95
User B updates to: Price = $12, Qty = 90

// Last write wins: Price = $12, Qty = 90
// But User A expected: Price = $15, Qty = 95

// ✅ SOLUTION: Optimistic Concurrency with RowVersion
public class Product : IEntity<int>
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
    [Timestamp]  // EF Core will check this
    public byte[] RowVersion { get; set; } = null!;
}

// In update:
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    return Result.Fail("CONCURRENCY_CONFLICT", 
        "Product was updated by another user. Please reload and try again.");
}
```

---

### 4. **Permission Issues** 🟠 HIGH

```csharp
// ❌ NO PERMISSION CHECK
public async Task<Result> UpdateProductAsync(int productId, UpdateProductDto dto)
{
    var product = await _repo.GetByIdAsync(productId);
    // No permission validation!
    product.Price = dto.Price;
    await _repo.UpdateAsync(product);
}

// ✅ WITH PERMISSION CHECK
public async Task<Result> UpdateProductAsync(int productId, UpdateProductDto dto)
{
    var product = await _repo.GetByIdAsync(productId);
    
    // Check tenant ownership
    if (product.TenantId != _tenantContext.TenantId)
        return Result.Fail("UNAUTHORIZED", "Not authorized to update this product");
    
    // Check permissions
    if (!await _authService.HasPermissionAsync("EditProduct"))
        return Result.Fail("FORBIDDEN", "You do not have permission to edit products");
    
    product.Price = dto.Price;
    await _repo.UpdateAsync(product);
}
```

---

### 5. **Database Constraints** 🔴 CRITICAL

```csharp
// ❌ CONSTRAINT VIOLATION NOT CAUGHT
public async Task<Result> UpdateProductAsync(UpdateProductDto dto)
{
    // Database has CHECK constraint: Price >= 0
    // But no validation here
    product.Price = -5;
    await _context.SaveChangesAsync(); // ❌ Throws SqlException
}

// ✅ PROPER CONSTRAINT HANDLING
public async Task<Result> UpdateProductAsync(UpdateProductDto dto)
{
    if (dto.Price < 0)
        return Result.Fail("PRICE_NEGATIVE", "Price must be >= 0");
    
    if (dto.Quantity < 0)
        return Result.Fail("QTY_NEGATIVE", "Quantity must be >= 0");
    
    try
    {
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();
        return Result.Ok();
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
    {
        if (sqlEx.Number == 547) // Foreign key violation
            return Result.Fail("FK_ERROR", "Referenced record not found");
        
        if (sqlEx.Number == 2627) // Unique constraint
            return Result.Fail("UNIQUE_ERROR", "Duplicate value not allowed");
        
        throw;
    }
}
```

---

### 6. **Network Timeout** 🟡 MEDIUM

```csharp
// ❌ NO TIMEOUT HANDLING
public async Task UpdateAsync(Product product)
{
    await _context.SaveChangesAsync(); // Could timeout silently
}

// ✅ WITH TIMEOUT
public async Task UpdateAsync(Product product)
{
    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
    {
        try
        {
            await _context.SaveChangesAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return Result.Fail("TIMEOUT", "Update operation timed out. Please retry.");
        }
    }
}
```

---

## Error Scenarios

### Scenario 1: Price Update Succeeds, Quantity Update Fails

```
Initial State:
  Product: Price=$100, Quantity=50

Update Request:
  Price: $120, Quantity: 40

Execution:
  ✅ Price updated to $120
  ❌ Quantity update fails (FK constraint)

Result: ❌ INCONSISTENT STATE
  Product: Price=$120, Quantity=50 (WRONG!)
```

**Recovery:**
```csharp
// Use transaction to rollback both
using var transaction = await db.Database.BeginTransactionAsync();
try {
    await UpdatePrice(product, 120);
    await UpdateQuantity(product, 40);
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync(); // Both rollback
    throw;
}
```

---

### Scenario 2: Concurrent Updates

```
Time  User A                  User B
─────────────────────────────────────
T1    Reads: P=$100, Q=50
T2                            Reads: P=$100, Q=50
T3    Updates: P=$120, Q=40
T4    Commits: ✅
T5                            Updates: P=$110, Q=45
T6                            Commits: ✅ (overwrites!)

Result: User A's changes LOST ❌
```

**Recovery:**
```csharp
// Add RowVersion (optimistic concurrency)
[Timestamp]
public byte[] RowVersion { get; set; } = null!;

// EF detects conflict
catch (DbUpdateConcurrencyException ex)
{
    // Reload latest values
    var latest = await _repo.GetByIdAsync(productId);
    
    // Return conflict with current values
    return Result.Fail("CONFLICT", 
        "Product was updated. Current: P=$110, Q=45");
}
```

---

### Scenario 3: Permission Denied Mid-Update

```
User Request: Update Product (Regular User)

Validation:
  ✅ Price valid
  ✅ Quantity valid
  ✅ Database state valid

Permission Check: ❌ FAILS (not admin)

Result: Request rejected, no changes made ✅
```

---

### Scenario 4: Database Constraint Violation

```
Request: Update Price to $-50 (invalid)

Validation Layer: ❌ Catches (if implemented)
Database Constraint: ❌ Would catch (CHECK constraint)

Result: Error returned to user
```

---

## Detection & Diagnosis

### Monitoring Strategy

```csharp
// Log all update operations
public class ProductUpdateLogger
{
    public async Task LogUpdateAttemptAsync(
        int productId, 
        UpdateProductDto dto,
        string userId)
    {
        var log = new AuditLog
        {
            EntityType = "Product",
            EntityId = productId,
            Operation = "Update",
            NewValues = JsonConvert.SerializeObject(dto),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };
        
        await _auditRepo.AddAsync(log);
    }
    
    public async Task LogUpdateFailureAsync(
        int productId,
        string errorCode,
        string errorMessage)
    {
        var log = new ErrorLog
        {
            EntityType = "Product",
            EntityId = productId,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        };
        
        await _errorRepo.AddAsync(log);
    }
}
```

### Alert Conditions

```
🔴 CRITICAL ALERTS:
  - 5+ failed updates in 1 minute
  - Concurrency conflicts on same product
  - Permission denied for admin user
  - Database constraint violations

🟠 HIGH ALERTS:
  - Transaction timeouts
  - Slow update queries (>5s)
  - Concurrent user conflicts
```

---

## Recovery Procedures

### Step 1: Identify the Failure

```csharp
// Check error logs
var failedUpdate = await db.ErrorLogs
    .Where(x => x.EntityType == "Product" && x.EntityId == productId)
    .OrderByDescending(x => x.Timestamp)
    .FirstOrDefaultAsync();

Console.WriteLine($"Error: {failedUpdate.ErrorCode}");
Console.WriteLine($"Message: {failedUpdate.ErrorMessage}");
```

---

### Step 2: Verify Current State

```csharp
// Get current state
var product = await db.Products.FindAsync(productId);
var inventory = await db.Inventory.FindAsync(productId);

Console.WriteLine($"Price: {product.Price}");
Console.WriteLine($"Quantity: {inventory.Quantity}");

// Compare with audit log
var lastGood = await db.AuditLogs
    .Where(x => x.EntityId == productId && x.Operation == "Update")
    .OrderByDescending(x => x.Timestamp)
    .FirstOrDefaultAsync();
```

---

### Step 3: Correct the Data

```csharp
// Option 1: Retry the update
public async Task<Result> RetryUpdateAsync(int productId, UpdateProductDto dto)
{
    var product = await db.Products.FindAsync(productId);
    
    // Validate again
    var validation = await ValidateAsync(dto);
    if (validation.IsFailure)
        return validation;
    
    // Apply changes with transaction
    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        
        return Result.Ok("Update successful");
    }
    catch
    {
        await tx.RollbackAsync();
        return Result.Fail("Retry failed");
    }
}
```

---

### Step 4: Validate Fix

```csharp
// Verify consistency
public async Task<Result> ValidateConsistencyAsync(int productId)
{
    var product = await db.Products.FindAsync(productId);
    var inventory = await db.Inventory.FindAsync(productId);
    var audit = await db.AuditLogs
        .Where(x => x.EntityId == productId)
        .OrderByDescending(x => x.Timestamp)
        .FirstOrDefaultAsync();
    
    var errors = new List<string>();
    
    if (product == null)
        errors.Add("Product not found");
    
    if (inventory == null)
        errors.Add("Inventory record not found");
    
    if (product.Price < 0)
        errors.Add($"Invalid price: {product.Price}");
    
    if (inventory.Quantity < 0)
        errors.Add($"Invalid quantity: {inventory.Quantity}");
    
    return errors.Any()
        ? Result.Fail(string.Join(", ", errors))
        : Result.Ok("Consistency verified");
}
```

---

## Prevention Strategy

### Best Practices

```csharp
// 1. ALWAYS USE TRANSACTIONS
using var transaction = await db.Database.BeginTransactionAsync();
try {
    // Multi-step operation
    await step1Async();
    await step2Async();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}

// 2. VALIDATE BEFORE SAVING
var validation = await ValidateAsync(dto);
if (validation.IsFailure)
    return validation;

// 3. CHECK PERMISSIONS
if (!await authService.HasPermissionAsync("EditProduct"))
    return Result.Fail("UNAUTHORIZED");

// 4. HANDLE CONCURRENCY
if (product.RowVersion != dto.RowVersion)
    return Result.Fail("CONCURRENCY_CONFLICT");

// 5. LOG ALL CHANGES
await auditService.LogAsync(EntityType.Product, productId, 
    "Update", oldValues, newValues, userId);

// 6. USE RESULT<T> PATTERN
return Result.Ok(updatedProduct);
```

---

## Implementation Checklist

- [ ] Add transaction handling to all update methods
- [ ] Implement comprehensive validation layer
- [ ] Add optimistic concurrency control (RowVersion)
- [ ] Implement permission checks
- [ ] Add audit logging
- [ ] Create error handling middleware
- [ ] Write unit tests for failure scenarios
- [ ] Add monitoring and alerts
- [ ] Create recovery procedures
- [ ] Document rollback procedures
- [ ] Train team on error handling
- [ ] Test failover scenarios

---

## Code Example: Correct Implementation

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<Result<ProductDto>>> UpdateProduct(
    int id, 
    UpdateProductRequest request)
{
    try
    {
        // 1. VALIDATE INPUT
        if (id <= 0 || request == null)
            return BadRequest(Result.Fail("Invalid input"));
        
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(Result.Fail(validation.Errors.Select(e => e.ErrorMessage)));
        
        // 2. CHECK PERMISSIONS
        if (!await _authService.HasPermissionAsync("EditProduct"))
            return Forbid();
        
        // 3. LOAD ENTITY
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (product == null)
            return NotFound(Result.Fail("Product not found"));
        
        // 4. CHECK TENANT
        if (product.TenantId != _tenantContext.TenantId)
            return Forbid();
        
        // 5. CHECK CONCURRENCY
        if (Convert.ToBase64String(product.RowVersion) != request.RowVersion)
            return Conflict(Result.Fail("CONCURRENCY_CONFLICT", 
                "Product was modified by another user"));
        
        // 6. APPLY CHANGES WITH TRANSACTION
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            product.Price = request.Price;
            product.Quantity = request.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = _userContext.UserId;
            
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            
            // 7. LOG CHANGE
            await _auditService.LogAsync(
                EntityType.Product, 
                id,
                AuditOperation.Update,
                oldValues: null,
                newValues: request,
                userId: _userContext.UserId);
            
            await transaction.CommitAsync();
            
            // 8. RETURN SUCCESS
            var dto = _mapper.Map<ProductDto>(product);
            return Ok(Result.Ok(dto));
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict(Result.Fail("CONCURRENCY_FAILED"));
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database update failed for product {productId}", id);
            return BadRequest(Result.Fail("DATABASE_ERROR", "Update failed due to data constraints"));
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error updating product {productId}", id);
        return StatusCode(500, Result.Fail("INTERNAL_ERROR", "An unexpected error occurred"));
    }
}
```

---

**Status:** Active Issue  
**Next Review:** May 1, 2026  
**Owner:** Development Team
