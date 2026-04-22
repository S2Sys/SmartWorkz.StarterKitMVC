using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the standalone AuditDeletableEntity&lt;TId&gt; and AuditDeletableEntity base classes.
/// Tests combined audit trailing and soft delete functionality.
///
/// Standalone Design:
/// AuditDeletableEntity is a self-contained class that inherits from Entity&lt;TId&gt; and
/// implements both IAuditable and ISoftDeletable directly, combining audit tracking with soft delete
/// capabilities in a single entity class without hierarchical inheritance.
/// </summary>
public class AuditDeletableEntityTests
{
    #region AuditDeletableEntity<int> - Audit Property Tests

    [Fact]
    public void AuditDeletableEntity_StoresCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableEntityInt { CreatedAt = now };

        // Act
        var createdAt = entity.CreatedAt;

        // Assert
        Assert.Equal(now, createdAt);
    }

    [Fact]
    public void AuditDeletableEntity_StoresCreatedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { CreatedBy = "user123" };

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal("user123", createdBy);
    }

    [Fact]
    public void AuditDeletableEntity_StoresUpdatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableEntityInt { UpdatedAt = now };

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Equal(now, updatedAt);
    }

    [Fact]
    public void AuditDeletableEntity_StoresUpdatedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { UpdatedBy = "user456" };

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Equal("user456", updatedBy);
    }

    [Fact]
    public void AuditDeletableEntity_CreatedByDefaultIsEmpty()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal(string.Empty, createdBy);
    }

    [Fact]
    public void AuditDeletableEntity_UpdatedAtDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Null(updatedAt);
    }

    [Fact]
    public void AuditDeletableEntity_UpdatedByDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Null(updatedBy);
    }

    #endregion

    #region AuditDeletableEntity<int> - Soft Delete Property Tests

    [Fact]
    public void AuditDeletableEntity_IsDeletedDefaultIsFalse()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void AuditDeletableEntity_StoresIsDeleted()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { IsDeleted = true };

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public void AuditDeletableEntity_StoresDeletedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableEntityInt { DeletedAt = now };

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Equal(now, deletedAt);
    }

    [Fact]
    public void AuditDeletableEntity_StoresDeletedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { DeletedBy = 42 };

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Equal(42, deletedBy);
    }

    [Fact]
    public void AuditDeletableEntity_DeletedAtDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Null(deletedAt);
    }

    [Fact]
    public void AuditDeletableEntity_DeletedByDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Null(deletedBy);
    }

    #endregion

    #region AuditDeletableEntity Combined Properties Test

    [Fact]
    public void AuditDeletableEntity_AllPropertiesAreSettable()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var updateTime = DateTime.UtcNow.AddHours(1);
        var deleteTime = DateTime.UtcNow.AddHours(2);
        var entity = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            UpdatedAt = updateTime,
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deleteTime,
            DeletedBy = 99
        };

        // Act & Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(creationTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(updateTime, entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deleteTime, entity.DeletedAt);
        Assert.Equal(99, entity.DeletedBy);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void AuditDeletableEntity_ImplementsIAuditable()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<IAuditable>(entity);
    }

    [Fact]
    public void AuditDeletableEntity_ImplementsISoftDeletable()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<ISoftDeletable>(entity);
    }

    [Fact]
    public void AuditDeletableEntity_ImplementsIAuditableInterface_HasAuditProperties()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt() as IAuditable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("CreatedAt") != null);
        Assert.True(entity.GetType().GetProperty("CreatedBy") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedAt") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedBy") != null);
    }

    [Fact]
    public void AuditDeletableEntity_ImplementsISoftDeletableInterface_HasDeleteProperties()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt() as ISoftDeletable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("IsDeleted") != null);
        Assert.True(entity.GetType().GetProperty("DeletedAt") != null);
        Assert.True(entity.GetType().GetProperty("DeletedBy") != null);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuditDeletableEntity_InheritsFromEntity()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void AuditDeletableEntity_InheritsIdProperty()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt { Id = 99 };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(99, id);
    }

    [Fact]
    public void AuditDeletableEntity_PreservesEntityEqualitySemantics()
    {
        // Arrange
        var entity1 = new TestAuditDeletableEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = true
        };
        var entity2 = new TestAuditDeletableEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "bob",
            IsDeleted = false
        };

        // Act & Assert
        // Two entities with same Id are equal, regardless of audit or delete properties
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void AuditDeletableEntity_DifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestAuditDeletableEntityInt { Id = 5 };
        var entity2 = new TestAuditDeletableEntityInt { Id = 10 };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
    }

    [Fact]
    public void AuditDeletableEntity_CanBeUsedInHashSet()
    {
        // Arrange
        var entity1 = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1",
            IsDeleted = false
        };
        var entity2 = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "user2",
            IsDeleted = true
        };
        var entity3 = new TestAuditDeletableEntityInt { Id = 2 };
        var set = new HashSet<TestAuditDeletableEntityInt>();

        // Act
        set.Add(entity1);
        set.Add(entity2); // Should not add duplicate (same Id)
        set.Add(entity3);

        // Assert
        Assert.Equal(2, set.Count);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void AuditDeletableEntity_SupportsGuidPrimaryKey()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new TestAuditDeletableEntityGuid
        {
            Id = guid,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user123",
            IsDeleted = false
        };

        // Act & Assert
        Assert.Equal(guid, entity.Id);
        Assert.Equal("user123", entity.CreatedBy);
        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void AuditDeletableEntity_SupportsStringPrimaryKey()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityString
        {
            Id = "CODE123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user456",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddHours(1),
            DeletedBy = 7
        };

        // Act & Assert
        Assert.Equal("CODE123", entity.Id);
        Assert.Equal("user456", entity.CreatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(7, entity.DeletedBy);
    }

    #endregion

    #region Convenience Class Tests

    [Fact]
    public void AuditDeletableEntity_ConvenienceClass_HasIntId()
    {
        // Arrange
        var entity = new TestAuditDeletableEntity { Id = 100, CreatedAt = DateTime.UtcNow };

        // Act & Assert
        Assert.Equal(100, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void AuditDeletableEntity_ConvenienceClass_InheritsFromAuditDeletableEntityInt()
    {
        // Arrange & Act
        var entity = new TestAuditDeletableEntity { Id = 50 };

        // Assert
        Assert.IsAssignableFrom<AuditDeletableEntity<int>>(entity);
    }

    [Fact]
    public void AuditDeletableEntity_ConvenienceClass_HasAllAuditAndDeleteProperties()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var deleteTime = DateTime.UtcNow.AddHours(1);
        var entity = new TestAuditDeletableEntity
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            UpdatedAt = creationTime.AddHours(1),
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deleteTime,
            DeletedBy = 42
        };

        // Act & Assert
        Assert.Equal(creationTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(creationTime.AddHours(1), entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deleteTime, entity.DeletedAt);
        Assert.Equal(42, entity.DeletedBy);
    }

    #endregion

    #region Soft Delete Scenario Tests

    [Fact]
    public void AuditDeletableEntity_SoftDeleteScenario_FullAuditTrail()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var updateTime = creationTime.AddHours(2);
        var deleteTime = creationTime.AddHours(5);

        var entity = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            UpdatedAt = updateTime,
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deleteTime,
            DeletedBy = 99
        };

        // Act
        var auditTrail = new
        {
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            IsDeleted = entity.IsDeleted,
            DeletedAt = entity.DeletedAt,
            DeletedBy = entity.DeletedBy
        };

        // Assert - Verify complete audit trail
        Assert.Equal(creationTime, auditTrail.CreatedAt);
        Assert.Equal("alice", auditTrail.CreatedBy);
        Assert.Equal(updateTime, auditTrail.UpdatedAt);
        Assert.Equal("bob", auditTrail.UpdatedBy);
        Assert.True(auditTrail.IsDeleted);
        Assert.Equal(deleteTime, auditTrail.DeletedAt);
        Assert.Equal(99, auditTrail.DeletedBy);
    }

    [Fact]
    public void AuditDeletableEntity_ActiveEntity_DeletedFieldsAreNull()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = false
        };

        // Act & Assert - Active entities have null delete fields
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);
    }

    [Fact]
    public void AuditDeletableEntity_CanTransitionFromActiveToDeleted()
    {
        // Arrange
        var entity = new TestAuditDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = false
        };

        // Act
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow.AddHours(1);
        entity.DeletedBy = 42;

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.NotNull(entity.DeletedAt);
        Assert.Equal(42, entity.DeletedBy);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test audit-deletable entity with int primary key.
    /// </summary>
    private class TestAuditDeletableEntityInt : AuditDeletableEntity<int>
    {
    }

    /// <summary>
    /// Test audit-deletable entity with Guid primary key.
    /// </summary>
    private class TestAuditDeletableEntityGuid : AuditDeletableEntity<Guid>
    {
    }

    /// <summary>
    /// Test audit-deletable entity with string primary key.
    /// </summary>
    private class TestAuditDeletableEntityString : AuditDeletableEntity<string>
    {
    }

    /// <summary>
    /// Test convenience audit-deletable entity inheriting from AuditDeletableEntity (int primary key).
    /// </summary>
    private class TestAuditDeletableEntity : AuditDeletableEntity
    {
    }

    #endregion
}
