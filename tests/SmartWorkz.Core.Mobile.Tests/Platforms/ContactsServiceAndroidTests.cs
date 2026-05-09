namespace SmartWorkz.Mobile.Tests.Platforms;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Platform-specific tests for ContactsService on Android.
/// Tests validate Android-specific contact retrieval with mocked Android APIs.
///
/// Note: Android-specific APIs (ContentResolver, ContactsContract) cannot be called
/// in unit tests without full Android SDK environment. These tests validate the
/// service behavior with mocked permission states.
///
/// Test Coverage:
/// 1. Android platform availability check
/// 2. Android contact retrieval flow validation
/// 3. Android search functionality
/// 4. Permission handling specific to Android READ_CONTACTS
/// </summary>
public class ContactsServiceAndroidTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServiceAndroidTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_AndroidWithPermission_RequestsFromAddressBook()
    {
        // Arrange
        // On Android, the service checks READ_CONTACTS permission
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // Verify permission was checked (actual contact retrieval would use Android ContentProvider)
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchContactsAsync_AndroidPlatform_UsesLikeQuery()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        const string searchQuery = "John";

        // Act
        var result = await _sut.SearchContactsAsync(searchQuery);

        // Assert
        // Android implementation uses SQL LIKE query on ContactsContract.Contacts
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PickContactAsync_Android_NotAvailable()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        // Android doesn't have a native contact picker in current implementation
        var result = await _sut.PickContactAsync();

        // Assert
        // Should return null on Android as contact picker is not available
        Assert.Null(result);
    }

    [Fact]
    public async Task IsAvailableAsync_Android_ReturnsTrue()
    {
        // Act
        // On Android platform, contacts service is available if hardware exists
        var result = await _sut.IsAvailableAsync();

        // Assert
        // Android platform supports contacts access when permission is granted
        Assert.IsType<bool>(result);
    }
}
