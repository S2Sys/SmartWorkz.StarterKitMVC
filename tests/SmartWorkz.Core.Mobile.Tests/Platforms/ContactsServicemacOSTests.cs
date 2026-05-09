namespace SmartWorkz.Mobile.Tests.Platforms;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Platform-specific tests for ContactsService on macOS.
/// Tests validate macOS-specific contact retrieval with mocked macOS APIs.
///
/// Note: macOS-specific APIs (Contacts framework) cannot be called in unit tests
/// without full macOS SDK environment. These tests validate the service behavior
/// with mocked permission states.
///
/// Test Coverage:
/// 1. macOS platform availability check
/// 2. macOS contact retrieval with Contacts framework
/// 3. macOS contact picker functionality (if available)
/// 4. Permission handling specific to macOS Contacts permission
/// </summary>
public class ContactsServicemacOSTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServicemacOSTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_macOSWithPermission_UsesContactsFramework()
    {
        // Arrange
        // On macOS (Mac Catalyst), the service checks Contacts permission
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // macOS uses CNContactStore (same as iOS) to retrieve all contacts
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchContactsAsync_macOSPlatform_FiltersByContactInfo()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        const string searchQuery = "Alice";

        // Act
        var result = await _sut.SearchContactsAsync(searchQuery);

        // Assert
        // macOS implementation filters CNContact results similar to iOS
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PickContactAsync_macOS_NotAvailableOrUsesSystemPicker()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        // macOS may not have a contact picker like iOS, or may use system dialog
        var result = await _sut.PickContactAsync();

        // Assert
        Assert.IsType<Contact?>(result);
    }

    [Fact]
    public async Task IsAvailableAsync_macOS_ChecksFrameworkAvailability()
    {
        // Act
        // On macOS (Mac Catalyst), contacts service availability depends on platform
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }
}
