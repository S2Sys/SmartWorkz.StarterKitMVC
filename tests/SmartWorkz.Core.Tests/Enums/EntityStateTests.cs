using System.ComponentModel.DataAnnotations;
using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Enums;

/// <summary>
/// Tests for EntityState enum - comprehensive state management across multiple categories.
/// </summary>
public class EntityStateTests
{
    #region Lifecycle Category Tests

    [Fact]
    public void LifecycleCategory_Active_HasCorrectValue()
    {
        // Assert
        Assert.Equal(0, (int)EntityState.Active);
    }

    [Fact]
    public void LifecycleCategory_Inactive_HasCorrectValue()
    {
        // Assert
        Assert.Equal(1, (int)EntityState.Inactive);
    }

    [Fact]
    public void LifecycleCategory_Archived_HasCorrectValue()
    {
        // Assert
        Assert.Equal(2, (int)EntityState.Archived);
    }

    [Fact]
    public void LifecycleCategory_Deleted_HasCorrectValue()
    {
        // Assert
        Assert.Equal(3, (int)EntityState.Deleted);
    }

    #endregion

    #region Verification Category Tests

    [Fact]
    public void VerificationCategory_EmailVerified_HasCorrectValue()
    {
        // Assert
        Assert.Equal(4, (int)EntityState.EmailVerified);
    }

