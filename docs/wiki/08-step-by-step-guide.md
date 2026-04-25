# Zero-to-Hero Step-by-Step Guide

Build a complete CRUD feature using SmartWorkz patterns. Each step includes exact code.

---

## Step 1: Create New Razor Pages Project

```bash
cd C:\projects
dotnet new webapp -n MySmartWorkzApp
cd MySmartWorkzApp
```

Update `MySmartWorkzApp.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
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
          "retainedFileCountLimit": 7
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

```csharp
using Serilog;
using Microsoft.EntityFrameworkCore;
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

## Step 5: Create First Page

Create `Pages/Products/Detail.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySmartWorkzApp.Data;

namespace MySmartWorkzApp.Pages.Products
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailModel> _logger;

        public Product Product { get; set; }

        public DetailModel(ApplicationDbContext context, ILogger<DetailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

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
    <a href="/products/list" class="btn btn-secondary">Back to List</a>
</div>
```

---

## Step 6: Create List Page

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

        public IEnumerable<Product> Products { get; set; }

        public ListModel(ApplicationDbContext context, ILogger<ListModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Loading all products");
            Products = await _context.Products.OrderByDescending(p => p.CreatedAt).ToListAsync();
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
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

---

## Step 7: Add Excel Export

Add to `List.cshtml.cs`:

```csharp
using SmartWorkz.Core.External;

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
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IExcelExporter, ExcelExporter>();
```

---

## Step 8: Add CQRS Query

Create `Queries/GetAllProductsQuery.cs`:

```csharp
using SmartWorkz.Core.Shared.CQRS;

public class GetAllProductsQuery : IQuery<IEnumerable<ProductDto>> { }

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

Create `Handlers/GetAllProductsQueryHandler.cs`:

```csharp
using SmartWorkz.Core.Shared.CQRS;

public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> HandleAsync(GetAllProductsQuery query, CancellationToken ct)
    {
        return await _context.Products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price })
            .ToListAsync(ct);
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IQueryHandler<GetAllProductsQuery, IEnumerable<ProductDto>>, GetAllProductsQueryHandler>();
```

---

## Step 9: Run and Verify

```bash
# 1. Build
dotnet build

# 2. Run
dotnet run

# 3. Open browser
https://localhost:5001/products/list

# 4. Create test product in database
INSERT INTO Products (Name, Price, Description, CreatedAt, CreatedBy)
VALUES ('Test Product', 99.99, 'A test', GETDATE(), 'admin');

# 5. Reload page - should show product

# 6. Test export - click export button
```

**Troubleshooting:**

**"Database connection failed"** → Verify SQL Server/LocalDB is running  
**"Port already in use"** → Change port in `launchSettings.json`  
**"Migration failed"** → Delete database and re-run migrations  
**"Serilog not working"** → Check logs/ folder permissions  

---

You now have a complete working application with products list, detail page, Excel export, and CQRS query! 🎉

**Next:** Explore Pattern Library (files 09-27) for advanced features.
