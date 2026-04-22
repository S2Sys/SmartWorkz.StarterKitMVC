using SmartWorkz.Core;
using SmartWorkz.Core.Helpers;

namespace SmartWorkz.Core.Tests.Helpers;

/// <summary>
/// Tests for StateGroups helper class - validates state collection management
/// using master/subset pattern across 15 entity type categories.
/// </summary>
public class StateGroupsTests
{
    #region AllValidStates Tests

    [Fact]
    public void AllValidStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.AllValidStates);
    }

    [Fact]
    public void AllValidStates_ContainsAllEntityStateValues()
    {
        // Act - count all valid EntityState enum values (0-63)
        var allEnumValues = Enum.GetValues(typeof(EntityState))
            .Cast<EntityState>()
            .ToList();

        // Assert - AllValidStates should contain all 64 states
        Assert.Equal(64, allEnumValues.Count);
        foreach (var state in allEnumValues)
        {
            Assert.Contains(state, StateGroups.AllValidStates);
        }
    }

    [Fact]
    public void AllValidStates_HasCorrectCount()
    {
        // Act & Assert - verify we have all 64 states
        Assert.Equal(64, StateGroups.AllValidStates.Count);
    }

    #endregion

    #region LifecycleStates Tests

    [Fact]
    public void LifecycleStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.LifecycleStates);
    }

    [Fact]
    public void LifecycleStates_IsNonEmpty()
    {
        // Act & Assert
        Assert.NotEmpty(StateGroups.LifecycleStates);
    }

    [Fact]
    public void LifecycleStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[] { EntityState.Active, EntityState.Inactive, EntityState.Archived, EntityState.Deleted };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.LifecycleStates);
        }
    }

    [Fact]
    public void LifecycleStates_HasCorrectCount()
    {
        // Act & Assert
        Assert.Equal(4, StateGroups.LifecycleStates.Count);
    }

    #endregion

    #region VerificationStates Tests

    [Fact]
    public void VerificationStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.VerificationStates);
    }

    [Fact]
    public void VerificationStates_IsNonEmpty()
    {
        // Act & Assert
        Assert.NotEmpty(StateGroups.VerificationStates);
    }

    [Fact]
    public void VerificationStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.EmailVerified,
            EntityState.EmailPending,
            EntityState.PhoneVerified,
            EntityState.PhonePending
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.VerificationStates);
        }
    }

    #endregion

    #region ApprovalStates Tests

    [Fact]
    public void ApprovalStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.ApprovalStates);
    }

    [Fact]
    public void ApprovalStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.PendingApproval,
            EntityState.Approved,
            EntityState.Rejected
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.ApprovalStates);
        }
    }

    #endregion

    #region DocumentStates Tests

    [Fact]
    public void DocumentStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.DocumentStates);
    }

    [Fact]
    public void DocumentStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.Draft,
            EntityState.Submitted,
            EntityState.InReview,
            EntityState.Completed
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.DocumentStates);
        }
    }

    #endregion

    #region SecurityStates Tests

    [Fact]
    public void SecurityStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.SecurityStates);
    }

    [Fact]
    public void SecurityStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.PasswordReset,
            EntityState.PasswordResetting,
            EntityState.Locked,
            EntityState.Suspended
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.SecurityStates);
        }
    }

    #endregion

    #region PaymentStates Tests

    [Fact]
    public void PaymentStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.PaymentStates);
    }

    [Fact]
    public void PaymentStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.PaymentPending,
            EntityState.PaymentReceived,
            EntityState.PaymentFailed
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.PaymentStates);
        }
    }

    #endregion

    #region OrderStates Tests

    [Fact]
    public void OrderStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.OrderStates);
    }

    [Fact]
    public void OrderStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.OrderPlaced,
            EntityState.OrderConfirmed,
            EntityState.Shipped,
            EntityState.Delivered,
            EntityState.Returned,
            EntityState.Cancelled
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.OrderStates);
        }
    }

    #endregion

    #region InventoryStates Tests

    [Fact]
    public void InventoryStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.InventoryStates);
    }

    [Fact]
    public void InventoryStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.OutOfStock,
            EntityState.LowStock,
            EntityState.Restocking,
            EntityState.BackOrder
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.InventoryStates);
        }
    }

    #endregion

    #region CommunicationStates Tests

    [Fact]
    public void CommunicationStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.CommunicationStates);
    }

    [Fact]
    public void CommunicationStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.Read,
            EntityState.Unread,
            EntityState.Forwarded,
            EntityState.Replied
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.CommunicationStates);
        }
    }

    #endregion

    #region SubscriptionStates Tests

    [Fact]
    public void SubscriptionStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.SubscriptionStates);
    }

    [Fact]
    public void SubscriptionStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.Trial,
            EntityState.SubscriptionActive,
            EntityState.Paused,
            EntityState.SubscriptionCancelled,
            EntityState.Expired
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.SubscriptionStates);
        }
    }

    #endregion

    #region UserAccountStates Tests

    [Fact]
    public void UserAccountStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.UserAccountStates);
    }

    [Fact]
    public void UserAccountStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.EmailVerificationPending,
            EntityState.PhoneVerificationPending,
            EntityState.KYCPending,
            EntityState.KYCApproved,
            EntityState.KYCRejected,
            EntityState.AccountSuspended
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.UserAccountStates);
        }
    }

    #endregion

    #region ReturnRefundStates Tests

    [Fact]
    public void ReturnRefundStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.ReturnRefundStates);
    }

    [Fact]
    public void ReturnRefundStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.ReturnRequested,
            EntityState.ReturnApproved,
            EntityState.ReturnRejected,
            EntityState.ReturnInProgress,
            EntityState.ReturnCompleted
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.ReturnRefundStates);
        }
    }

    #endregion

    #region TaskJobStates Tests

    [Fact]
    public void TaskJobStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.TaskJobStates);
    }

    [Fact]
    public void TaskJobStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.Assigned,
            EntityState.InProgress,
            EntityState.OnHold,
            EntityState.TaskCompleted,
            EntityState.Failed
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.TaskJobStates);
        }
    }

    #endregion

    #region ReviewRatingStates Tests

    [Fact]
    public void ReviewRatingStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.ReviewRatingStates);
    }

    [Fact]
    public void ReviewRatingStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.PendingReview,
            EntityState.UnderReview,
            EntityState.ReviewApproved,
            EntityState.ReviewRejected
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.ReviewRatingStates);
        }
    }

    #endregion

    #region ShippingStates Tests

    [Fact]
    public void ShippingStates_IsNotNull()
    {
        // Act & Assert
        Assert.NotNull(StateGroups.ShippingStates);
    }

    [Fact]
    public void ShippingStates_ContainsExpectedValues()
    {
        // Arrange
        var expected = new[]
        {
            EntityState.InTransit,
            EntityState.OutForDelivery,
            EntityState.DeliveredShipping
        };

        // Act & Assert
        foreach (var state in expected)
        {
            Assert.Contains(state, StateGroups.ShippingStates);
        }
    }

    #endregion

    #region Master/Subset Pattern Validation Tests

    [Fact]
    public void AllStateCollections_AreSubsetsOfAllValidStates()
    {
        // Arrange
        var collections = new[]
        {
            StateGroups.LifecycleStates,
            StateGroups.VerificationStates,
            StateGroups.ApprovalStates,
            StateGroups.DocumentStates,
            StateGroups.SecurityStates,
            StateGroups.PaymentStates,
            StateGroups.OrderStates,
            StateGroups.InventoryStates,
            StateGroups.CommunicationStates,
            StateGroups.SubscriptionStates,
            StateGroups.UserAccountStates,
            StateGroups.ReturnRefundStates,
            StateGroups.TaskJobStates,
            StateGroups.ReviewRatingStates,
            StateGroups.ShippingStates
        };

        // Act & Assert
        foreach (var collection in collections)
        {
            foreach (var state in collection)
            {
                Assert.Contains(state, StateGroups.AllValidStates);
            }
        }
    }

    [Fact]
    public void AllStateCollections_ContainNoInvalidStates()
    {
        // Arrange
        var validEnumValues = Enum.GetValues(typeof(EntityState))
            .Cast<EntityState>()
            .ToHashSet();

        var collections = new[]
        {
            StateGroups.LifecycleStates,
            StateGroups.VerificationStates,
            StateGroups.ApprovalStates,
            StateGroups.DocumentStates,
            StateGroups.SecurityStates,
            StateGroups.PaymentStates,
            StateGroups.OrderStates,
            StateGroups.InventoryStates,
            StateGroups.CommunicationStates,
            StateGroups.SubscriptionStates,
            StateGroups.UserAccountStates,
            StateGroups.ReturnRefundStates,
            StateGroups.TaskJobStates,
            StateGroups.ReviewRatingStates,
            StateGroups.ShippingStates
        };

        // Act & Assert
        foreach (var collection in collections)
        {
            foreach (var state in collection)
            {
                Assert.True(validEnumValues.Contains(state), $"State {state} is not a valid EntityState value");
            }
        }
    }

    [Fact]
    public void StateCollections_AreReadOnly()
    {
        // Act & Assert - IReadOnlySet interface confirms immutability (no Add/Remove methods available)
        Assert.IsAssignableFrom<IReadOnlySet<EntityState>>(StateGroups.LifecycleStates);
        Assert.IsAssignableFrom<IReadOnlySet<EntityState>>(StateGroups.OrderStates);
        Assert.IsAssignableFrom<IReadOnlySet<EntityState>>(StateGroups.AllValidStates);
    }

    #endregion

    #region GetValidStatesFor Tests

    [Fact]
    public void GetValidStatesFor_UserType_ReturnsUserAccountStates()
    {
        // Arrange - use User type (name matches the mapping in StateGroups)
        var userType = typeof(User);

        // Act
        var result = StateGroups.GetValidStatesFor(userType);

        // Assert
        Assert.Equal(StateGroups.UserAccountStates, result);
        Assert.Contains(EntityState.EmailVerificationPending, result);
        Assert.Contains(EntityState.KYCApproved, result);
    }

    [Fact]
    public void GetValidStatesFor_OrderType_ReturnsOrderStates()
    {
        // Arrange - use Order type (name matches the mapping in StateGroups)
        var orderType = typeof(Order);

        // Act
        var result = StateGroups.GetValidStatesFor(orderType);

        // Assert
        Assert.Equal(StateGroups.OrderStates, result);
        Assert.Contains(EntityState.OrderPlaced, result);
        Assert.Contains(EntityState.Shipped, result);
    }

    [Fact]
    public void GetValidStatesFor_ProductType_ReturnsInventoryStates()
    {
        // Arrange - use Product type (name matches the mapping in StateGroups)
        var productType = typeof(Product);

        // Act
        var result = StateGroups.GetValidStatesFor(productType);

        // Assert
        Assert.Equal(StateGroups.InventoryStates, result);
        Assert.Contains(EntityState.OutOfStock, result);
        Assert.Contains(EntityState.LowStock, result);
    }

    [Fact]
    public void GetValidStatesFor_UnknownEntityType_ThrowsArgumentException()
    {
        // Arrange
        var unknownType = typeof(Random);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => StateGroups.GetValidStatesFor(unknownType));
    }

    [Fact]
    public void GetValidStatesFor_NullType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StateGroups.GetValidStatesFor(null!));
    }

    [Fact]
    public void GetValidStatesFor_TypeWithTaskInName_ReturnsTaskJobStates()
    {
        // Arrange - use TaskEntity type which has "Task" in its name
        var taskType = typeof(TaskEntity);

        // Act
        var result = StateGroups.GetValidStatesFor(taskType);

        // Assert
        Assert.Equal(StateGroups.TaskJobStates, result);
        Assert.Contains(EntityState.Assigned, result);
        Assert.Contains(EntityState.TaskCompleted, result);
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void AllStateCollections_NoOverlappingDuplicates()
    {
        // Arrange
        var collections = new[]
        {
            StateGroups.LifecycleStates,
            StateGroups.VerificationStates,
            StateGroups.ApprovalStates,
            StateGroups.DocumentStates,
            StateGroups.SecurityStates,
            StateGroups.PaymentStates,
            StateGroups.OrderStates,
            StateGroups.InventoryStates,
            StateGroups.CommunicationStates,
            StateGroups.SubscriptionStates,
            StateGroups.UserAccountStates,
            StateGroups.ReturnRefundStates,
            StateGroups.TaskJobStates,
            StateGroups.ReviewRatingStates,
            StateGroups.ShippingStates
        };

        var seenStates = new HashSet<EntityState>();

        // Act & Assert - verify no state appears in multiple collections
        foreach (var collection in collections)
        {
            foreach (var state in collection)
            {
                Assert.False(seenStates.Contains(state),
                    $"State {state} appears in multiple collections - violates master/subset pattern");
                seenStates.Add(state);
            }
        }
    }

    [Fact]
    public void AllValidStates_MatchesSumOfAllCollections()
    {
        // Arrange
        var collections = new[]
        {
            StateGroups.LifecycleStates,
            StateGroups.VerificationStates,
            StateGroups.ApprovalStates,
            StateGroups.DocumentStates,
            StateGroups.SecurityStates,
            StateGroups.PaymentStates,
            StateGroups.OrderStates,
            StateGroups.InventoryStates,
            StateGroups.CommunicationStates,
            StateGroups.SubscriptionStates,
            StateGroups.UserAccountStates,
            StateGroups.ReturnRefundStates,
            StateGroups.TaskJobStates,
            StateGroups.ReviewRatingStates,
            StateGroups.ShippingStates
        };

        var combinedStates = new HashSet<EntityState>();

        // Act
        foreach (var collection in collections)
        {
            foreach (var state in collection)
            {
                combinedStates.Add(state);
            }
        }

        // Assert
        Assert.Equal(StateGroups.AllValidStates.Count, combinedStates.Count);
        foreach (var state in combinedStates)
        {
            Assert.Contains(state, StateGroups.AllValidStates);
        }
    }

    #endregion
}

/// <summary>
/// Mock type with name "User" for testing User -> UserAccountStates mapping.
/// </summary>
internal class User
{
}

/// <summary>
/// Mock type with name "Order" for testing Order -> OrderStates mapping.
/// </summary>
internal class Order
{
}

/// <summary>
/// Mock type with name "Product" for testing Product -> InventoryStates mapping.
/// </summary>
internal class Product
{
}

/// <summary>
/// Mock type with "Task" in name for testing type-based state mapping.
/// </summary>
internal class TaskEntity
{
}
