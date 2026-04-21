using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.MultiTenancy;

/// <summary>
/// Tests for TenantContext - AsyncLocal-based scoped tenant tracking.
/// </summary>
public class TenantContextTests : IDisposable
{
    private readonly TenantContext _context;

    public TenantContextTests()
    {
        _context = new TenantContext();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region GetTenantId Tests

    [Fact]
    public void GetTenantId_InitiallyReturnsDefault()
    {
        // Act
        var tenantId = _context.GetTenantId();

        // Assert
        Assert.Equal("default", tenantId);
    }

    [Fact]
    public void GetTenantId_ReturnsUpdatedValueAfterSetTenantId()
    {
        // Arrange
        var newTenantId = "tenant-123";

        // Act
        _context.SetTenantId(newTenantId);
        var result = _context.GetTenantId();

        // Assert
        Assert.Equal(newTenantId, result);
    }

    #endregion

    #region SetTenantId Tests

    [Fact]
    public void SetTenantId_UpdatesTenantIdProperty()
    {
        // Arrange
        var newTenantId = "tenant-456";

        // Act
        _context.SetTenantId(newTenantId);

        // Assert
        Assert.Equal(newTenantId, _context.TenantId);
    }

    [Fact]
    public void SetTenantId_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _context.SetTenantId(null!));
        Assert.Contains("Value cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void SetTenantId_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _context.SetTenantId(""));
        Assert.Contains("Value cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void SetTenantId_WithWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _context.SetTenantId("   "));
        Assert.Contains("Value cannot be null or whitespace", ex.Message);
    }

    #endregion

    #region TenantId Property Tests

    [Fact]
    public void TenantId_PropertyGet_ReturnsCurrentValue()
    {
        // Arrange
        var tenantId = "tenant-property";

        // Act
        _context.TenantId = tenantId;
        var result = _context.TenantId;

        // Assert
        Assert.Equal(tenantId, result);
    }

    [Fact]
    public void TenantId_PropertySet_UpdatesValue()
    {
        // Arrange
        var tenantId = "tenant-prop-set";

        // Act
        _context.TenantId = tenantId;

        // Assert
        Assert.Equal(tenantId, _context.TenantId);
    }

    #endregion

    #region IsMultiTenant Tests

    [Fact]
    public void IsMultiTenant_AlwaysReturnsTrue()
    {
        // Act
        var isMultiTenant = _context.IsMultiTenant;

        // Assert
        Assert.True(isMultiTenant);
    }

    [Fact]
    public void IsMultiTenant_RemainsTrue_AfterSettingTenantId()
    {
        // Arrange
        _context.SetTenantId("any-tenant");

        // Act
        var isMultiTenant = _context.IsMultiTenant;

        // Assert
        Assert.True(isMultiTenant);
    }

    #endregion

    #region Async Context Isolation Tests

    [Fact]
    public async Task DifferentAsyncContexts_HaveIsolatedTenantIds()
    {
        // Arrange
        var mainTenantId = "main-tenant";
        _context.SetTenantId(mainTenantId);

        // Act
        var nestedTenantId = "nested-tenant";
        string? nestedResult = null;

        var task = Task.Run(() =>
        {
            _context.SetTenantId(nestedTenantId);
            nestedResult = _context.GetTenantId();
        });

        await task;
        var mainResult = _context.GetTenantId();

        // Assert
        Assert.Equal(nestedTenantId, nestedResult);
        Assert.Equal(mainTenantId, mainResult);
    }

    [Fact]
    public async Task NestedAsyncFlows_PreserveParentTenantContext()
    {
        // Arrange
        var parentTenantId = "parent-tenant";
        _context.SetTenantId(parentTenantId);

        // Act
        string? childResult = null;
        await Task.Run(async () =>
        {
            // Child task should inherit parent's tenant context
            childResult = _context.GetTenantId();
            await Task.Delay(10);
        });

        var parentResult = _context.GetTenantId();

        // Assert
        Assert.Equal(parentTenantId, childResult);
        Assert.Equal(parentTenantId, parentResult);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetTenantId_AfterMultipleSetCalls_ReturnsLastValue()
    {
        // Arrange
        _context.SetTenantId("first");
        _context.SetTenantId("second");
        _context.SetTenantId("final");

        // Act
        var result = _context.GetTenantId();

        // Assert
        Assert.Equal("final", result);
    }

    [Fact]
    public void SetTenantId_WithSpecialCharacters_Succeeds()
    {
        // Arrange
        var specialTenantId = "tenant-with-special-chars-@#$%";

        // Act
        _context.SetTenantId(specialTenantId);

        // Assert
        Assert.Equal(specialTenantId, _context.GetTenantId());
    }

    [Fact]
    public void SetTenantId_WithVeryLongValue_Succeeds()
    {
        // Arrange
        var longTenantId = new string('a', 1000);

        // Act
        _context.SetTenantId(longTenantId);

        // Assert
        Assert.Equal(longTenantId, _context.GetTenantId());
    }

    #endregion
}

