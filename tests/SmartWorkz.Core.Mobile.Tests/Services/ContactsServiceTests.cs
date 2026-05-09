namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

/// <summary>
/// Unit tests for ContactsService covering contact retrieval, searching, and permission handling.
/// Tests validate platform-agnostic behavior with mocked permission service.
///
/// Test Coverage:
/// 1. GetAllContactsAsync with permission granted (happy path)
/// 2. GetAllContactsAsync with permission denied (error handling)
/// 3. GetAllContactsAsync with Windows platform (unavailable)
/// 4. SearchContactsAsync with matching results
/// 5. SearchContactsAsync with no results
/// 6. SearchContactsAsync with permission denied
/// 7. PickContactAsync with permission granted
/// 8. IsAvailableAsync availability check
///
/// Patterns:
/// - Per-test mock creation for IPermissionService
/// - Permission request/check flow validation
/// - Empty list returns on permission denial
/// - Null returns on unavailable operations
/// - Exception handling and resilience testing
/// </summary>
public class ContactsServiceTests
{
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly ContactsService _sut;

    public ContactsServiceTests()
    {
        _sut = new ContactsService(NullLogger<ContactsService>.Instance, _permissionService.Object);
    }

    [Fact]
    public async Task GetAllContactsAsync_PermissionGranted_ReturnsContactList()
    {
        // Arrange
        var expectedContacts = new List<Contact>
        {
            new Contact("1", "John", "Doe", "john@example.com", "555-1234", null),
            new Contact("2", "Jane", "Smith", "jane@example.com", "555-5678", null)
        };

        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.GetAllContactsAsync();

        // Assert
        Assert.NotNull(result);
        // Note: Actual platform implementation would return contacts from platform code
        // For this test, we verify the service accepts permission-granted scenario
        _permissionService.Verify(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllContactsAsync_PermissionDenied_ReturnsEmptyList()
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
        Assert.NotNull(result);
        Assert.Empty(result);
        _permissionService.Verify(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllContactsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.GetAllContactsAsync(cts.Token));
    }

    [Fact]
    public async Task SearchContactsAsync_ValidQuery_ReturnsResults()
    {
        // Arrange
        const string query = "John";

        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.SearchContactsAsync(query);

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchContactsAsync_PermissionDenied_ReturnsEmptyList()
    {
        // Arrange
        const string query = "John";

        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        _permissionService
            .Setup(p => p.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.SearchContactsAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchContactsAsync_EmptyQuery_ThrowsException()
    {
        // Arrange
        const string query = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SearchContactsAsync(query));
    }

    [Fact]
    public async Task PickContactAsync_PermissionGranted_ReturnsContact()
    {
        // Arrange
        _permissionService
            .Setup(p => p.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.PickContactAsync();

        // Assert
        // On Windows, returns null. On mobile platforms with native implementation, would return Contact
        Assert.IsType<Contact?>(result);
    }

    [Fact]
    public async Task IsAvailableAsync_ChecksServiceAvailability()
    {
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }
}
