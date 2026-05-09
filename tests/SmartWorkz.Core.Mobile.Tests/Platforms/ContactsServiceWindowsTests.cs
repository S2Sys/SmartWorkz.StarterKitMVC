namespace SmartWorkz.Mobile.Tests.Platforms;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Platform-specific tests for ContactsService on Windows.
/// Tests validate that contact access is properly unavailable on Windows.
///
/// Windows does not have a standard contacts API for MAUI applications.
/// The ContactsService returns empty results or null on Windows platform.
///
/// Test Coverage:
/// 1. GetAllContactsAsync returns empty list on Windows
/// 2. SearchContactsAsync returns empty list on Windows
/// 3. PickContactAsync returns null on Windows
/// 4. IsAvailableAsync returns false on Windows
/// </summary>
public class ContactsServiceWindowsTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServiceWindowsTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_Windows_ReturnsEmptyList()
    {
        // Arrange
        // On Windows, contact access is not available

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // Windows platform doesn't support contact access in MAUI
        Assert.NotNull(result);
        Assert.Empty(result);
        // Verify permission was never requested since Windows doesn't support it
        _permissionService.Verify(
            p => p.CheckAsync(It.IsAny<MobilePermission>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchContactsAsync_Windows_ReturnsEmptyList()
    {
        // Arrange
        const string searchQuery = "Contact";

        // Act
        var result = await _sut.SearchContactsAsync(searchQuery);

        // Assert
        // Windows platform doesn't support contact search
        Assert.NotNull(result);
        Assert.Empty(result);
        _permissionService.Verify(
            p => p.CheckAsync(It.IsAny<MobilePermission>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PickContactAsync_Windows_ReturnsNull()
    {
        // Act
        var result = await _sut.PickContactAsync();

        // Assert
        // Windows platform doesn't support contact picker
        Assert.Null(result);
        _permissionService.Verify(
            p => p.CheckAsync(It.IsAny<MobilePermission>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsAvailableAsync_Windows_ReturnsFalse()
    {
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        // Windows platform doesn't have contact service available
        Assert.False(result);
    }
}
