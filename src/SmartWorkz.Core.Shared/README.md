# SmartWorkz.Core.Shared

Shared utilities, data access, caching, and cross-cutting concerns for SmartWorkz applications.

## Getting Started

### Prerequisites
- .NET 9.0 or higher
- SQL Server 2019+ (for database operations)

### Installation

```xml
<ProjectReference Include="path/to/SmartWorkz.Core.Shared/SmartWorkz.Core.Shared.csproj" />
```

### Basic Usage

```csharp
using SmartWorkz.Core.Shared.Data;
using SmartWorkz.Core.Shared.Caching;
using SmartWorkz.Core.Shared.File;

// Database operations
var provider = DbProviderFactory.GetProvider(DatabaseProvider.SqlServer);

// Caching
var cache = new CacheManager();

// File operations
var resizer = new ImageResizer();
```

## Project Structure

- **Data/** — Database access, providers, and Dapper utilities
- **Caching/** — In-memory caching and cache attributes
- **File/** — File operations (upload, resize, delete)
- **Templates/** — Template engine with placeholder support
- **Utilities/** — Helper utilities and extensions
- **Extensions/** — Common extension methods
- **Attributes/** — Custom attributes (Cache, TypeFilter)

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Data.SqlClient | 5.1.5 | SQL Server connectivity |
| Dapper | 2.1.15 | Lightweight ORM |
| System.Drawing.Common | 8.0.0 | Image processing |
| System.Diagnostics.PerformanceCounter | 8.0.0 | Performance monitoring |
| Microsoft.Extensions.Caching.Abstractions | 9.0.0 | Caching abstractions |
| Microsoft.AspNetCore.Mvc.Core | 2.3.0 | Attribute support |

## Configuration

### Database Configuration

```csharp
// Use with dependency injection
services.AddScoped<IDbProvider>(provider => 
    DbProviderFactory.GetProvider(DatabaseProvider.SqlServer));

// Connection string in appsettings.json
"ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SmartWorkz;Integrated Security=true;"
}
```

### Caching Configuration

```csharp
// Register cache manager
services.AddMemoryCache();
services.AddScoped<ICacheManager, CacheManager>();
```

### Template Engine Configuration

```csharp
var engine = new TemplateEngine();
var result = engine.Process("Hello {Name}", new { Name = "World" });
// Result: "Hello World"
```

## Key Features

### Data Access
- **Database Provider Pattern** — Support for SQL Server, MySQL, PostgreSQL, SQLite
- **Dapper Integration** — Efficient parameterized queries
- **Connection Management** — Safe async connection handling

### Caching
- **Memory Cache** — In-process caching with expiration
- **Cache Attributes** — Declarative caching with `[Cache]` attribute
- **Cache Invalidation** — Built-in cache clearing strategies

### File Management
- **Image Processing** — Resize, validate, and process images
- **Secure Upload** — Validation and sanitization
- **Format Support** — JPEG, PNG, GIF, BMP, TIFF

### Template Processing
- **Placeholder Support** — `{PropertyName}` and `{{PropertyName}}` formats
- **Data Binding** — Simple object-to-template binding
- **Custom Formatters** — Extensible formatting pipeline

## Query Optimization & N+1 Prevention

### Problem: Reflection Overhead
Dapper uses reflection to map query results to objects. On hot paths, this adds overhead:

```csharp
// ❌ Inefficient: Reflection happens on every call
var result = await connection.QueryAsync<User>(sql, new { Id = 1 });
```

### Solution: Query Caching & Batching

**1. Use QueryMultiple for Related Data**

```csharp
// ✅ Efficient: Single roundtrip, single SQL execution
using (var connection = new SqlConnection(connectionString))
{
    const string sql = @"
        SELECT * FROM Users WHERE Id = @Id;
        SELECT * FROM UserRoles WHERE UserId = @Id;
        SELECT * FROM Permissions WHERE RoleId IN (SELECT RoleId FROM UserRoles WHERE UserId = @Id)";
    
    using (var reader = connection.QueryMultiple(sql, new { Id = userId }))
    {
        var user = reader.ReadSingle<User>();
        var roles = reader.Read<Role>();
        var permissions = reader.Read<Permission>();
    }
}
```

**2. Cache Reflection Results with Expression Trees**

For frequently-executed queries, consider:

```csharp
// Use Dapper's built-in SqlMapper.GetDeserializer for caching
var deserializer = SqlMapper.GetDeserializer<User>(
    reader: null, 
    effectiveType: typeof(User), 
    returnType: typeof(User),
    index: 0,
    length: 1,
    returnNullIfFirstMissing: false);
```

**3. Eager Load Instead of Lazy Load**

```csharp
// ❌ N+1 problem: Loop executes separate query per user
var users = connection.Query<User>("SELECT * FROM Users").ToList();
foreach (var user in users)
{
    user.Roles = connection.Query<Role>(
        "SELECT * FROM Roles WHERE UserId = @UserId", 
        new { user.Id }).ToList();
}

// ✅ Single query with JOIN
const string sql = @"
    SELECT u.*, r.* FROM Users u
    LEFT JOIN UserRoles ur ON u.Id = ur.UserId
    LEFT JOIN Roles r ON ur.RoleId = r.Id";

var userDict = new Dictionary<int, User>();
connection.Query<User, Role, User>(sql,
    (user, role) =>
    {
        if (!userDict.TryGetValue(user.Id, out var userEntry))
        {
            userEntry = user;
            userDict.Add(user.Id, userEntry);
        }
        if (role != null)
            userEntry.Roles.Add(role);
        return userEntry;
    },
    splitOn: "Id");
```

**4. Parameterized Bulk Operations**

```csharp
// ❌ Inefficient: Loop + reflection per insert
foreach (var user in users)
{
    await connection.ExecuteAsync(
        "INSERT INTO Users (Name) VALUES (@Name)", 
        user);
}

// ✅ Batch insert: Single execution
await connection.ExecuteAsync(
    "INSERT INTO Users (Name) VALUES (@Name)", 
    users);
```

**5. Use Compiled Queries for Hot Paths**

```csharp
// Cache the query plan
private static readonly Func<IDbConnection, int, Task<User>> GetUserById = 
    CompileQuery((IDbConnection db, int id) => 
        db.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { id }));

// Reuse without recompilation
var user = await GetUserById(connection, userId);
```

## Performance Comparison

| Approach | Roundtrips | SQL Executions | Reflection Calls | Speed |
|----------|-----------|-----------------|-----------------|-------|
| N+1 Loop | N+1 | N+1 | N+1 | ❌ Slowest |
| QueryMultiple | 1 | 1 | 3 | ✅ Fast |
| Bulk Insert | 1 | 1 | 1 | ✅ Fastest |
| Compiled | 1 | 1 | 1 | ✅ Fastest |

## Performance Considerations

- **Caching** — Use `[Cache]` attribute for frequently accessed data
- **Lazy Loading** — Avoid N+1 problems with eager loading or explicit queries
- **Image Optimization** — Resize images server-side to reduce bandwidth
- **Async Operations** — Use async methods for I/O-bound operations

## Testing

```csharp
[TestMethod]
public void TestCaching()
{
    var cache = new CacheManager();
    cache.Set("key", "value", TimeSpan.FromMinutes(5));
    Assert.AreEqual("value", cache.Get("key"));
}
```

## Contributing

- Follow SQL injection prevention patterns
- Use parameterized queries exclusively
- Cache responsibly to avoid stale data
- Test image processing with various formats
