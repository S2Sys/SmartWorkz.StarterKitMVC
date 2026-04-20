# Data Access Guide

Complete reference for database access patterns, CSV/XML serialization, and multi-database support in SmartWorkz.

---

## Overview

SmartWorkz provides **flexible data access tools** for different scenarios:

1. **ADO.NET Helper** — Low-level database access for complex queries and stored procedures
2. **Dapper Helper** — Lightweight ORM extension methods (reflection-based, no package dependency)
3. **CSV Helper** — RFC-4180 compliant CSV reading and writing
4. **XML Helper** — XML serialization and XPath queries
5. **DbProviderFactory** — Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite, Oracle)

Choose the right tool for your use case:
- **AdoHelper** — Stored procedures, complex queries, raw SQL
- **DapperHelper** — Simple CRUD operations, type-safe queries
- **EF Core** — Complex object graphs, navigation properties
- **CsvHelper** — Bulk imports, exports, data migration
- **XmlHelper** — Configuration, message parsing, legacy systems

---

## ADO.NET Helper

### AdoHelper — Direct Database Access

`AdoHelper` provides static methods for executing SQL commands, queries, stored procedures, and transactions using any `IDbProvider`.

```csharp
using SmartWorkz.Core.Shared.Data;

var provider = DbProviderFactory.GetProvider("SqlServer");
var connectionString = "Server=localhost;Database=MyDb;...";
```

### Methods

#### ExecuteQueryAsync<T> — Get Multiple Rows

```csharp
public static async Task<Result<List<T>>> ExecuteQueryAsync<T>(
    IDbProvider provider,
    string connectionString,
    string query,
    Dictionary<string, object>? parameters = null,
    CommandType commandType = CommandType.Text
)
```

**Example:**

```csharp
var query = "SELECT * FROM Products WHERE CategoryId = @CategoryId";
var parameters = new Dictionary<string, object> { ["CategoryId"] = 5 };

var result = await AdoHelper.ExecuteQueryAsync<Product>(
    provider, connectionString, query, parameters
);

if (result.Succeeded)
{
    var products = result.Data; // List<Product>
    foreach (var product in products)
    {
        Console.WriteLine($"{product.Name}: ${product.Price}");
    }
}
```

#### ExecuteScalarAsync<T> — Get Single Value

```csharp
public static async Task<Result<T>> ExecuteScalarAsync<T>(
    IDbProvider provider,
    string connectionString,
    string query,
    Dictionary<string, object>? parameters = null
)
```

**Example:**

```csharp
var query = "SELECT COUNT(*) FROM Products WHERE Price > @MinPrice";
var parameters = new Dictionary<string, object> { ["MinPrice"] = 100 };

var result = await AdoHelper.ExecuteScalarAsync<int>(provider, connectionString, query, parameters);

if (result.Succeeded)
{
    Console.WriteLine($"Expensive products: {result.Data}");
}
```

#### ExecuteNonQueryAsync — Execute INSERT/UPDATE/DELETE

```csharp
public static async Task<Result<int>> ExecuteNonQueryAsync(
    IDbProvider provider,
    string connectionString,
    string query,
    Dictionary<string, object>? parameters = null,
    CommandType commandType = CommandType.Text
)
```

Returns the number of rows affected.

**Example:**

```csharp
var query = "UPDATE Products SET Price = @NewPrice WHERE Id = @ProductId";
var parameters = new Dictionary<string, object>
{
    ["NewPrice"] = 199.99,
    ["ProductId"] = 42
};

var result = await AdoHelper.ExecuteNonQueryAsync(provider, connectionString, query, parameters);

if (result.Succeeded)
{
    Console.WriteLine($"Updated {result.Data} rows");
}
```

#### ExecuteStoredProcedureAsync<T> — Call Stored Procedure

```csharp
var result = await AdoHelper.ExecuteStoredProcedureAsync<OrderSummary>(
    provider,
    connectionString,
    "sp_GetOrderSummary",
    new Dictionary<string, object> { ["UserId"] = 123 }
);
```

#### ExecuteTransactionAsync — Multiple Operations in One Transaction

