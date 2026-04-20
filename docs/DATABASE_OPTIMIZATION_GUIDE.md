# Database Query Optimization Guide

Best practices for optimizing database queries with Dapper in SmartWorkz applications.

## The N+1 Problem

### What It Is

```csharp
// ❌ SLOW: Executes N+1 queries (1 + 100 = 101 total)
var users = await connection.QueryAsync<User>("SELECT * FROM Users");
foreach (var user in users)
{
    user.Orders = (await connection.QueryAsync<Order>(
        "SELECT * FROM Orders WHERE UserId = @UserId",
        new { UserId = user.Id })).ToList();
}
// Database roundtrips: 101
// Execution time: ~5 seconds for 100 users
```

### Why It's Slow

1. **Network Latency** — Each query adds 10-100ms network overhead
2. **Query Parsing** — SQL Server re-parses each query
3. **Connection Overhead** — Open/close connection for each query
4. **Resource Contention** — Database connection pool exhaustion

## Solution 1: QueryMultiple (Recommended)

Single roundtrip, multiple result sets:

```csharp
// ✅ FAST: Single query execution, single roundtrip
const string sql = @"
    SELECT * FROM Users WHERE Department = @Dept;
    SELECT * FROM Orders WHERE CreatedDate > @Date;
    SELECT * FROM Products WHERE IsActive = 1";

using (var reader = await connection.QueryMultipleAsync(sql, 
    new { Dept = "Sales", Date = DateTime.Now.AddMonths(-1) }))
{
    var users = (await reader.ReadAsync<User>()).ToList();
    var orders = (await reader.ReadAsync<Order>()).ToList();
    var products = (await reader.ReadAsync<Product>()).ToList();
}
// Database roundtrips: 1
// Execution time: ~50ms
```

### Advantages
- Single database roundtrip
- Single SQL compilation
- Connection reused
- 100x+ performance improvement

### When to Use
- Loading related data (users + orders + payments)
- Batch operations (counts, aggregates, detail records)
- Complex reports (multiple datasets)

## Solution 2: JOIN with Multi-Mapping

Combine data in a single query:

```csharp
// ✅ FAST: JOIN returns normalized data, Dapper maps to objects
const string sql = @"
    SELECT 
        u.UserId, u.Name, u.Email,
        o.OrderId, o.OrderDate, o.TotalAmount
    FROM Users u
    LEFT JOIN Orders o ON u.UserId = o.UserId
    WHERE u.Department = @Dept
    ORDER BY u.UserId";

var userDict = new Dictionary<int, User>();

await connection.QueryAsync<User, Order, User>(sql,
    (user, order) =>
    {
        // Dapper calls this for every row
        if (!userDict.TryGetValue(user.UserId, out var userEntry))
        {
            userEntry = user;
            userDict.Add(user.UserId, userEntry);
        }
        if (order != null)
            userEntry.Orders.Add(order);
        
        return userEntry;
    },
    new { Dept = "Sales" },
    splitOn: "OrderId");  // Column name where next type starts

var result = userDict.Values.ToList();
```

### Advantages
- True relational join (filtered at DB level)
- No duplicate data transfer
- Database does the heavy lifting
- Scales well for complex relationships

### When to Use
- Parent-child relationships (one-to-many)
- Filtering by related data (users with orders > $1000)
- Aggregations with details

### Handling Multiple Joins

```csharp
const string sql = @"
    SELECT 
        u.UserId, u.Name,
        o.OrderId, o.OrderDate,
        p.ProductId, p.ProductName
    FROM Users u
    LEFT JOIN Orders o ON u.UserId = o.UserId
    LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
    LEFT JOIN Products p ON oi.ProductId = p.ProductId
    WHERE u.Department = @Dept";

var userDict = new Dictionary<int, User>();

await connection.QueryAsync<User, Order, Product, User>(sql,
    (user, order, product) =>
    {
        if (!userDict.TryGetValue(user.UserId, out var userEntry))
        {
            userEntry = user;
            userDict.Add(user.UserId, userEntry);
        }
        
        if (order != null)
        {
            var orderEntry = userEntry.Orders.FirstOrDefault(o => o.OrderId == order.OrderId) 
                ?? new Order { OrderId = order.OrderId, OrderDate = order.OrderDate };
            
            if (product != null && !orderEntry.Products.Exists(p => p.ProductId == product.ProductId))
                orderEntry.Products.Add(product);
            
            if (!userEntry.Orders.Contains(orderEntry))
                userEntry.Orders.Add(orderEntry);
        }
        
        return userEntry;
    },
    new { Dept = "Sales" },
    splitOn: "OrderId,ProductId");

var result = userDict.Values.ToList();
```

## Solution 3: Batch Operations

Execute multiple commands in one call:

```csharp
// ❌ SLOW: 100 separate INSERT statements
foreach (var user in newUsers)
{
    await connection.ExecuteAsync(
        "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)", 
        user);
}
// Execution time: ~10 seconds

// ✅ FAST: Single batch INSERT
await connection.ExecuteAsync(
    "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)", 
    newUsers);  // Pass enumerable instead of single object
// Execution time: ~100ms (100x faster)
```

### Bulk Operations with Table-Valued Parameters

