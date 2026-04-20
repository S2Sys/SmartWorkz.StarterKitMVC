using SmartWorkz.Core.ValueObjects;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ECommerceDbContext db)
    {
        try
        {
            if (db.Categories.Any()) return;

            var electronics = new Category { Name = "Electronics", Slug = "electronics", Description = "Gadgets and devices" };
            var books = new Category { Name = "Books", Slug = "books", Description = "Technical and fiction books" };
            var clothing = new Category { Name = "Clothing", Slug = "clothing", Description = "Apparel and accessories" };
            db.Categories.AddRange(electronics, books, clothing);

            // Electronics - 15 products
            db.Products.AddRange(
                new Product { Name = "Laptop Pro 15", Slug = "laptop-pro-15", Description = "High-performance 15-inch laptop for developers with 32GB RAM", Price = Money.Create(1299.99m, "USD").Data!, Stock = 10, Category = electronics, IsActive = true },
                new Product { Name = "Laptop Pro 13", Slug = "laptop-pro-13", Description = "Compact 13-inch laptop for portability", Price = Money.Create(999.99m, "USD").Data!, Stock = 15, Category = electronics, IsActive = true },
                new Product { Name = "Wireless Mouse", Slug = "wireless-mouse", Description = "Ergonomic wireless mouse with precision tracking", Price = Money.Create(29.99m, "USD").Data!, Stock = 50, Category = electronics, IsActive = true },
                new Product { Name = "Gaming Mouse", Slug = "gaming-mouse", Description = "High-precision gaming mouse with RGB lighting", Price = Money.Create(59.99m, "USD").Data!, Stock = 35, Category = electronics, IsActive = true },
                new Product { Name = "Mechanical Keyboard", Slug = "mechanical-keyboard", Description = "TKL mechanical keyboard with cherry switches", Price = Money.Create(89.99m, "USD").Data!, Stock = 25, Category = electronics, IsActive = true },
                new Product { Name = "Wireless Keyboard", Slug = "wireless-keyboard", Description = "Slim wireless keyboard for office use", Price = Money.Create(45.99m, "USD").Data!, Stock = 40, Category = electronics, IsActive = true },
                new Product { Name = "USB-C Hub", Slug = "usb-c-hub", Description = "7-in-1 USB-C hub with multiple ports", Price = Money.Create(39.99m, "USD").Data!, Stock = 20, Category = electronics, IsActive = true },
                new Product { Name = "Monitor 27 inch", Slug = "monitor-27-inch", Description = "4K IPS monitor 27 inch 60Hz", Price = Money.Create(349.99m, "USD").Data!, Stock = 8, Category = electronics, IsActive = true },
                new Product { Name = "Monitor 24 inch", Slug = "monitor-24-inch", Description = "1080p Full HD monitor 24 inch", Price = Money.Create(199.99m, "USD").Data!, Stock = 12, Category = electronics, IsActive = true },
                new Product { Name = "Webcam 4K", Slug = "webcam-4k", Description = "Ultra HD 4K webcam for streaming", Price = Money.Create(129.99m, "USD").Data!, Stock = 18, Category = electronics, IsActive = true },
                new Product { Name = "Headphones Wireless", Slug = "headphones-wireless", Description = "Premium wireless headphones with noise cancellation", Price = Money.Create(199.99m, "USD").Data!, Stock = 30, Category = electronics, IsActive = true },
                new Product { Name = "Gaming Headset", Slug = "gaming-headset", Description = "Gaming headset with surround sound", Price = Money.Create(149.99m, "USD").Data!, Stock = 25, Category = electronics, IsActive = true },
                new Product { Name = "SSD 1TB", Slug = "ssd-1tb", Description = "NVMe SSD 1TB for ultra-fast storage", Price = Money.Create(99.99m, "USD").Data!, Stock = 45, Category = electronics, IsActive = true },
                new Product { Name = "USB Drive 32GB", Slug = "usb-drive-32gb", Description = "Fast USB 3.1 drive 32GB", Price = Money.Create(19.99m, "USD").Data!, Stock = 60, Category = electronics, IsActive = true },
                new Product { Name = "Portable Charger", Slug = "portable-charger", Description = "20000mAh portable power bank", Price = Money.Create(34.99m, "USD").Data!, Stock = 50, Category = electronics, IsActive = true }
            );

            // Books - 15 products
            db.Products.AddRange(
                new Product { Name = "Clean Code", Slug = "clean-code", Description = "Robert C. Martin's classic on writing maintainable code", Price = Money.Create(39.99m, "USD").Data!, Stock = 30, Category = books, IsActive = true },
                new Product { Name = "Domain-Driven Design", Slug = "domain-driven-design", Description = "Eric Evans comprehensive guide to DDD", Price = Money.Create(49.99m, "USD").Data!, Stock = 20, Category = books, IsActive = true },
                new Product { Name = "C# in Depth", Slug = "csharp-in-depth", Description = "Jon Skeet's comprehensive guide to C#", Price = Money.Create(44.99m, "USD").Data!, Stock = 15, Category = books, IsActive = true },
                new Product { Name = "Design Patterns", Slug = "design-patterns", Description = "Gang of Four design patterns reference", Price = Money.Create(54.99m, "USD").Data!, Stock = 18, Category = books, IsActive = true },
                new Product { Name = "Refactoring", Slug = "refactoring", Description = "Martin Fowler's guide to improving code", Price = Money.Create(49.99m, "USD").Data!, Stock = 22, Category = books, IsActive = true },
                new Product { Name = "Code Complete", Slug = "code-complete", Description = "Steve McConnell's comprehensive software construction guide", Price = Money.Create(59.99m, "USD").Data!, Stock = 14, Category = books, IsActive = true },
                new Product { Name = "The Pragmatic Programmer", Slug = "pragmatic-programmer", Description = "Essential software development practices", Price = Money.Create(49.99m, "USD").Data!, Stock = 25, Category = books, IsActive = true },
                new Product { Name = "Microservices Patterns", Slug = "microservices-patterns", Description = "Patterns and best practices for microservices", Price = Money.Create(59.99m, "USD").Data!, Stock = 12, Category = books, IsActive = true },
                new Product { Name = "Building Microservices", Slug = "building-microservices", Description = "Sam Newman's guide to microservices architecture", Price = Money.Create(54.99m, "USD").Data!, Stock = 16, Category = books, IsActive = true },
                new Product { Name = "Kubernetes in Action", Slug = "kubernetes-in-action", Description = "Complete guide to Kubernetes orchestration", Price = Money.Create(59.99m, "USD").Data!, Stock = 11, Category = books, IsActive = true },
                new Product { Name = "Docker Deep Dive", Slug = "docker-deep-dive", Description = "Nigel Poulton's comprehensive Docker guide", Price = Money.Create(44.99m, "USD").Data!, Stock = 19, Category = books, IsActive = true },
                new Product { Name = "Git in Depth", Slug = "git-in-depth", Description = "Master Git version control", Price = Money.Create(39.99m, "USD").Data!, Stock = 28, Category = books, IsActive = true },
                new Product { Name = "SQL Performance Explained", Slug = "sql-performance", Description = "Database query optimization techniques", Price = Money.Create(49.99m, "USD").Data!, Stock = 13, Category = books, IsActive = true },
                new Product { Name = "NoSQL Distilled", Slug = "nosql-distilled", Description = "Understanding NoSQL databases", Price = Money.Create(44.99m, "USD").Data!, Stock = 17, Category = books, IsActive = true },
                new Product { Name = "API Design Patterns", Slug = "api-design-patterns", Description = "Best practices for RESTful APIs", Price = Money.Create(54.99m, "USD").Data!, Stock = 10, Category = books, IsActive = true }
            );

            // Clothing - 15 products
            db.Products.AddRange(
                new Product { Name = "Dev Hoodie", Slug = "dev-hoodie", Description = "Comfortable developer hoodie in dark gray", Price = Money.Create(49.99m, "USD").Data!, Stock = 40, Category = clothing, IsActive = true },
                new Product { Name = "Tech T-Shirt", Slug = "tech-tshirt", Description = "100% cotton premium tech tee", Price = Money.Create(24.99m, "USD").Data!, Stock = 60, Category = clothing, IsActive = true },
                new Product { Name = "Laptop Backpack", Slug = "laptop-backpack", Description = "Water-resistant 30L backpack with laptop compartment", Price = Money.Create(79.99m, "USD").Data!, Stock = 35, Category = clothing, IsActive = true },
                new Product { Name = "Developer Polo", Slug = "developer-polo", Description = "Professional polo shirt for developers", Price = Money.Create(39.99m, "USD").Data!, Stock = 30, Category = clothing, IsActive = true },
                new Product { Name = "Cargo Pants", Slug = "cargo-pants", Description = "Comfortable cargo pants with multiple pockets", Price = Money.Create(59.99m, "USD").Data!, Stock = 25, Category = clothing, IsActive = true },
                new Product { Name = "Jeans Slim Fit", Slug = "jeans-slim-fit", Description = "Durable slim fit jeans", Price = Money.Create(69.99m, "USD").Data!, Stock = 40, Category = clothing, IsActive = true },
                new Product { Name = "Windbreaker Jacket", Slug = "windbreaker-jacket", Description = "Lightweight windbreaker jacket", Price = Money.Create(89.99m, "USD").Data!, Stock = 20, Category = clothing, IsActive = true },
                new Product { Name = "Winter Coat", Slug = "winter-coat", Description = "Warm winter coat with insulation", Price = Money.Create(149.99m, "USD").Data!, Stock = 15, Category = clothing, IsActive = true },
                new Product { Name = "Socks Bundle", Slug = "socks-bundle", Description = "Pack of 5 developer socks", Price = Money.Create(19.99m, "USD").Data!, Stock = 80, Category = clothing, IsActive = true },
                new Product { Name = "Cap Baseball", Slug = "cap-baseball", Description = "Classic baseball cap", Price = Money.Create(29.99m, "USD").Data!, Stock = 45, Category = clothing, IsActive = true },
                new Product { Name = "Beanie Winter", Slug = "beanie-winter", Description = "Warm winter beanie", Price = Money.Create(24.99m, "USD").Data!, Stock = 50, Category = clothing, IsActive = true },
                new Product { Name = "Sneakers Running", Slug = "sneakers-running", Description = "Comfortable running sneakers", Price = Money.Create(99.99m, "USD").Data!, Stock = 30, Category = clothing, IsActive = true },
                new Product { Name = "Shoes Casual", Slug = "shoes-casual", Description = "Casual everyday shoes", Price = Money.Create(79.99m, "USD").Data!, Stock = 35, Category = clothing, IsActive = true },
                new Product { Name = "Belt Leather", Slug = "belt-leather", Description = "Premium leather belt", Price = Money.Create(44.99m, "USD").Data!, Stock = 40, Category = clothing, IsActive = true },
                new Product { Name = "Scarf Wool", Slug = "scarf-wool", Description = "Warm wool scarf", Price = Money.Create(34.99m, "USD").Data!, Stock = 25, Category = clothing, IsActive = true }
            );

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to seed database", ex);
        }
    }
}