```csharp
var commands = new List<(string Query, Dictionary<string, object>? Parameters)>
{
    ("INSERT INTO Orders (UserId, Total) VALUES (@UserId, @Total)",
     new Dictionary<string, object> { ["UserId"] = 123, ["Total"] = 450.00 }),
    
    ("UPDATE Products SET Stock = Stock - 1 WHERE Id = @ProductId",
     new Dictionary<string, object> { ["ProductId"] = 42 })
};

var result = await AdoHelper.ExecuteTransactionAsync(
    provider, connectionString, commands
);

if (!result.Succeeded)
{
    // All operations rolled back
    Console.WriteLine("Transaction failed, changes reverted");
}
```

### Multi-Result Set Queries

#### ExecuteQueryAsync2Sets<T1, T2> — Get Two Result Sets

```csharp
var queries = new List<string>
{
    "SELECT * FROM Users WHERE Id = @UserId",
    "SELECT * FROM Orders WHERE UserId = @UserId"
};

var result = await AdoHelper.ExecuteQueryAsync2Sets<User, Order>(
    provider, connectionString, queries,
    new Dictionary<string, object> { ["UserId"] = 123 }
);

if (result.Succeeded)
{
    var (users, orders) = result.Data;
    var user = users.FirstOrDefault();
    var userOrders = orders;
}
```

#### ExecuteQueryAsync3Sets<T1, T2, T3> — Get Three Result Sets

```csharp
var queries = new List<string>
{
    "SELECT * FROM Products WHERE CategoryId = @CatId",
    "SELECT * FROM Reviews WHERE ProductId IN (SELECT Id FROM Products WHERE CategoryId = @CatId)",
    "SELECT * FROM Categories WHERE Id = @CatId"
};

var result = await AdoHelper.ExecuteQueryAsync3Sets<Product, Review, Category>(
    provider, connectionString, queries,
    new Dictionary<string, object> { ["CatId"] = 5 }
);

if (result.Succeeded)
{
    var (products, reviews, categories) = result.Data;
}
```

---

## Dapper Helper

### DapperHelper — Lightweight ORM

`DapperHelper` wraps Dapper via reflection, so there's **no package dependency** in your csproj.

```csharp
using SmartWorkz.Core.Shared.Data;

var provider = DbProviderFactory.GetProvider("SqlServer");
var connectionString = "...";
```

### Methods

#### DapperQueryAsync<T> — Get Multiple Objects

```csharp
public static async Task<Result<List<T>>> DapperQueryAsync<T>(
    IDbProvider provider,
    string connectionString,
    string query,
    object? parameters = null,
    CommandType commandType = CommandType.Text
)
```

**Example:**

```csharp
var query = "SELECT * FROM Products WHERE Active = @Active";
var result = await DapperHelper.DapperQueryAsync<Product>(
    provider, connectionString, query,
    new { Active = true }
);

if (result.Succeeded)
{
    var activeProducts = result.Data;
}
```

#### DapperQuerySingleAsync<T> — Get Single Object

```csharp
var result = await DapperHelper.DapperQuerySingleAsync<Product>(
    provider, connectionString,
    "SELECT * FROM Products WHERE Id = @Id",
    new { Id = 42 }
);

if (result.Succeeded && result.Data != null)
{
    var product = result.Data;
}
```

#### DapperExecuteAsync — INSERT/UPDATE/DELETE

```csharp
var result = await DapperHelper.DapperExecuteAsync(
    provider, connectionString,
    "UPDATE Products SET Price = @Price WHERE Id = @Id",
    new { Price = 99.99, Id = 42 }
);

if (result.Succeeded)
{
    Console.WriteLine($"Updated {result.Data} rows");
}
```

---

## CSV Helper

### CsvHelper — RFC-4180 CSV Operations

Read and write CSV files with proper quoting, escaping, and column mapping.

```csharp
using SmartWorkz.Core.Shared.Data;
```

### CSV Writing

```csharp
var products = new List<Product>
{
    new Product { Id = 1, Name = "Widget", Price = 29.99 },
    new Product { Id = 2, Name = "Gadget", Price = 49.99 }
};

// Define column mapping
var mapping = CsvMapping<Product>.CreateAuto()
    .Column(p => p.Id, "ID")
    .Column(p => p.Name, "Product Name")
    .Column(p => p.Price, "Price (USD)");

// Write to CSV
var csvContent = new CsvHelper.CsvWriter<Product>(mapping)
    .Write(products);

// Save to file
File.WriteAllText("products.csv", csvContent);
// Output:
// ID,Product Name,Price (USD)
// 1,Widget,29.99
// 2,Gadget,49.99
```