    [Fact]
    public void VerificationCategory_EmailPending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(5, (int)EntityState.EmailPending);
    }

    [Fact]
    public void VerificationCategory_PhoneVerified_HasCorrectValue()
    {
        // Assert
        Assert.Equal(6, (int)EntityState.PhoneVerified);
    }

    [Fact]
    public void VerificationCategory_PhonePending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(7, (int)EntityState.PhonePending);
    }

    #endregion

    #region Approval Category Tests

    [Fact]
    public void ApprovalCategory_PendingApproval_HasCorrectValue()
    {
        // Assert
        Assert.Equal(8, (int)EntityState.PendingApproval);
    }

    [Fact]
    public void ApprovalCategory_Approved_HasCorrectValue()
    {
        // Assert
        Assert.Equal(9, (int)EntityState.Approved);
    }

    [Fact]
    public void ApprovalCategory_Rejected_HasCorrectValue()
    {
        // Assert
        Assert.Equal(10, (int)EntityState.Rejected);
    }

    #endregion

    #region Document Category Tests

    [Fact]
    public void DocumentCategory_Draft_HasCorrectValue()
    {
        // Assert
        Assert.Equal(11, (int)EntityState.Draft);
    }

    [Fact]
    public void DocumentCategory_Submitted_HasCorrectValue()
    {
        // Assert
        Assert.Equal(12, (int)EntityState.Submitted);
    }

    [Fact]
    public void DocumentCategory_InReview_HasCorrectValue()
    {
        // Assert
        Assert.Equal(13, (int)EntityState.InReview);
    }

    [Fact]
    public void DocumentCategory_Completed_HasCorrectValue()
    {
        // Assert
        Assert.Equal(14, (int)EntityState.Completed);
    }

    #endregion

    #region Security Category Tests

    [Fact]
    public void SecurityCategory_PasswordReset_HasCorrectValue()
    {
        // Assert
        Assert.Equal(15, (int)EntityState.PasswordReset);
    }

    [Fact]
    public void SecurityCategory_PasswordResetting_HasCorrectValue()
    {
        // Assert
        Assert.Equal(16, (int)EntityState.PasswordResetting);
    }

    [Fact]
    public void SecurityCategory_Locked_HasCorrectValue()
    {
        // Assert
        Assert.Equal(17, (int)EntityState.Locked);
    }

    [Fact]
    public void SecurityCategory_Suspended_HasCorrectValue()
    {
        // Assert
        Assert.Equal(18, (int)EntityState.Suspended);
    }

    #endregion

    #region Payment Category Tests

    [Fact]
    public void PaymentCategory_PaymentPending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(19, (int)EntityState.PaymentPending);
    }

    [Fact]
    public void PaymentCategory_PaymentReceived_HasCorrectValue()
    {
        // Assert
        Assert.Equal(20, (int)EntityState.PaymentReceived);
    }

    [Fact]
    public void PaymentCategory_PaymentFailed_HasCorrectValue()
    {
        // Assert
        Assert.Equal(21, (int)EntityState.PaymentFailed);
    }

    #endregion

    #region Order Category Tests

    [Fact]
    public void OrderCategory_OrderPlaced_HasCorrectValue()
    {
        // Assert
        Assert.Equal(22, (int)EntityState.OrderPlaced);
    }

    [Fact]
    public void OrderCategory_OrderConfirmed_HasCorrectValue()
    {
        // Assert
        Assert.Equal(23, (int)EntityState.OrderConfirmed);
    }

    [Fact]
    public void OrderCategory_Shipped_HasCorrectValue()
    {
        // Assert
        Assert.Equal(24, (int)EntityState.Shipped);
    }

    [Fact]
    public void OrderCategory_Delivered_HasCorrectValue()
    {
        // Assert
        Assert.Equal(25, (int)EntityState.Delivered);
    }

    [Fact]
    public void OrderCategory_Returned_HasCorrectValue()
    {
        // Assert
        Assert.Equal(26, (int)EntityState.Returned);
    }

    [Fact]
    public void OrderCategory_Cancelled_HasCorrectValue()
    {
        // Assert
        Assert.Equal(27, (int)EntityState.Cancelled);
    }

    #endregion

    #region Inventory/Stock Category Tests

    [Fact]
    public void InventoryCategory_OutOfStock_HasCorrectValue()
    {
        // Assert
        Assert.Equal(28, (int)EntityState.OutOfStock);
    }

    [Fact]
    public void InventoryCategory_LowStock_HasCorrectValue()
    {
        // Assert
        Assert.Equal(29, (int)EntityState.LowStock);
    }

    [Fact]
    public void InventoryCategory_Restocking_HasCorrectValue()
    {
        // Assert
        Assert.Equal(30, (int)EntityState.Restocking);
    }

    [Fact]
    public void InventoryCategory_BackOrder_HasCorrectValue()
    {
        // Assert
        Assert.Equal(31, (int)EntityState.BackOrder);
    }

    #endregion

    #region Communication Category Tests

    [Fact]
    public void CommunicationCategory_Read_HasCorrectValue()
    {
        // Assert
        Assert.Equal(32, (int)EntityState.Read);
    }

    [Fact]
    public void CommunicationCategory_Unread_HasCorrectValue()
    {
        // Assert
        Assert.Equal(33, (int)EntityState.Unread);
    }

    [Fact]
    public void CommunicationCategory_Forwarded_HasCorrectValue()
    {
        // Assert
        Assert.Equal(34, (int)EntityState.Forwarded);
    }

    [Fact]
    public void CommunicationCategory_Replied_HasCorrectValue()
    {
        // Assert
        Assert.Equal(35, (int)EntityState.Replied);
    }

    #endregion

    #region Subscription Category Tests

    [Fact]
    public void SubscriptionCategory_Trial_HasCorrectValue()
    {
        // Assert
        Assert.Equal(36, (int)EntityState.Trial);
    }

    [Fact]
    public void SubscriptionCategory_Active_HasCorrectValue()
    {
        // Assert
        Assert.Equal(37, (int)EntityState.SubscriptionActive);
    }

    [Fact]
    public void SubscriptionCategory_Paused_HasCorrectValue()
    {
        // Assert
        Assert.Equal(38, (int)EntityState.Paused);
    }

    [Fact]
    public void SubscriptionCategory_Cancelled_HasCorrectValue()
    {
        // Assert
        Assert.Equal(39, (int)EntityState.SubscriptionCancelled);
    }

    [Fact]
    public void SubscriptionCategory_Expired_HasCorrectValue()
    {
        // Assert
        Assert.Equal(40, (int)EntityState.Expired);
    }

    #endregion

    #region User Account Category Tests

    [Fact]
    public void UserAccountCategory_EmailVerificationPending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(41, (int)EntityState.EmailVerificationPending);
    }

    [Fact]
    public void UserAccountCategory_PhoneVerificationPending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(42, (int)EntityState.PhoneVerificationPending);
    }

    [Fact]
    public void UserAccountCategory_KYCPending_HasCorrectValue()
    {
        // Assert
        Assert.Equal(43, (int)EntityState.KYCPending);
    }

    [Fact]
    public void UserAccountCategory_KYCApproved_HasCorrectValue()
    {
        // Assert
        Assert.Equal(44, (int)EntityState.KYCApproved);
    }

    [Fact]
    public void UserAccountCategory_KYCRejected_HasCorrectValue()
    {
        // Assert
        Assert.Equal(45, (int)EntityState.KYCRejected);
    }

    [Fact]
    public void UserAccountCategory_AccountSuspended_HasCorrectValue()
    {
        // Assert
        Assert.Equal(46, (int)EntityState.AccountSuspended);
    }

    #endregion

    #region Return/Refund Category Tests

    [Fact]
    public void ReturnCategory_ReturnRequested_HasCorrectValue()
    {
        // Assert
        Assert.Equal(47, (int)EntityState.ReturnRequested);
    }

    [Fact]
    public void ReturnCategory_ReturnApproved_HasCorrectValue()
    {
        // Assert
        Assert.Equal(48, (int)EntityState.ReturnApproved);
    }

    [Fact]
    public void ReturnCategory_ReturnRejected_HasCorrectValue()
    {
        // Assert
        Assert.Equal(49, (int)EntityState.ReturnRejected);
    }

    [Fact]
    public void ReturnCategory_ReturnInProgress_HasCorrectValue()
    {
        // Assert
        Assert.Equal(50, (int)EntityState.ReturnInProgress);
    }

    [Fact]
    public void ReturnCategory_ReturnCompleted_HasCorrectValue()
    {
        // Assert
        Assert.Equal(51, (int)EntityState.ReturnCompleted);
    }

    #endregion

    #region Task/Job Category Tests

    [Fact]
    public void TaskCategory_Assigned_HasCorrectValue()
    {
        // Assert
        Assert.Equal(52, (int)EntityState.Assigned);
    }

    [Fact]
    public void TaskCategory_InProgress_HasCorrectValue()
    {
        // Assert
        Assert.Equal(53, (int)EntityState.InProgress);
    }

    [Fact]
    public void TaskCategory_OnHold_HasCorrectValue()
    {
        // Assert
        Assert.Equal(54, (int)EntityState.OnHold);
    }

    [Fact]
    public void TaskCategory_Completed_HasCorrectValue()
    {
        // Assert
        Assert.Equal(55, (int)EntityState.TaskCompleted);
    }

    [Fact]
    public void TaskCategory_Failed_HasCorrectValue()
    {
        // Assert
        Assert.Equal(56, (int)EntityState.Failed);
    }

    #endregion

    #region Review/Rating Category Tests

    [Fact]
    public void ReviewCategory_PendingReview_HasCorrectValue()
    {
        // Assert
        Assert.Equal(57, (int)EntityState.PendingReview);
    }

    [Fact]
    public void ReviewCategory_UnderReview_HasCorrectValue()
    {
        // Assert
        Assert.Equal(58, (int)EntityState.UnderReview);
    }

    [Fact]
    public void ReviewCategory_ReviewApproved_HasCorrectValue()
    {
        // Assert
        Assert.Equal(59, (int)EntityState.ReviewApproved);
    }

    [Fact]
    public void ReviewCategory_ReviewRejected_HasCorrectValue()
    {
        // Assert
        Assert.Equal(60, (int)EntityState.ReviewRejected);
    }

    #endregion

    #region Shipping Status Category Tests

    [Fact]
    public void ShippingCategory_InTransit_HasCorrectValue()
    {
        // Assert
        Assert.Equal(61, (int)EntityState.InTransit);
    }

    [Fact]
    public void ShippingCategory_OutForDelivery_HasCorrectValue()
    {
        // Assert
        Assert.Equal(62, (int)EntityState.OutForDelivery);
    }

    [Fact]
    public void ShippingCategory_Delivered_HasCorrectValue()
    {
        // Assert
        Assert.Equal(63, (int)EntityState.DeliveredShipping);
    }

    #endregion

    #region Display Attribute Tests

    [Theory]
    [InlineData(EntityState.Active, "Active")]
    [InlineData(EntityState.Inactive, "Inactive")]
    [InlineData(EntityState.Archived, "Archived")]
    [InlineData(EntityState.Deleted, "Deleted")]
    [InlineData(EntityState.EmailVerified, "Email Verified")]
    [InlineData(EntityState.EmailPending, "Email Pending")]
    [InlineData(EntityState.PhoneVerified, "Phone Verified")]
    [InlineData(EntityState.PhonePending, "Phone Pending")]
    [InlineData(EntityState.PendingApproval, "Pending Approval")]
    [InlineData(EntityState.Approved, "Approved")]
    [InlineData(EntityState.Rejected, "Rejected")]
    [InlineData(EntityState.Draft, "Draft")]
    [InlineData(EntityState.Submitted, "Submitted")]
    [InlineData(EntityState.InReview, "In Review")]
    [InlineData(EntityState.Completed, "Completed")]
    [InlineData(EntityState.PasswordReset, "Password Reset")]
    [InlineData(EntityState.PasswordResetting, "Password Resetting")]
    [InlineData(EntityState.Locked, "Locked")]
    [InlineData(EntityState.Suspended, "Suspended")]
    [InlineData(EntityState.PaymentPending, "Payment Pending")]
    [InlineData(EntityState.PaymentReceived, "Payment Received")]
    [InlineData(EntityState.PaymentFailed, "Payment Failed")]
    [InlineData(EntityState.OrderPlaced, "Order Placed")]
    [InlineData(EntityState.OrderConfirmed, "Order Confirmed")]
    [InlineData(EntityState.Shipped, "Shipped")]
    [InlineData(EntityState.Delivered, "Delivered")]
    [InlineData(EntityState.Returned, "Returned")]
    [InlineData(EntityState.Cancelled, "Cancelled")]
    [InlineData(EntityState.OutOfStock, "Out Of Stock")]
    [InlineData(EntityState.LowStock, "Low Stock")]
    [InlineData(EntityState.Restocking, "Restocking")]
    [InlineData(EntityState.BackOrder, "Back Order")]
    [InlineData(EntityState.Read, "Read")]
    [InlineData(EntityState.Unread, "Unread")]
    [InlineData(EntityState.Forwarded, "Forwarded")]
    [InlineData(EntityState.Replied, "Replied")]
    [InlineData(EntityState.Trial, "Trial")]
    [InlineData(EntityState.SubscriptionActive, "Active")]
    [InlineData(EntityState.Paused, "Paused")]
    [InlineData(EntityState.SubscriptionCancelled, "Subscription Cancelled")]
    [InlineData(EntityState.Expired, "Expired")]
    [InlineData(EntityState.EmailVerificationPending, "Email Verification Pending")]
    [InlineData(EntityState.PhoneVerificationPending, "Phone Verification Pending")]
    [InlineData(EntityState.KYCPending, "KYC Pending")]
    [InlineData(EntityState.KYCApproved, "KYC Approved")]
    [InlineData(EntityState.KYCRejected, "KYC Rejected")]
    [InlineData(EntityState.AccountSuspended, "Account Suspended")]
    [InlineData(EntityState.ReturnRequested, "Return Requested")]
    [InlineData(EntityState.ReturnApproved, "Return Approved")]
    [InlineData(EntityState.ReturnRejected, "Return Rejected")]
    [InlineData(EntityState.ReturnInProgress, "Return In Progress")]
    [InlineData(EntityState.ReturnCompleted, "Return Completed")]
    [InlineData(EntityState.Assigned, "Assigned")]
    [InlineData(EntityState.InProgress, "In Progress")]
    [InlineData(EntityState.OnHold, "On Hold")]
    [InlineData(EntityState.TaskCompleted, "Task Completed")]
    [InlineData(EntityState.Failed, "Failed")]
    [InlineData(EntityState.PendingReview, "Pending Review")]
    [InlineData(EntityState.UnderReview, "Under Review")]
    [InlineData(EntityState.ReviewApproved, "Review Approved")]
    [InlineData(EntityState.ReviewRejected, "Review Rejected")]
    [InlineData(EntityState.InTransit, "In Transit")]
    [InlineData(EntityState.OutForDelivery, "Out For Delivery")]
    [InlineData(EntityState.DeliveredShipping, "Delivered")]
    public void DisplayAttribute_ReturnsCorrectFriendlyName(EntityState state, string expectedName)
    {
        // Act
        var displayAttribute = typeof(EntityState)
            .GetMember(state.ToString())
            .First()
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        // Assert
        Assert.NotNull(displayAttribute);
        Assert.Equal(expectedName, displayAttribute!.Name);
    }

    #endregion

    #region Enum Definition Tests

    [Fact]
    public void EntityState_IsEnum()
    {
        // Assert
        Assert.True(typeof(EntityState).IsEnum);
    }

    [Fact]
    public void EntityState_HasMinimumStateCount()
    {
        // Act - EntityState should have 60+ values
        var stateCount = Enum.GetValues(typeof(EntityState)).Length;

        // Assert
        Assert.True(stateCount >= 60, $"EntityState should have 60+ values, but has {stateCount}");
    }

    [Fact]
    public void EntityState_AllValuesHaveDisplayAttribute()
    {
        // Act
        var fieldsWithoutDisplay = typeof(EntityState)
            .GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.MemberType == System.Reflection.MemberTypes.Field)
            .Where(m => !m.GetCustomAttributes(typeof(DisplayAttribute), false).Any())
            .ToList();

        // Assert
        Assert.Empty(fieldsWithoutDisplay);
    }

    [Fact]
    public void EntityState_CanParseStringValues()
    {
        // Act & Assert - Test parsing for representative values
        Assert.Equal(EntityState.Active, Enum.Parse<EntityState>("Active"));
        Assert.Equal(EntityState.Inactive, Enum.Parse<EntityState>("Inactive"));
        Assert.Equal(EntityState.EmailVerified, Enum.Parse<EntityState>("EmailVerified"));
        Assert.Equal(EntityState.PendingApproval, Enum.Parse<EntityState>("PendingApproval"));
        Assert.Equal(EntityState.Draft, Enum.Parse<EntityState>("Draft"));
        Assert.Equal(EntityState.PasswordReset, Enum.Parse<EntityState>("PasswordReset"));
        Assert.Equal(EntityState.PaymentPending, Enum.Parse<EntityState>("PaymentPending"));
        Assert.Equal(EntityState.OrderPlaced, Enum.Parse<EntityState>("OrderPlaced"));
    }

    [Fact]
    public void EntityState_SequentialIntegerValues()
    {
        // Act - Get all enum values and their integer equivalents
        var values = Enum.GetValues(typeof(EntityState)).Cast<EntityState>().ToList();

        // Assert - Values should be sequential starting from 0
        for (int i = 0; i < values.Count; i++)
        {
            Assert.Equal(i, (int)values[i]);
        }
    }

    #endregion
}
