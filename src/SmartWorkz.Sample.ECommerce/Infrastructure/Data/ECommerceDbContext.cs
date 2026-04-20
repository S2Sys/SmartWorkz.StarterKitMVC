using Microsoft.EntityFrameworkCore;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Data;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore value objects (complex due to private constructors)
        modelBuilder.Entity<Product>().Ignore(p => p.Price);
        modelBuilder.Entity<OrderItem>().Ignore(i => i.UnitPrice);
        modelBuilder.Entity<Order>().Ignore(o => o.ShippingAddress);
        modelBuilder.Entity<Order>().Ignore(o => o.Total);

        // Ignore complex properties
        modelBuilder.Entity<Customer>().Ignore(c => c.Email);

        // Ignore domain events collection (not persisted)
        modelBuilder.Entity<Order>().Ignore(o => o.DomainEvents);

        // Indexes
        modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
    }
}
