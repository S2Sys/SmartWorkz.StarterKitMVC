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

        // Owned types
        modelBuilder.Entity<Product>().OwnsOne(p => p.Price);
        modelBuilder.Entity<OrderItem>().OwnsOne(i => i.UnitPrice);
        modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress);

        // Customer EmailAddress owned
        modelBuilder.Entity<Customer>().OwnsOne(c => c.Email, b => {
            b.Property(e => e.Value).HasColumnName("Email");
        });

        // Ignore domain events collection (not persisted)
        modelBuilder.Entity<Order>().Ignore(o => o.DomainEvents);

        // Indexes
        modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex("Email_Value").IsUnique();
    }
}
