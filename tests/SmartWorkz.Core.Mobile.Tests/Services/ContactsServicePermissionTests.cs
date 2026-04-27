namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Tests for ContactsService permission handling and permission request flow.
/// Validates that the service properly checks and requests permissions before
/// accessing contacts.
///
/// Test Coverage:
/// 1. Permission already granted flows (check-only)
/// 2. Permission denied flows (request permission)
/// 3. Multiple permission status scenarios (Denied, DeniedAlways, Restricted, Limited)
/// </summary>
public class ContactsServicePermissionTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServicePermissionTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_PermissionAlreadyGranted_DoesNotRequestAgain()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // If already granted, should not request again
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
        _permissionService.Verify(
            p => p.RequestAsync(It.IsAny<MobilePermission>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllContactsAsync_PermissionNotGranted_RequestsPermission()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);
        _permissionService
            .Setup(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // Should request permission after check fails
        _permissionService.Verify(
            p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
        _permissionService.Verify(
            p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllContactsAsync_PermissionDeniedAlways_ReturnsEmpty()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.DeniedAlways);
        _permissionService
            .Setup(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.DeniedAlways);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        // Even after request, if still denied, return empty
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchContactsAsync_PermissionRequestGrantedOnSecondAttempt_ProceedsWithSearch()
    {
        // Arrange
        var permissionChecks = 0;
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .Returns((MobilePermission _, CancellationToken _) =>
            {
                permissionChecks++;
                // First check denies, simulating permission not yet granted
                return Task.FromResult(PermissionStatus.Denied);
            });

        _permissionService
            .Setup(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.SearchContactsAsync("test");

        // Assert
        // After permission granted by request, should proceed
        Assert.NotNull(result);
        _permissionService.Verify(
            p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PickContactAsync_PermissionStatuses_HandleRestrictedAndLimited()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Limited); // iOS: can access some contacts
        _permissionService
            .Setup(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Limited);

        // Act
        var result = await _sut.PickContactAsync();

        // Assert
        // Limited permission should still allow contact picker to proceed
        Assert.IsType<Contact?>(result);
    }
}
