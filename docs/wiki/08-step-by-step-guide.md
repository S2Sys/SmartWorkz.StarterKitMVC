# SmartWorkz Zero-to-Hero Implementation Guide

Build a complete CRUD feature from scratch using SmartWorkz patterns.

---

## Step 1: Create a New Razor Pages Project

Create a new ASP.NET Core Razor Pages application with SmartWorkz references.

```bash
cd C:\projects
dotnet new webapp -n MySmartWorkzApp
cd MySmartWorkzApp
```

Open `MySmartWorkzApp.csproj` and replace with:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
    <PackageReference Include="Serilog" Version="4.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="ClosedXML" Version="0.101.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\SmartWorkz.Core\SmartWorkz.Core.csproj" />
    <ProjectReference Include="..\..\SmartWorkz.Core.Web\SmartWorkz.Core.Web.csproj" />
    <ProjectReference Include="..\..\SmartWorkz.Core.Shared\Caching\SmartWorkz.Core.Shared.Caching.csproj" />
    <ProjectReference Include="..\..\SmartWorkz.Core.External\SmartWorkz.Core.External.csproj" />
  </ItemGroup>
</Project>
```

---

## Step 2: Configure appsettings.json

Replace `appsettings.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MySmartWorkzAppDb;Trusted_Connection=true;"
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "fileSizeLimitBytes": 10485760
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

---

## Step 3: Create ApplicationDbContext

Create `Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SmartWorkz.Core;

namespace MySmartWorkzApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
        }
    }

    public class Product : AuditEntity<int>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}
```

---

## Step 4: Wire Up Program.cs

Replace `Program.cs` with:

```csharp
using Serilog;
using Microsoft.EntityFrameworkCore;
using SmartWorkz.Core.Shared.Logging;
using SmartWorkz.Core.Shared.Caching;
using MySmartWorkzApp.Data;

var builder = WebApplicationBuilder.CreateBuilder(args);

// 1. Add Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services
builder.Services.AddRazorPages();
builder.Services.AddScoped<IQueryCacheService, QueryCacheService>();
builder.Services.AddMemoryCache();

// 3. Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Add logging
builder.Services.AddLogging();

var app = builder.Build();

// 5. Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// 6. Run migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
```

---

## Step 5: Create Initial Migration

Run Entity Framework migrations:

```bash
dotnet ef migrations add InitialCreate --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

Verify: Check your LocalDB instance. Table `Products` should exist with columns: Id, Name, Price, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy.

---

## Step 6: Create First Page — Product Details

Create `Pages/Products/Detail.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySmartWorkzApp.Data;
using SmartWorkz.Core;

namespace MySmartWorkzApp.Pages.Products
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(ApplicationDbContext context, ILogger<DetailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            _logger.LogInformation("Fetching product {ProductId}", id);
            
            Product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            
            if (Product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return NotFound();
            }

            return Page();
        }
    }
}
```

Create `Pages/Products/Detail.cshtml`:

```html
@page "/products/{id:int}"
@model DetailModel

<div class="container mt-4">
    <h1>@Model.Product.Name</h1>
    <p><strong>Price:</strong> $@Model.Product.Price.ToString("0.00")</p>
    <p><strong>Description:</strong> @Model.Product.Description</p>
    <p><strong>Created:</strong> @Model.Product.CreatedAt.ToString("yyyy-MM-dd HH:mm")</p>
    <p><strong>Created By:</strong> @Model.Product.CreatedBy</p>
    <a href="/products/list" class="btn btn-secondary">Back to List</a>
</div>
```

---

## Step 7: Create List Page — All Products

Create `Pages/Products/List.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySmartWorkzApp.Data;

namespace MySmartWorkzApp.Pages.Products
{
    public class ListModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ListModel> _logger;

        public ListModel(ApplicationDbContext context, ILogger<ListModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Product> Products { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Loading all products");
            Products = await _context.Products.OrderByDescending(p => p.CreatedAt).ToListAsync();
            _logger.LogInformation("Loaded {Count} products", Products.Count());
        }
    }
}
```

Create `Pages/Products/List.cshtml`:

```html
@page "/products/list"
@model ListModel

