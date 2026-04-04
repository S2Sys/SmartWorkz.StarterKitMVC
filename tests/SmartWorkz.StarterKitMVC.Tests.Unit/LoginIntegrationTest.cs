using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Models;
using Xunit;

namespace SmartWorkz.StarterKitMVC.Tests.Unit;

public class LoginIntegrationTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;

    public LoginIntegrationTest()
    {
        // Build configuration from appsettings.json
        var basePath = Path.GetDirectoryName(typeof(LoginIntegrationTest).Assembly.Location) ?? Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Build service collection with infrastructure services
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructureServices(configuration);
        services.AddRepositories();
        services.AddApplicationServices();

        _serviceProvider = services.BuildServiceProvider();
        _authService = _serviceProvider.GetRequiredService<IAuthService>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "admin@smartworkz.test",
            Password: "TestPassword123!",
            TenantId: "DEFAULT"
        );

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.True(result.Succeeded, "Login should succeed");
        Assert.NotNull(result.Data);
        Assert.Equal("admin@smartworkz.test", result.Data.User.Email);
        Assert.Contains("Admin", result.Data.User.Roles, StringComparer.OrdinalIgnoreCase);
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "admin@smartworkz.test",
            Password: "WrongPassword123!",
            TenantId: "DEFAULT"
        );

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.False(result.Succeeded, "Login should fail with wrong password");
        Assert.Equal("auth.invalid_credentials", result.MessageKey);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "nonexistent@smartworkz.test",
            Password: "TestPassword123!",
            TenantId: "DEFAULT"
        );

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.False(result.Succeeded, "Login should fail for non-existent user");
        Assert.Equal("auth.invalid_credentials", result.MessageKey);
    }

    [Fact]
    public async Task LoginAsync_AllTestUsers_ShouldHaveAdminRole()
    {
        // Arrange
        var testUsers = new[]
        {
            ("admin@smartworkz.test", "Admin"),
            ("manager@smartworkz.test", "Manager"),
            ("staff@smartworkz.test", "Staff"),
            ("customer@smartworkz.test", "Customer")
        };

        // Act & Assert
        foreach (var (email, expectedRole) in testUsers)
        {
            var loginRequest = new LoginRequest(email, "TestPassword123!", "DEFAULT");
            var result = await _authService.LoginAsync(loginRequest);

            Assert.True(result.Succeeded, $"Login should succeed for {email}");
            Assert.NotNull(result.Data);
            Assert.Contains(expectedRole, result.Data.User.Roles, StringComparer.OrdinalIgnoreCase);
        }
    }
}
