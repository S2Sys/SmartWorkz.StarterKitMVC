namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;

public class ConflictResolverTests
{
    [Fact]
    public async Task LastWriteWinsResolver_SelectsMoreRecentChange()
    {
        // Arrange
        var resolver = new LastWriteWinsResolver();
        var local = new SyncChange("id", "Order", "Status", "A", "B", DateTime.UtcNow.AddSeconds(10), "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", DateTime.UtcNow, "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.LastWriteWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(local, result);
    }

    [Fact]
    public async Task LastWriteWinsResolver_SelectsRemoteWhenMoreRecent()
    {
        // Arrange
        var resolver = new LastWriteWinsResolver();
        var now = DateTime.UtcNow;
        var local = new SyncChange("id", "Order", "Status", "A", "B", now, "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", now.AddSeconds(10), "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.LastWriteWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(remote, result);
    }

    [Fact]
    public async Task ClientWinsResolver_AlwaysSelectsLocal()
    {
        // Arrange
        var resolver = new ClientWinsResolver();
        var local = new SyncChange("id", "Order", "Status", "A", "B", DateTime.UtcNow, "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", DateTime.UtcNow.AddSeconds(10), "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.ClientWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(local, result);
    }

    [Fact]
    public async Task ClientWinsResolver_AlwaysSelectsLocalEvenIfOlder()
    {
        // Arrange
        var resolver = new ClientWinsResolver();
        var now = DateTime.UtcNow;
        var local = new SyncChange("id", "Order", "Status", "A", "B", now, "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", now.AddSeconds(30), "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.ClientWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(local, result);
    }

    [Fact]
    public async Task ServerWinsResolver_AlwaysSelectsRemote()
    {
        // Arrange
        var resolver = new ServerWinsResolver();
        var local = new SyncChange("id", "Order", "Status", "A", "B", DateTime.UtcNow.AddSeconds(10), "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", DateTime.UtcNow, "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.ServerWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(remote, result);
    }

    [Fact]
    public async Task ServerWinsResolver_AlwaysSelectsRemoteEvenIfOlder()
    {
        // Arrange
        var resolver = new ServerWinsResolver();
        var now = DateTime.UtcNow;
        var local = new SyncChange("id", "Order", "Status", "A", "B", now.AddSeconds(30), "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", now, "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.ServerWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(remote, result);
    }

    [Fact]
    public void LastWriteWinsResolver_CanResolve_ReturnsTrueForStrategy()
    {
        // Arrange
        var resolver = new LastWriteWinsResolver();

        // Act & Assert
        Assert.True(resolver.CanResolve(ConflictResolutionStrategy.LastWriteWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.ClientWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.ServerWins));
    }

    [Fact]
    public void ClientWinsResolver_CanResolve_ReturnsTrueForStrategy()
    {
        // Arrange
        var resolver = new ClientWinsResolver();

        // Act & Assert
        Assert.True(resolver.CanResolve(ConflictResolutionStrategy.ClientWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.LastWriteWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.ServerWins));
    }

    [Fact]
    public void ServerWinsResolver_CanResolve_ReturnsTrueForStrategy()
    {
        // Arrange
        var resolver = new ServerWinsResolver();

        // Act & Assert
        Assert.True(resolver.CanResolve(ConflictResolutionStrategy.ServerWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.LastWriteWins));
        Assert.False(resolver.CanResolve(ConflictResolutionStrategy.ClientWins));
    }

    [Fact]
    public async Task LastWriteWinsResolver_ThrowsGuardException_WhenConflictIsNull()
    {
        // Arrange
        var resolver = new LastWriteWinsResolver();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => resolver.ResolveAsync(null!));
    }

    [Fact]
    public async Task ClientWinsResolver_ThrowsGuardException_WhenConflictIsNull()
    {
        // Arrange
        var resolver = new ClientWinsResolver();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => resolver.ResolveAsync(null!));
    }

    [Fact]
    public async Task ServerWinsResolver_ThrowsGuardException_WhenConflictIsNull()
    {
        // Arrange
        var resolver = new ServerWinsResolver();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => resolver.ResolveAsync(null!));
    }

    [Fact]
    public async Task LastWriteWinsResolver_WithEqualTimestamps_SelectsRemote()
    {
        // Arrange
        var resolver = new LastWriteWinsResolver();
        var timestamp = DateTime.UtcNow;
        var local = new SyncChange("id", "Order", "Status", "A", "B", timestamp, "u1");
        var remote = new SyncChange("id", "Order", "Status", "A", "C", timestamp, "u2");
        var conflict = new SyncConflict(Guid.NewGuid().ToString(), local, remote,
            ConflictResolutionStrategy.LastWriteWins, DateTime.UtcNow);

        // Act
        var result = await resolver.ResolveAsync(conflict);

        // Assert
        Assert.Equal(remote, result); // When equal, remote is selected
    }
}