<div class="container mt-4">
    <h1>Products</h1>
    <a href="/products/create" class="btn btn-primary mb-3">Add Product</a>

    <table class="table table-striped">
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Price</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var product in Model.Products)
            {
                <tr>
                    <td>@product.Id</td>
                    <td>@product.Name</td>
                    <td>$@product.Price.ToString("0.00")</td>
                    <td>
                        <a href="/products/@product.Id" class="btn btn-sm btn-info">View</a>
                        <a href="/products/@product.Id/edit" class="btn btn-sm btn-warning">Edit</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

---

## Step 8: Add Excel Export

Add to `List.cshtml.cs`:

```csharp
using SmartWorkz.Core.External;

public class ListModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<ListModel> _logger;

    public ListModel(
        ApplicationDbContext context,
        IExcelExporter excelExporter,
        ILogger<ListModel> logger)
    {
        _context = context;
        _excelExporter = excelExporter;
        _logger = logger;
    }

    public IEnumerable<Product> Products { get; set; }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Loading all products");
        Products = await _context.Products.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnGetExportAsync()
    {
        var products = await _context.Products.ToListAsync();
        
        var result = await _excelExporter.ExportAsync(
            products,
            "Products",
            HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        return File(
            result.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"products-{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
```

Add to `List.cshtml` (below the list table):

```html
<a href="/products/list?handler=Export" class="btn btn-success mt-3">Export to Excel</a>
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IExcelExporter, ExcelExporter>();
```

---

## Step 9: Add CQRS Query

Create `Queries/GetAllProductsQuery.cs`:

```csharp
using SmartWorkz.Core.Shared.CQRS;

namespace MySmartWorkzApp.Queries
{
    public class GetAllProductsQuery : IQuery<IEnumerable<ProductDto>>
    {
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

Create `Queries/GetAllProductsQueryHandler.cs`:

```csharp
using SmartWorkz.Core.Shared.CQRS;
using MySmartWorkzApp.Data;
using MySmartWorkzApp.Queries;

namespace MySmartWorkzApp.Handlers
{
    public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;

        public GetAllProductsQueryHandler(ApplicationDbContext context, ILogger<GetAllProductsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllProductsQuery");
            
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Query returned {Count} products", products.Count);
            return products;
        }
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>>, GetAllProductsQueryHandler>();
```

Update `List.cshtml.cs` to use the query:

```csharp
private readonly IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>> _queryHandler;

public ListModel(
    ApplicationDbContext context,
    IExcelExporter excelExporter,
    IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>> queryHandler,
    ILogger<ListModel> logger)
{
    _context = context;
    _excelExporter = excelExporter;
    _queryHandler = queryHandler;
    _logger = logger;
}

public async Task OnGetAsync()
{
    _logger.LogInformation("Loading all products via CQRS query");
    var dtos = await _queryHandler.HandleAsync(new GetAllProductsQuery(), CancellationToken.None);
    Products = await _context.Products.ToListAsync();  // Still fetch for UI binding
}
```

---

## Verification Checklist

Run each command and verify output:

```bash
# 1. Build
dotnet build
# Expected: Build succeeded. 0 Warning(s)

# 2. Run
dotnet run
# Expected: Application started. Press CTRL+C to shut down.

# 3. Test endpoints
# Browser: https://localhost:5001/products/list
# Expected: Empty table (no products yet), "Add Product" button visible

# 4. Create test product (via database seeding or manual INSERT)
INSERT INTO Products (Name, Price, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
VALUES ('Test Product', 99.99, 'A test product', GETDATE(), 'admin', GETDATE(), 'admin');

# 5. Reload /products/list
# Expected: Table shows "Test Product" with price "$99.99"

# 6. Test detail page
# Browser: https://localhost:5001/products/1
# Expected: Shows product details with timestamps

# 7. Test Excel export
# Click "Export to Excel" button
# Expected: Downloads products-YYYYMMDD.xlsx file

# 8. Verify logs
# Check logs/ folder
# Expected: logs/app-YYYY-MM-DD.txt file exists with entries like "[12:34:56 inf] Loading all products"
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Project references not found" | Verify relative paths in .csproj match your directory structure |
| "Database migration failed" | Check SQL Server/LocalDB is running: `sqllocaldb info` |
| "Port 5001 already in use" | Change port in `launchSettings.json` or stop conflicting process |
| "Serilog not working" | Verify Serilog packages are installed: `dotnet list package` |
| "CQRS handler not found" | Check DI registration in Program.cs matches handler namespace |
| "Excel export returns error" | Ensure ClosedXML package is installed and Products table has data |

---

**Next:** Explore the [Translation System](09-translation-system.md) for multi-language support, or the [Cache Attribute Pattern](20-cache-attribute.md) for response caching.
