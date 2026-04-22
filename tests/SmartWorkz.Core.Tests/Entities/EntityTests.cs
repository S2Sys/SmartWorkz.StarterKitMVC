using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the Entity&lt;TId&gt; and Entity base classes.
/// Tests DDD identity-based equality semantics.
/// </summary>
public class EntityTests
{
    #region Entity<int> Tests

    [Fact]
    public void Entity_Int_WithIdSet_ReturnsCorrectId()
    {
        // Arrange
        var entity = new TestEntityInt { Id = 42 };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(42, id);
    }

    [Fact]
    public void Entity_ConvenienceClass_HasIntId()
    {
        // Arrange
        var entity = new TestEntity { Id = 99 };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(99, id);
        Assert.IsType<int>(id);
    }

    [Fact]
    public void Entity_IntNewInstance_HasDefaultId()
    {
        // Arrange & Act
        var entity = new TestEntityInt();

        // Assert
        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void Entity_TwoIntInstancesWithSameId_AreEqual()
    {
        // Arrange
        var entity1 = new TestEntityInt { Id = 5 };
        var entity2 = new TestEntityInt { Id = 5 };

        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.False(entity1 != entity2);
    }

    [Fact]
    public void Entity_TwoIntInstancesWithDifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityInt { Id = 5 };
        var entity2 = new TestEntityInt { Id = 10 };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void Entity_IntWithSameId_HaveSameHashCode()
    {
        // Arrange
        var entity1 = new TestEntityInt { Id = 7 };
        var entity2 = new TestEntityInt { Id = 7 };

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Entity_IntInstancesCanBeUsedInHashSet()
    {
        // Arrange
        var entity1 = new TestEntityInt { Id = 3 };
        var entity2 = new TestEntityInt { Id = 3 };
        var entity3 = new TestEntityInt { Id = 4 };
        var set = new HashSet<TestEntityInt>();

        // Act
        set.Add(entity1);
        set.Add(entity2); // Should not add duplicate
        set.Add(entity3);

        // Assert
        Assert.Equal(2, set.Count);
    }

    #endregion

    #region Entity<Guid> Tests

    [Fact]
    public void Entity_Guid_WithIdSet_ReturnsCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new TestEntityGuid { Id = guid };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(guid, id);
    }

    [Fact]
    public void Entity_GuidNewInstance_HasDefaultId()
    {
        // Arrange & Act
        var entity = new TestEntityGuid();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entity_TwoGuidInstancesWithSameId_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity1 = new TestEntityGuid { Id = guid };
        var entity2 = new TestEntityGuid { Id = guid };

        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.False(entity1 != entity2);
    }

    [Fact]
    public void Entity_TwoGuidInstancesWithDifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityGuid { Id = Guid.NewGuid() };
        var entity2 = new TestEntityGuid { Id = Guid.NewGuid() };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    #endregion

    #region Entity<string> Tests

    [Fact]
    public void Entity_String_WithIdSet_ReturnsCorrectId()
    {
        // Arrange
        var entity = new TestEntityString { Id = "CODE123" };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal("CODE123", id);
    }

    [Fact]
    public void Entity_TwoStringInstancesWithSameId_AreEqual()
    {
        // Arrange
        var entity1 = new TestEntityString { Id = "ABC" };
        var entity2 = new TestEntityString { Id = "ABC" };

        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void Entity_TwoStringInstancesWithDifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityString { Id = "ABC" };
        var entity2 = new TestEntityString { Id = "XYZ" };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
    }

    #endregion

    #region Equality and Reference Tests

    [Fact]
    public void Entity_EqualsNull_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntityInt { Id = 1 };

        // Act & Assert
        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Entity_EqualsObjectOfDifferentType_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntityInt { Id = 1 };
        var obj = new object();

        // Act & Assert
        Assert.False(entity.Equals(obj));
    }

    [Fact]
    public void Entity_EqualsEntityOfDifferentGenericType_ReturnsFalse()
    {
        // Arrange
        var intEntity = new TestEntityInt { Id = 1 };
        var guidEntity = new TestEntityGuid { Id = Guid.Empty };

        // Act & Assert
        Assert.False(intEntity.Equals((object)guidEntity));
    }

    [Fact]
    public void Entity_ReferenceSameInstance_AreEqual()
    {
        // Arrange
        var entity = new TestEntityInt { Id = 5 };

        // Act & Assert
        Assert.Equal(entity, entity);
        Assert.True(entity == entity);
    }

    [Fact]
    public void Entity_IdEqualityIgnoresOtherProperties()
    {
        // Arrange
        var entity1 = new TestEntityInt { Id = 1, Name = "Alice" };
        var entity2 = new TestEntityInt { Id = 1, Name = "Bob" };

        // Act & Assert - Entities with same Id are equal regardless of other properties
        Assert.Equal(entity1, entity2);
    }

    #endregion

    #region Convenience Entity Tests

    [Fact]
    public void Entity_ConvenienceClass_InheritsFromEntityInt()
    {
        // Arrange & Act
        var entity = new TestEntity { Id = 100 };

        // Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.Equal(100, entity.Id);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test entity with int primary key.
    /// </summary>
    private class TestEntityInt : Entity<int>
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity with Guid primary key.
    /// </summary>
    private class TestEntityGuid : Entity<Guid>
    {
    }

    /// <summary>
    /// Test entity with string primary key.
    /// </summary>
    private class TestEntityString : Entity<string>
    {
    }

    /// <summary>
    /// Test convenience entity inheriting from Entity.
    /// </summary>
    private class TestEntity : Entity
    {
    }

    #endregion
}