### CSV Reading

```csharp
var csvContent = File.ReadAllText("products.csv");

var mapping = CsvMapping<Product>.CreateAuto()
    .Column(p => p.Id, "ID")
    .Column(p => p.Name, "Product Name")
    .Column(p => p.Price, "Price (USD)");

var options = new CsvOptions
{
    HasHeader = true,
    Delimiter = ",",
    QuoteChar = '"',
    TrimValues = true
};

var result = new CsvHelper.CsvReader<Product>(mapping, options)
    .Read(csvContent);

if (result.Succeeded)
{
    var products = result.Data;
    foreach (var product in products)
    {
        Console.WriteLine($"{product.Name}: ${product.Price}");
    }
}
```

### CsvOptions Configuration

```csharp
public class CsvOptions
{
    public string Delimiter { get; set; } = ",";
    public char QuoteChar { get; set; } = '"';
    public bool HasHeader { get; set; } = true;
    public bool TrimValues { get; set; } = true;
}
```

### Bulk Import Example

```csharp
public async Task<Result<int>> BulkImportProductsAsync(IFormFile csvFile)
{
    using var stream = csvFile.OpenReadStream();
    using var reader = new StreamReader(stream);
    var csvContent = await reader.ReadToEndAsync();
    
    var mapping = CsvMapping<Product>.CreateAuto()
        .Column(p => p.Name, "Name")
        .Column(p => p.Sku, "SKU")
        .Column(p => p.Price, "Price");
    
    var readResult = new CsvHelper.CsvReader<Product>(mapping).Read(csvContent);
    
    if (!readResult.Succeeded)
        return Result.Fail<int>(readResult.Error);
    
    var products = readResult.Data;
    var insertedCount = 0;
    
    foreach (var product in products)
    {
        var result = await _productRepository.AddAsync(product);
        if (result.Succeeded)
            insertedCount++;
    }
    
    return Result.Ok(insertedCount);
}
```

---

## XML Helper

### XmlHelper — XML Serialization & XPath

```csharp
using SmartWorkz.Core.Shared.Data;
```

### Serialize Object to XML

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

var product = new Product { Id = 1, Name = "Widget", Price = 29.99 };

var xmlResult = XmlHelper.Serialize(product);

if (xmlResult.Succeeded)
{
    var xml = xmlResult.Data;
    Console.WriteLine(xml);
    // Output:
    // <?xml version="1.0" encoding="utf-8"?>
    // <Product>
    //     <Id>1</Id>
    //     <Name>Widget</Name>
    //     <Price>29.99</Price>
    // </Product>
}
```

### Deserialize XML to Object

```csharp
var xml = @"<?xml version=""1.0""?>
<Product>
    <Id>1</Id>
    <Name>Widget</Name>
    <Price>29.99</Price>
</Product>";

var result = XmlHelper.Deserialize<Product>(xml);

if (result.Succeeded)
{
    var product = result.Data;
    Console.WriteLine($"{product.Name}: ${product.Price}");
}
```

### XPath Query

```csharp
var xml = @"<?xml version=""1.0""?>
<Products>
    <Product>
        <Name>Widget</Name>
        <Price>29.99</Price>
    </Product>
    <Product>
        <Name>Gadget</Name>
        <Price>49.99</Price>
    </Product>
</Products>";

// Query product names
var namesResult = XmlHelper.Query(xml, "//Product/Name/text()");

if (namesResult.Succeeded)
{
    var names = namesResult.Data; // ["Widget", "Gadget"]
}
```

### XmlOptions Configuration

```csharp
public class XmlOptions
{
    public string RootElement { get; set; } = "root";
    public bool IncludeXmlDeclaration { get; set; } = true;
    public bool Indent { get; set; } = true;
}
```

---

## Multi-Database Support

### DbProviderFactory — Register & Retrieve Providers

Pre-registered database providers:

```csharp
using SmartWorkz.Core.Shared.Data;

