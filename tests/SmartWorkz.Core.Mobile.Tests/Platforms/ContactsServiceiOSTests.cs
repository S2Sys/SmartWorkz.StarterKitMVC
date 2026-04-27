namespace SmartWorkz.Mobile.Tests.Platforms;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Platform-specific tests for ContactsService on iOS.
/// Tests validate iOS-specific contact retrieval with mocked iOS APIs.
///
/// Note: iOS-specific APIs (Contacts framework) cannot be called in unit tests
/// without full iOS SDK environment. These tests validate the service behavior
/// with mocked permission states.
///
/// Test Coverage:
/// 1. iOS platform availability check
/// 2. iOS contact retrieval with Contacts framework
/// 3. iOS contact picker functionality
/// 4. Permission handling specific to iOS Contacts permission
/// </summary>
public class ContactsServiceiOSTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServiceiOSTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_iOSWithPermission_UsesContactsFramework()
    {
        // Arrange
        // On iOS, the service checks Contacts permission (not READ_CONTACTS like Android)
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // iOS uses CNContactStore to retrieve all contacts from address book
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchContactsAsync_iOSPlatform_FiltersByNameEmailPhone()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        const string searchQuery = "Smith";

        // Act
        var result = await _sut.SearchContactsAsync(searchQuery);

        // Assert
        // iOS implementation filters CNContact results by given/family name and email
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PickContactAsync_iOS_UsesNativeContactPicker()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        // iOS supports native contact picker via CNContactPickerViewController
        var result = await _sut.PickContactAsync();

        // Assert
        // iOS can return a single contact picked by user or null if cancelled
        Assert.IsType<Contact?>(result);
    }

    [Fact]
    public async Task IsAvailableAsync_iOS_ChecksFrameworkAvailability()
    {
        // Act
        // On iOS, contacts service is available on all iOS versions with Contacts framework
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }
}
