namespace SmartWorkz.Sample.ECommerce.Tests.Unit.Services;

using Moq;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Application.Validators;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Domain.Specifications;

public class ECommerceAuthServiceTests
{
    private readonly Mock<IRepository<Customer, int>> _customerRepoMock = new();
    private readonly JwtSettings _jwtSettings = new()
    {
        Secret = "test-secret-key-32-characters-minimum!!!!",
        Issuer = "test",
        Audience = "test",
        ExpiryMinutes = 60
    };
    private readonly RegisterValidator _registerValidator = new();

    private ECommerceAuthService CreateService() =>
        new(_customerRepoMock.Object, _jwtSettings, _registerValidator);

    [Fact]
    public async Task LoginAsync_with_unknown_email_returns_failure()
    {
        // Arrange
        _customerRepoMock
            .Setup(r => r.FindAllAsync(It.IsAny<CustomerByEmailSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>().AsReadOnly());

        var service = CreateService();

        // Act
        var result = await service.LoginAsync("nobody@example.com", "password");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_with_wrong_password_returns_failure()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = EmailAddress.Create("test@example.com").Data!,
            PasswordHash = EncryptionHelper.HashPassword("correctpassword")
        };

        _customerRepoMock
            .Setup(r => r.FindAllAsync(It.IsAny<CustomerByEmailSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer }.AsReadOnly());

        var service = CreateService();

        // Act
        var result = await service.LoginAsync("test@example.com", "wrongpass");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task RegisterAsync_with_invalid_dto_returns_failure()
    {
        // Arrange
        var service = CreateService();
        var dto = new RegisterDto("", "", "notanemail", "short", "short");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task RegisterAsync_with_valid_dto_returns_success_with_token()
    {
        // Arrange
        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var dto = new RegisterDto("John", "Doe", "john@example.com", "ValidPass123!", "ValidPass123!");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Data));
    }

    [Fact]
    public async Task RegisterAsync_creates_new_customer_in_repository()
    {
        // Arrange
        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var dto = new RegisterDto("Jane", "Smith", "jane@example.com", "SecurePass123!", "SecurePass123!");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.True(result.Succeeded);
        _customerRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