// Get provider by name
var sqlServer = DbProviderFactory.GetProvider("SqlServer");
var mysql = DbProviderFactory.GetProvider("MySql");
var postgres = DbProviderFactory.GetProvider("PostgreSql");
var sqlite = DbProviderFactory.GetProvider("Sqlite");
var oracle = DbProviderFactory.GetProvider("Oracle");
```

### DbProviderFactory — Type-Safe Enum Overload

**Phase 1:** Use `DatabaseProvider` enum for type-safe provider selection (recommended):

```csharp
// Type-safe enum (preferred)
var sqlServer = DbProviderFactory.GetProvider(DatabaseProvider.SqlServer);
var mysql = DbProviderFactory.GetProvider(DatabaseProvider.MySql);
var postgres = DbProviderFactory.GetProvider(DatabaseProvider.PostgreSql);
var sqlite = DbProviderFactory.GetProvider(DatabaseProvider.Sqlite);
var oracle = DbProviderFactory.GetProvider(DatabaseProvider.Oracle);
```

**Benefit:** Compile-time safety, IntelliSense support, no string typos.

### IDbProvider Interface

```csharp
public interface IDbProvider
{
    string ProviderName { get; }
    
    IDbConnection CreateConnection(string connectionString);
    string GetParameterPrefix();           // @ for SQL Server, : for Oracle
    string GetLastInsertIdSql();          // SCOPE_IDENTITY() vs LAST_INSERT_ID()
    string GetPaginationSql();            // OFFSET/FETCH vs LIMIT
    string FormatIdentifier(string name); // [Name] vs `Name`
    Task<bool> TestConnectionAsync(string connectionString);
}
```

### Register Custom Provider

```csharp
DbProviderFactory.Register("CustomDb", new CustomDbProvider());

var provider = DbProviderFactory.GetProvider("CustomDb");
```

### Get Provider from Connection String

```csharp
var connectionString = "Server=localhost;Database=MyDb;";
var provider = DbProviderFactory.GetProviderFromConnectionString(connectionString);
// Auto-detects SQL Server, MySQL, PostgreSQL based on server/keywords
```

---

## Decision Table: When to Use What

| Scenario | Best Choice | Why |
|----------|-------------|-----|
| Simple CRUD (Get, Insert, Update, Delete) | DapperHelper | Type-safe, minimal overhead |
| Complex object graphs with navigation | EF Core | Automatic relationship loading, change tracking |
| Raw SQL queries, stored procedures | AdoHelper | Full control, direct SQL execution |
| Reporting, aggregations, complex joins | AdoHelper or SQL views | Performance-optimized queries |
| Bulk imports/exports, data migration | CsvHelper | RFC-4180 compliant, simple mapping |
| Configuration files, message parsing | XmlHelper | Structured data, XPath queries |
| Multi-database compatibility | AdoHelper + DbProviderFactory | Provider abstraction layer |

---

## Complete Example: Multi-Database Import

```csharp
public class DataImportService
{
    private readonly IDbProvider _provider;
    private readonly string _connectionString;
    
    public DataImportService(string dbType, string connectionString)
    {
        _provider = DbProviderFactory.GetProvider(dbType);
        _connectionString = connectionString;
    }
    
    public async Task<Result<int>> ImportProductsFromCsvAsync(string csvPath)
    {
        try
        {
            // 1. Read CSV
            var csvContent = File.ReadAllText(csvPath);
            
            var mapping = CsvMapping<ProductImportDto>.CreateAuto()
                .Column(p => p.Name, "ProductName")
                .Column(p => p.Sku, "SKU")
                .Column(p => p.Price, "UnitPrice")
                .Column(p => p.Stock, "QuantityInStock");
            
            var readResult = new CsvHelper.CsvReader<ProductImportDto>(mapping)
                .Read(csvContent);
            
            if (!readResult.Succeeded)
                return Result.Fail<int>(readResult.Error);
            
            var products = readResult.Data;
            
            // 2. Insert into database using provider-specific SQL
            var insertedCount = 0;
            
            foreach (var product in products)
            {
                var query = @"
                    INSERT INTO Products (Name, Sku, Price, Stock, CreatedAt)
                    VALUES (@Name, @Sku, @Price, @Stock, @CreatedAt)
                ";
                
                var result = await AdoHelper.ExecuteNonQueryAsync(
                    _provider, _connectionString, query,
                    new Dictionary<string, object>
                    {
                        ["Name"] = product.Name,
                        ["Sku"] = product.Sku,
                        ["Price"] = product.Price,
                        ["Stock"] = product.Stock,
                        ["CreatedAt"] = DateTime.UtcNow
                    }
                );
                
                if (result.Succeeded)
                    insertedCount++;
            }
            
            return Result.Ok(insertedCount);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>(Error.FromException(ex));
        }
    }
}
```

---

## Performance Tips

### 1. Use Pagination for Large Result Sets

```csharp
var pageSize = 100;
var offset = (pageNumber - 1) * pageSize;

