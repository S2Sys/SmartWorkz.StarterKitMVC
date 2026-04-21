using Microsoft.EntityFrameworkCore;
using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.Mapping;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Application.Validators;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using SmartWorkz.Sample.ECommerce.Infrastructure.Repositories;

namespace SmartWorkz.Sample.ECommerce;

public static class ECommerceServiceExtensions
{
    public static IServiceCollection AddECommerceServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ECommerceDbContext>(opt =>
            opt.UseSqlite(config.GetConnectionString("Default") ?? "Data Source=ecommerce.db"));

        // Repositories
        services.AddScoped<IRepository<Product, int>, ProductRepository>();
        services.AddScoped<IRepository<Category, int>, CategoryRepository>();
        services.AddScoped<IRepository<Customer, int>, CustomerRepository>();
        services.AddScoped<IRepository<Order, int>, OrderRepository>();

        // Core services - JWT settings and auth helpers
        var jwtSettings = new JwtSettings
        {
            Secret = config["Jwt:Secret"] ?? "ecommerce-dev-secret-key-32chars!!",
            Issuer = config["Jwt:Issuer"] ?? "ecommerce",
            Audience = config["Jwt:Audience"] ?? "ecommerce",
            ExpiryMinutes = int.TryParse(config["Jwt:ExpiryMinutes"], out var expMin) ? expMin : 60
        };
        services.AddSingleton(jwtSettings);
        services.AddSingleton<InMemoryEventPublisher>();

        // Mapper registration with all profiles
        services.AddSingleton<IMapper>(sp => {
            var mapper = new SimpleMapper();
            mapper.RegisterProfile<Category, CategoryDto>(new CategoryToCategoryDtoProfile());
            mapper.RegisterProfile<Product, ProductDto>(new ProductToProductDtoProfile());
            mapper.RegisterProfile<Customer, CustomerDto>(new CustomerToCustomerDtoProfile());
            mapper.RegisterProfile<OrderItem, OrderItemDto>(new OrderItemToOrderItemDtoProfile());
            mapper.RegisterProfile<Order, OrderDto>(new OrderToOrderDtoProfile());
            return mapper;
        });

        // Application services
        services.AddScoped<ProductService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<OrderService>();
        services.AddScoped<ECommerceAuthService>();
        services.AddScoped<CartService>();
        services.AddScoped<CatalogSearchService>();

        // Validators
        services.AddSingleton<RegisterValidator>();
        services.AddSingleton<CheckoutValidator>();
        services.AddSingleton<OrderValidator>();

        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(opt => { opt.IdleTimeout = TimeSpan.FromMinutes(30); opt.Cookie.HttpOnly = true; });

        return services;
    }
}
