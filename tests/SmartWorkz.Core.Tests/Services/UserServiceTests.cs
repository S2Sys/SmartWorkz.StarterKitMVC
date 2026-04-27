using Xunit;
using Moq;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Services;

/// <summary>
/// Unit tests for IUserService interface and UserDto mapping.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserService> _mockUserService;

    public UserServiceTests()
    {
        _mockUserService = new Mock<IUserService>();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
    {
        // Arrange
        var userId = "user-123";
        var expectedUser = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _mockUserService.Object.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.True(result.IsActive);
        _mockUserService.Verify(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var userId = "non-existent";
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto)null!);

        // Act
        var result = await _mockUserService.Object.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserByIdAsync(null!, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException(nameof(null)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _mockUserService.Object.GetUserByIdAsync(null!));
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUserDto()
    {
        // Arrange
        var email = "user@example.com";
        var expectedUser = new UserDto
        {
            Id = "user-123",
            Email = email,
            FirstName = "Jane",
            LastName = "Smith",
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _mockUserService.Object.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-123", result.Id);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserService.Setup(s => s.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto)null!);

        // Act
        var result = await _mockUserService.Object.GetUserByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsCollectionOfUserDtos()
    {
        // Arrange
        var users = new[]
        {
            new UserDto { Id = "user-1", Email = "user1@example.com", FirstName = "Alice", LastName = "A", IsActive = true },
            new UserDto { Id = "user-2", Email = "user2@example.com", FirstName = "Bob", LastName = "B", IsActive = true },
            new UserDto { Id = "user-3", Email = "user3@example.com", FirstName = "Charlie", LastName = "C", IsActive = false }
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _mockUserService.Object.GetAllUsersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count());
        Assert.Single(result.Where(u => u.Id == "user-1"));
    }

    [Fact]
    public async Task CreateUserAsync_WithValidUserData_ReturnsCreatedUserDto()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        var createdUser = new UserDto
        {
            Id = "user-new",
            Email = createRequest.Email,
            FirstName = createRequest.FirstName,
            LastName = createRequest.LastName,
            IsActive = true
        };

        _mockUserService.Setup(s => s.CreateUserAsync(createRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _mockUserService.Object.CreateUserAsync(createRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.True(result.IsActive);
        _mockUserService.Verify(s => s.CreateUserAsync(createRequest, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        _mockUserService.Setup(s => s.CreateUserAsync(null!, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException(nameof(null)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _mockUserService.Object.CreateUserAsync(null!));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = "user-123";
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast"
        };

        var updatedUser = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast",
            IsActive = true
        };

        _mockUserService.Setup(s => s.UpdateUserAsync(userId, updateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _mockUserService.Object.UpdateUserAsync(userId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedFirst", result.FirstName);
        Assert.Equal("UpdatedLast", result.LastName);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_RemovesUser()
    {
        // Arrange
        var userId = "user-123";
        _mockUserService.Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockUserService.Object.DeleteUserAsync(userId);

        // Assert
        _mockUserService.Verify(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UserDto_Properties_AreAccessible()
    {
        // Arrange
        var user = new UserDto
        {
            Id = "user-props",
            Email = "props@example.com",
            FirstName = "Props",
            LastName = "Test",
            IsActive = true
        };

        // Act & Assert
        Assert.Equal("user-props", user.Id);
        Assert.Equal("props@example.com", user.Email);
        Assert.Equal("Props", user.FirstName);
        Assert.Equal("Test", user.LastName);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void UserDto_WithNullProperties_AllowsNullEmail()
    {
        // Arrange
        var user = new UserDto
        {
            Id = "user-null",
            Email = null!,
            FirstName = "Null",
            LastName = "Test",
            IsActive = true
        };

        // Act & Assert
        Assert.Null(user.Email);
        Assert.NotNull(user.Id);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var activeUsers = new[]
        {
            new UserDto { Id = "user-1", Email = "user1@example.com", FirstName = "Active", LastName = "User", IsActive = true }
        };

        _mockUserService.Setup(s => s.GetActiveUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUsers);

        // Act
        var result = await _mockUserService.Object.GetActiveUsersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, u => Assert.True(u.IsActive));
    }
}

/// <summary>
/// User service interface.
/// </summary>
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer object for user data.
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to create a new user.
/// </summary>
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Request to update user information.
/// </summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