var query = $@"
    SELECT * FROM Products
    ORDER BY Id
    {_provider.GetPaginationSql()} {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY
";
```

### 2. Use Stored Procedures for Complex Logic

```csharp
var result = await AdoHelper.ExecuteStoredProcedureAsync<Report>(
    _provider, _connectionString,
    "sp_GenerateMonthlyReport",
    new Dictionary<string, object> { ["Month"] = 3, ["Year"] = 2024 }
);
```

### 3. Use Transactions for Multi-Step Operations

```csharp
var commands = new List<(string, Dictionary<string, object>?)>
{
    ("INSERT INTO Orders ...", orderParams),
    ("UPDATE Inventory SET Stock = Stock - 1 ...", inventoryParams),
    ("INSERT INTO OrderItems ...", itemsParams)
};

var result = await AdoHelper.ExecuteTransactionAsync(
    _provider, _connectionString, commands
);
```

### 4. Index Frequently Queried Columns

```sql
CREATE INDEX idx_products_categoryid ON Products(CategoryId);
CREATE INDEX idx_orders_userid ON Orders(UserId, CreatedAt);
```

---

## Troubleshooting

### "Connection string not found"

**Solution:** Ensure connection string is in configuration:

```csharp
var connectionString = _config.GetConnectionString("DefaultConnection");
```

### "Provider not registered"

**Solution:** Register provider before use:

```csharp
DbProviderFactory.Register("CustomDb", new CustomDbProvider());
```

### CSV parsing errors

**Solution:** Check CsvOptions and column mapping:

```csharp
var options = new CsvOptions 
{ 
    Delimiter = ";",  // Semicolon-delimited?
    HasHeader = true,
    TrimValues = true
};
```

---

## Simplified Data Access with DbExtensions

**Phase 1:** Short-form extension methods on `IDbConnection` for common data access patterns.

Instead of verbose method names, use convenient aliases:

### ADO.NET Aliases

```csharp
// Traditional verbose
var result = await AdoHelper.ExecuteScalarAsync<int>(
    connection, 
    "SELECT COUNT(*) FROM Users WHERE Active = 1",
    provider
);

// Simplified (Phase 1)
var result = await connection.ScalarAsync<int>(
    "SELECT COUNT(*) FROM Users WHERE Active = 1",
    provider
);
```

**Available ADO methods:**
- `QueryAsync<T>()` — instead of `ExecuteQueryAsync<T>()`
- `ScalarAsync<T>()` — instead of `ExecuteScalarAsync<T>()`
- `NonQueryAsync()` — instead of `ExecuteNonQueryAsync()`

### Dapper Aliases

```csharp
// Simplified aliases on IDbConnection
var users = await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE Active = 1"
);

var user = await connection.QuerySingleAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = userId }
);

var rowsAffected = await connection.ExecuteAsync(
    "UPDATE Users SET LastLogin = @Now WHERE Id = @Id",
    new { Now = DateTime.UtcNow, Id = userId }
);
```

**Available Dapper methods:**
- `QueryAsync<T>()` — query multiple rows
- `QuerySingleAsync<T>()` — query single row
- `ExecuteAsync()` — non-query (INSERT/UPDATE/DELETE)

### When to Use DbExtensions

✅ **Use DbExtensions (simplified aliases)** when:
- You just want quick, simple queries
- You're already using ADO or Dapper
- Code clarity is more important than explicitness

✅ **Use AdoHelper/DapperHelper directly** when:
- You need full control over parameters
- You're working with stored procedures
- You want explicit transaction management

**Both approaches coexist** — choose based on your use case.

---

## See Also

- [Result Pattern Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#result-pattern) — Error handling
- [Repository Pattern Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#repository-pattern) — Data access abstraction
- [Microsoft ADO.NET Docs](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/)