```csharp
// For very large batches (1M+ rows), use TVP
const string sql = @"
    INSERT INTO Users (Name, Email)
    SELECT Name, Email FROM @UserTable";

var dataTable = new DataTable("UserTable");
dataTable.Columns.Add("Name", typeof(string));
dataTable.Columns.Add("Email", typeof(string));

foreach (var user in users)
{
    dataTable.Rows.Add(user.Name, user.Email);
}

await connection.ExecuteAsync(sql, new { UserTable = dataTable.AsTableValuedParameter("dbo.UserTableType") });
```

## Solution 4: Query Result Caching

Cache expensive query results:

```csharp
public class UserService
{
    private readonly IMemoryCache _cache;
    private readonly IDbConnection _db;
    private const string UsersCachKey = "users:all";

    public async Task<List<User>> GetAllUsersAsync()
    {
        // Check cache first
        if (_cache.TryGetValue(UsersCachKey, out List<User> cached))
            return cached;

        // Cache miss: execute query
        var users = (await _db.QueryAsync<User>("SELECT * FROM Users")).ToList();

        // Store in cache for 1 hour
        _cache.Set(UsersCachKey, users, TimeSpan.FromHours(1));

        return users;
    }

    public async Task CreateUserAsync(User user)
    {
        await _db.ExecuteAsync(
            "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)", 
            user);

        // Invalidate cache after insert
        _cache.Remove(UsersCachKey);
    }
}
```

### Cache Invalidation Patterns

```csharp
// Time-based (simple, staleness acceptable)
_cache.Set(key, data, TimeSpan.FromMinutes(5));

// Event-based (reactive)
public event EventHandler<UserChangedEventArgs> UserChanged;
private void OnUserChanged(User user)
{
    _cache.Remove($"user:{user.Id}");
    UserChanged?.Invoke(this, new UserChangedEventArgs(user));
}

// Dependency-based (complex queries)
var cacheKey = $"users:department:{deptId}";
var dependencies = new[] { $"dept:{deptId}", "users:all" };
_cache.Set(cacheKey, data, 
    new CacheItemPolicy { 
        ChangeMonitors = new[] { 
            _monitor.CreateCacheEntryChangeMonitor(dependencies) 
        } 
    });
```

## Solution 5: Database Indexing

Indexes dramatically improve query speed:

```sql
-- ❌ SLOW: Full table scan
SELECT * FROM Orders WHERE UserId = 123;
-- Time: 5000ms (searches all 1M rows)

-- ✅ FAST: Index seek
CREATE NONCLUSTERED INDEX IX_Orders_UserId 
ON Orders(UserId) 
INCLUDE (OrderDate, TotalAmount);
-- Time: 5ms (seeks directly to UserId = 123)
```

### Index Design Guidelines

```sql
-- 1. Index frequently filtered columns
CREATE INDEX IX_Users_Department ON Users(Department);

-- 2. Include non-key columns for covering queries
CREATE INDEX IX_Orders_UserId 
ON Orders(UserId) 
INCLUDE (OrderDate, TotalAmount);

-- 3. Avoid excessive indexes (slow writes)
-- Rule of thumb: 5-7 indexes per table max

-- 4. Check index fragmentation
SELECT 
    name,
    avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED')
WHERE database_id = DB_ID()
    AND avg_fragmentation_in_percent > 10;

-- 5. Rebuild fragmented indexes
ALTER INDEX IX_Orders_UserId ON Orders REBUILD;
```

## Performance Checklist

- [ ] Identify N+1 queries (use SQL Profiler)
- [ ] Replace with QueryMultiple or JOIN
- [ ] Add indexes for WHERE clauses
- [ ] Cache expensive reads
- [ ] Batch write operations
- [ ] Monitor query execution times
- [ ] Load test with realistic data volumes
- [ ] Use query hints sparingly
- [ ] Archive old data periodically
- [ ] Regular index maintenance

## Monitoring & Profiling

### SQL Server Profiler

```sql
-- Find slow queries
SELECT 
    t.text,
    s.total_elapsed_time / 1000000 as total_seconds,
    s.execution_count,
    s.total_elapsed_time / s.execution_count / 1000 as avg_milliseconds
FROM sys.dm_exec_query_stats s
CROSS APPLY sys.dm_exec_sql_text(s.sql_handle) t
WHERE s.total_elapsed_time > 1000000  -- Over 1 second
ORDER BY s.total_elapsed_time DESC;
```

### Application-Level Logging

```csharp
public static class QueryTimer
{
    public static async Task<T> ExecuteWithTimingAsync<T>(
        IDbConnection db,
        string sql,
        Func<IDbConnection, Task<T>> query,
        ILogger logger)
    {
        var sw = Stopwatch.StartNew();
        var result = await query(db);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 1000)  // Flag queries > 1 second
            logger.LogWarning($"Slow query detected: {sw.ElapsedMilliseconds}ms\n{sql}");

        return result;
    }
}
```

## Common Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| N+1 queries | 100x slower | Use QueryMultiple or JOIN |
| Missing indexes | Full table scans | Create indexes on WHERE columns |
| Fetching unused columns | Wasted bandwidth | SELECT specific columns |
| Not parameterizing | SQL injection risk | Always use @param |
| Cursor-based processing | Extremely slow | Use set-based operations |
| SELECT * | Memory waste | Select needed columns only |
| Not caching reads | Redundant queries | Implement caching strategy |

## Summary

1. **N+1 = Slow** — Always load related data in single query
2. **Index = Fast** — Critical for performance at scale
3. **Cache = Scalable** — Reduces database load
4. **Batch = Efficient** — Combine multiple operations
5. **Monitor = Proactive** — Find problems early
