using AutoMapper;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Text;
using SmartWorkz.Core;
using SmartWorkz.Shared;
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
        services.AddSingleton<InMemoryEventSubscriber>();
        services.AddSingleton<InMemoryEventPublisher>();

        // Mapper registration with all profiles
        services.AddSingleton<SmartWorkz.Shared.IMapper>(sp => {
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
        services.AddSingleton<CreateProductValidator>();

        // IDbConnection for CatalogSearchService (Dapper-based searches)
        services.AddScoped<IDbConnection>(_ =>
            new SqliteConnection(config.GetConnectionString("Default") ?? "Data Source=ecommerce.db"));

        // Authentication schemes
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";  // MVC controllers use cookie auth
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  // API uses Bearer
        })
        .AddCookie("Cookies", options =>
        {
            options.LoginPath = "/Account/Login";
            options.Cookie.Name = "ecommerce_auth";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.MapInboundClaims = false;  // Keep "sub" claim name, don't remap to ClaimTypes.NameIdentifier
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "sub",      // JWT "sub" maps to User.Identity.Name
                RoleClaimType = "roles"     // JWT "roles" are used for [Authorize(Roles = "...")]
            };
        });

        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(opt => { opt.IdleTimeout = TimeSpan.FromMinutes(30); opt.Cookie.HttpOnly = true; });

        return services;
    }
}

