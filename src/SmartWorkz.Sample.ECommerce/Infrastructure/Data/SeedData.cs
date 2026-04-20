using SmartWorkz.Core.ValueObjects;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ECommerceDbContext db)
    {
        if (db.Categories.Any()) return;

        var electronics = new Category { Name = "Electronics", Slug = "electronics", Description = "Gadgets and devices" };
        var books = new Category { Name = "Books", Slug = "books", Description = "Technical and fiction books" };
        var clothing = new Category { Name = "Clothing", Slug = "clothing", Description = "Apparel and accessories" };
        db.Categories.AddRange(electronics, books, clothing);

        db.Products.AddRange(
            new Product { Name = "Laptop Pro 15", Slug = "laptop-pro-15", Description = "High-performance laptop for developers", Price = Money.Create(1299.99m, "USD").Data, Stock = 10, Category = electronics },
            new Product { Name = "Wireless Mouse", Slug = "wireless-mouse", Description = "Ergonomic wireless mouse", Price = Money.Create(29.99m, "USD").Data, Stock = 50, Category = electronics },
            new Product { Name = "Mechanical Keyboard", Slug = "mechanical-keyboard", Description = "TKL mechanical keyboard", Price = Money.Create(89.99m, "USD").Data, Stock = 25, Category = electronics },
            new Product { Name = "Clean Code", Slug = "clean-code", Description = "Robert C. Martin's classic", Price = Money.Create(39.99m, "USD").Data, Stock = 30, Category = books },
            new Product { Name = "Domain-Driven Design", Slug = "domain-driven-design", Description = "Eric Evans blue book", Price = Money.Create(49.99m, "USD").Data, Stock = 20, Category = books },
            new Product { Name = "C# in Depth", Slug = "csharp-in-depth", Description = "Jon Skeet's comprehensive guide", Price = Money.Create(44.99m, "USD").Data, Stock = 15, Category = books },
            new Product { Name = "Dev Hoodie", Slug = "dev-hoodie", Description = "Comfortable developer hoodie", Price = Money.Create(49.99m, "USD").Data, Stock = 40, Category = clothing },
            new Product { Name = "Tech T-Shirt", Slug = "tech-tshirt", Description = "100% cotton tech tee", Price = Money.Create(24.99m, "USD").Data, Stock = 60, Category = clothing },
            new Product { Name = "Laptop Backpack", Slug = "laptop-backpack", Description = "Water-resistant 30L backpack", Price = Money.Create(79.99m, "USD").Data, Stock = 35, Category = clothing }
        );

        await db.SaveChangesAsync();
    }
}
