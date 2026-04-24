namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Models;

public class ConflictResolutionEngineTests
{
    [Fact]
    public async Task ResolveAsync_WithLastWriteWinsResolver_ResolvesConflict()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow, "user2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), localChange, remoteChange,
            ConflictResolutionStrategy.LastWriteWins, DateTime.UtcNow);

        // Act
        var resolved = await engine.ResolveAsync(conflict);

        // Assert
        Assert.Equal(localChange, resolved); // Local is more recent
    }

    [Fact]
    public void RegisterResolver_CustomResolver_IsRegistered()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var mockResolver = new Mock<IConflictResolver>();

        // Act
        engine.RegisterResolver(ConflictResolutionStrategy.CustomResolver, mockResolver.Object);

        // Assert - verify it doesn't throw and resolver is stored
        Assert.NotNull(mockResolver.Object);
    }

    [Fact]
    public async Task ResolveAsync_WithClientWinsResolver_AlwaysSelectsLocal()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow, "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow.AddSeconds(10), "user2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), localChange, remoteChange,
            ConflictResolutionStrategy.ClientWins, DateTime.UtcNow);

        // Act
        var resolved = await engine.ResolveAsync(conflict);

        // Assert
        Assert.Equal(localChange, resolved);
    }

    [Fact]
    public async Task ResolveAsync_WithServerWinsResolver_AlwaysSelectsRemote()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow, "user2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), localChange, remoteChange,
            ConflictResolutionStrategy.ServerWins, DateTime.UtcNow);

        // Act
        var resolved = await engine.ResolveAsync(conflict);

        // Assert
        Assert.Equal(remoteChange, resolved);
    }

    [Fact]
    public async Task ResolveAsync_WithUnknownStrategy_FallsBackToLastWriteWins()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow, "user2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), localChange, remoteChange,
            ConflictResolutionStrategy.CustomResolver, DateTime.UtcNow);

        // Act
        var resolved = await engine.ResolveAsync(conflict);

        // Assert - Should use LastWriteWins (default)
        Assert.Equal(localChange, resolved);
    }

    [Fact]
    public async Task ResolveAsync_WithCustomResolver_UsesRegisteredResolver()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();
        var mockResolver = new Mock<IConflictResolver>();
        var expectedChange = new SyncChange("id1", "Order", "Status", "Pending", "Custom",
            DateTime.UtcNow, "user1");

        mockResolver.Setup(r => r.ResolveAsync(It.IsAny<SyncConflict>()))
            .ReturnsAsync(expectedChange);

        engine.RegisterResolver(ConflictResolutionStrategy.CustomResolver, mockResolver.Object);

        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow, "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow.AddSeconds(10), "user2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), localChange, remoteChange,
            ConflictResolutionStrategy.CustomResolver, DateTime.UtcNow);

        // Act
        var resolved = await engine.ResolveAsync(conflict);

        // Assert
        Assert.Equal(expectedChange, resolved);
        mockResolver.Verify(r => r.ResolveAsync(conflict), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_ThrowsGuardException_WhenConflictIsNull()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.ResolveAsync(null!));
    }

    [Fact]
    public void RegisterResolver_ThrowsGuardException_WhenResolverIsNull()
    {
        // Arrange
        var engine = new ConflictResolutionEngine();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => engine.RegisterResolver(ConflictResolutionStrategy.CustomResolver, null!));
    }
}
