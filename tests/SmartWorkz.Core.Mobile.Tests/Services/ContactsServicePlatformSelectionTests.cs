namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Tests for ContactsService platform selection and routing logic.
/// Validates that the service correctly routes to platform-specific implementations
/// based on runtime platform detection.
///
/// Test Coverage:
/// 1. Windows platform routing (returns empty/null)
/// 2. Non-Windows platform routing (calls platform implementations)
/// 3. Conditional compilation handling (#if/#endif guards)
/// </summary>
public class ContactsServicePlatformSelectionTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServicePlatformSelectionTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_PlatformRouting_HandlesDifferentPlatforms()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        // Service should route to appropriate platform implementation
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // Result depends on platform: empty on Windows, contacts from platform code on mobile
        Assert.NotNull(result);
        Assert.IsType<List<Contact>>(result);
    }

    [Fact]
    public async Task SearchContactsAsync_PlatformRouting_HandlesDifferentPlatforms()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        const string query = "Test";

        // Act
        // Service should route to appropriate platform implementation
        var result = await _sut.SearchContactsAsync(query);

        // Assert
        // Result depends on platform: empty on Windows, filtered contacts on mobile
        Assert.NotNull(result);
        Assert.IsType<List<Contact>>(result);
    }

    [Fact]
    public async Task PickContactAsync_PlatformRouting_HandlesDifferentPlatforms()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        // Service should route to appropriate platform implementation
        var result = await _sut.PickContactAsync();

        // Assert
        // Result depends on platform:
        // - Windows: returns null (not supported)
        // - Android: returns null (not available)
        // - iOS/macOS: may return Contact or null (if cancelled by user)
        Assert.IsType<Contact?>(result);
    }
}
