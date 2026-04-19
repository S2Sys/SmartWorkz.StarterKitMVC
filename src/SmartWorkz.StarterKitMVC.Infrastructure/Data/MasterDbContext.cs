using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    // Master Schema Entities
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Domain.Entities.Master.TimeZone> TimeZones { get; set; }
    public DbSet<Domain.Entities.Master.Configuration> Configurations { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<GeoHierarchy> GeoHierarchies { get; set; }
    public DbSet<GeolocationPage> GeolocationPages { get; set; }
    public DbSet<CustomPage> CustomPages { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("Master");

        // Tenant Configuration
        modelBuilder.Entity<Tenant>()
            .HasKey(t => t.TenantId);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Countries)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Currencies)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Languages)
            .WithOne(l => l.Tenant)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.TimeZones)
            .WithOne(tz => tz.Tenant)
            .HasForeignKey(tz => tz.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Configurations)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.FeatureFlags)
            .WithOne(f => f.Tenant)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Menus)
            .WithOne(m => m.Tenant)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Categories)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Products)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.GeoHierarchies)
            .WithOne(g => g.Tenant)
            .HasForeignKey(g => g.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.GeolocationPages)
            .WithOne(gp => gp.Tenant)
            .HasForeignKey(gp => gp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.CustomPages)
            .WithOne(cp => cp.Tenant)
            .HasForeignKey(cp => cp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.BlogPosts)
            .WithOne(bp => bp.Tenant)
            .HasForeignKey(bp => bp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Customers)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Suppliers)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuration Key Mapping
        modelBuilder.Entity<Domain.Entities.Master.Configuration>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Domain.Entities.Master.Configuration>()
            .Property(c => c.Id).HasColumnName("ConfigId");

        // Country Key Mapping
        modelBuilder.Entity<Country>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Country>()
            .Property(c => c.Id).HasColumnName("CountryId");

        // Currency Key Mapping
        modelBuilder.Entity<Currency>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Currency>()
            .Property(c => c.Id).HasColumnName("CurrencyId");

        // Language Key Mapping
        modelBuilder.Entity<Language>()
            .HasKey(l => l.Id);
        modelBuilder.Entity<Language>()
            .Property(l => l.Id).HasColumnName("LanguageId");

        // TimeZone Key Mapping
        modelBuilder.Entity<Domain.Entities.Master.TimeZone>()
            .HasKey(tz => tz.Id);
        modelBuilder.Entity<Domain.Entities.Master.TimeZone>()
            .Property(tz => tz.Id).HasColumnName("TimeZoneId");

        // Configuration unique constraints
        modelBuilder.Entity<Domain.Entities.Master.Configuration>()
            .HasIndex(c => new { c.TenantId, c.Key })
            .IsUnique();

        modelBuilder.Entity<Country>()
            .HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique();

        modelBuilder.Entity<Currency>()
            .HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique();

        modelBuilder.Entity<Language>()
            .HasIndex(l => new { l.TenantId, l.Code })
            .IsUnique();

        modelBuilder.Entity<Domain.Entities.Master.TimeZone>()
            .HasIndex(tz => new { tz.TenantId, tz.Identifier })
            .IsUnique();

        // FeatureFlag Key Mapping
        modelBuilder.Entity<FeatureFlag>()
            .HasKey(f => f.Id);
        modelBuilder.Entity<FeatureFlag>()
            .Property(f => f.Id).HasColumnName("FeatureFlagId");

        // Menu Key Mapping
        modelBuilder.Entity<Menu>()
            .HasKey(m => m.Id);
        modelBuilder.Entity<Menu>()
            .Property(m => m.Id).HasColumnName("MenuId");

        // MenuItem Key Mapping
        modelBuilder.Entity<MenuItem>()
            .HasKey(mi => mi.Id);
        modelBuilder.Entity<MenuItem>()
            .Property(mi => mi.Id).HasColumnName("MenuItemId");

        // Category Key Mapping
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Category>()
            .Property(c => c.Id).HasColumnName("CategoryId");

        // Product Key Mapping
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Product>()
            .Property(p => p.Id).HasColumnName("ProductId");

        // GeoHierarchy Key Mapping
        modelBuilder.Entity<GeoHierarchy>()
            .HasKey(g => g.Id);
        modelBuilder.Entity<GeoHierarchy>()
            .Property(g => g.Id).HasColumnName("GeoId");

        // GeolocationPage Key Mapping
        modelBuilder.Entity<GeolocationPage>()
            .HasKey(gp => gp.Id);
        modelBuilder.Entity<GeolocationPage>()
            .Property(gp => gp.Id).HasColumnName("GeoPageId");

        // CustomPage Key Mapping
        modelBuilder.Entity<CustomPage>()
            .HasKey(cp => cp.Id);
        modelBuilder.Entity<CustomPage>()
            .Property(cp => cp.Id).HasColumnName("PageId");

        // BlogPost Key Mapping
        modelBuilder.Entity<BlogPost>()
            .HasKey(bp => bp.Id);
        modelBuilder.Entity<BlogPost>()
            .Property(bp => bp.Id).HasColumnName("PostId");

        // Customer Key Mapping
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Customer>()
            .Property(c => c.Id).HasColumnName("CustomerId");

        // Supplier Key Mapping
        modelBuilder.Entity<Supplier>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Supplier>()
            .Property(s => s.Id).HasColumnName("SupplierId");

        // Inventory Key Mapping
        modelBuilder.Entity<Inventory>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Inventory>()
            .Property(i => i.Id).HasColumnName("InventoryId");

        // Category self-referencing relationship
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.TenantId, c.Slug })
            .IsUnique();

        // Product configuration
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.SKU })
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.Slug })
            .IsUnique();

        // GeoHierarchy self-referencing
        modelBuilder.Entity<GeoHierarchy>()
            .HasOne(g => g.ParentGeo)
            .WithMany(g => g.ChildGeos)
            .HasForeignKey(g => g.ParentGeoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Menu and MenuItem relationship
        modelBuilder.Entity<Menu>()
            .HasMany(m => m.MenuItems)
            .WithOne(mi => mi.Menu)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
            .HasIndex(mi => new { mi.TenantId, mi.MenuId, mi.DisplayOrder });

        // CustomPage unique constraint
        modelBuilder.Entity<CustomPage>()
            .HasIndex(cp => new { cp.TenantId, cp.Slug })
            .IsUnique();

        // BlogPost unique constraint
        modelBuilder.Entity<BlogPost>()
            .HasIndex(bp => new { bp.TenantId, bp.Slug })
            .IsUnique();

        // Customer unique constraint
        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique();

        // Supplier configuration
        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Inventories)
            .WithOne(i => i.Supplier)
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inventory configuration
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Product)
            .WithMany(p => p.Inventories)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inventory>()
            .HasIndex(i => new { i.TenantId, i.ProductId })
            .IsUnique();
    }
}
