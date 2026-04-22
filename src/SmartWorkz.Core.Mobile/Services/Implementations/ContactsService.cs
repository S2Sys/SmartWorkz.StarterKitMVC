namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides contact access services for retrieving and searching device contacts.
/// </summary>
public partial class ContactsService : IContactsService
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissions;

    public ContactsService(ILogger logger, IPermissionService permissions)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _permissions = Guard.NotNull(permissions, nameof(permissions));
    }

    /// <summary>
    /// Retrieves all contacts from the device.
    /// </summary>
    public async Task<IReadOnlyList<Contact>> GetAllContactsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Contact access is not available on Windows platform");
        return [];
        #else
        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Contacts, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Contacts, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Contacts permission denied");
                return [];
            }

            return await GetAllContactsAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all contacts");
            return [];
        }
        #endif
    }

    /// <summary>
    /// Searches for contacts matching the given query.
    /// </summary>
    public async Task<IReadOnlyList<Contact>> SearchContactsAsync(string query, CancellationToken ct = default)
    {
        Guard.NotEmpty(query, nameof(query));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Contact search is not available on Windows platform");
        return [];
        #else
        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Contacts, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Contacts, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Contacts permission denied for search");
                return [];
            }

            return await SearchContactsAsyncPlatform(query, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search contacts");
            return [];
        }
        #endif
    }

    /// <summary>
    /// Picks a single contact using the device's native contact picker.
    /// </summary>
    public async Task<Contact?> PickContactAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Contact picker is not available on Windows platform");
        return null;
        #else
        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Contacts, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Contacts, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Contacts permission denied for picker");
                return null;
            }

            return await PickContactAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick contact");
            return null;
        }
        #endif
    }

    /// <summary>
    /// Checks if contact access is available on the device.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await IsAvailableAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check contact service availability");
            return false;
        }
        #endif
    }

    // Platform-specific partial methods (declared only for non-Windows)
    #if !WINDOWS
    private partial Task<IReadOnlyList<Contact>> GetAllContactsAsyncPlatform(CancellationToken ct);
    private partial Task<IReadOnlyList<Contact>> SearchContactsAsyncPlatform(string query, CancellationToken ct);
    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
    #endif
}
