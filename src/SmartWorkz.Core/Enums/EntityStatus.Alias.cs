namespace SmartWorkz.Core;

/// <summary>
/// [OBSOLETE] Type alias shim for backward compatibility.
/// EntityStatus has been replaced by EntityState enum.
/// Use EntityState instead — it provides the same functionality with 64 comprehensive states
/// across 15 categories (lifecycle, verification, approval, document, security, payment,
/// order, inventory, communication, subscription, user account, return/refund, task/job,
/// review/rating, shipping).
/// </summary>
/// <remarks>
/// Migration Path:
/// - Old code: EntityStatus.Active → New code: EntityState.Active
/// - No breaking changes: EntityStatus is a type alias to EntityState
/// - All existing EntityStatus references will compile without modification
/// - Update your code at your convenience to use EntityState directly
///
/// Removal Timeline:
/// - v1.x: EntityStatus available as [Obsolete] type alias
/// - v2.0: EntityStatus will be removed
///
/// EntityState Overview:
/// EntityState provides 64 states organized in 15 categories:
///
/// 1. LIFECYCLE (0-3): Active, Inactive, Archived, Deleted
/// 2. VERIFICATION (4-7): EmailVerified, EmailPending, PhoneVerified, PhonePending
/// 3. APPROVAL (8-10): PendingApproval, Approved, Rejected
/// 4. DOCUMENT (11-14): Draft, Submitted, InReview, Completed
/// 5. SECURITY (15-18): PasswordReset, PasswordResetting, Locked, Suspended
/// 6. PAYMENT (19-21): PaymentPending, PaymentReceived, PaymentFailed
/// 7. ORDER (22-27): OrderPlaced, OrderConfirmed, Shipped, Delivered, Returned, Cancelled
/// 8. INVENTORY (28-31): OutOfStock, LowStock, Restocking, BackOrder
/// 9. COMMUNICATION (32-35): Read, Unread, Forwarded, Replied
/// 10. SUBSCRIPTION (36-40): Trial, SubscriptionActive, Paused, SubscriptionCancelled, Expired
/// 11. USER ACCOUNT (41-46): EmailVerificationPending, PhoneVerificationPending, KYCPending, KYCApproved, KYCRejected, AccountSuspended
/// 12. RETURN/REFUND (47-51): ReturnRequested, ReturnApproved, ReturnRejected, ReturnInProgress, ReturnCompleted
/// 13. TASK/JOB (52-56): Assigned, InProgress, OnHold, TaskCompleted, Failed
/// 14. REVIEW/RATING (57-60): PendingReview, UnderReview, ReviewApproved, ReviewRejected
/// 15. SHIPPING (61-63): InTransit, OutForDelivery, DeliveredShipping
/// </remarks>
[System.Obsolete("Use EntityState instead. EntityStatus is deprecated as of v1.0 and will be removed in v2.0.", false)]
public enum EntityStatus
{
    /// <summary>Active state — Entity is fully operational and visible in standard queries.</summary>
    Active = EntityState.Active,

    /// <summary>Inactive state — Entity is temporarily disabled but preserved for potential reactivation.</summary>
    Inactive = EntityState.Inactive,

    /// <summary>Archived state — Entity is retained for historical reference and audit purposes.</summary>
    Archived = EntityState.Archived,

    /// <summary>Deleted state — Soft-deleted entity marked for removal but physically retained.</summary>
    Deleted = EntityState.Deleted,

    /// <summary>EmailVerified state — Email address has been verified and authenticated.</summary>
    EmailVerified = EntityState.EmailVerified,

    /// <summary>EmailPending state — Email verification is awaiting confirmation.</summary>
    EmailPending = EntityState.EmailPending,

    /// <summary>PhoneVerified state — Phone number has been verified and authenticated.</summary>
    PhoneVerified = EntityState.PhoneVerified,

    /// <summary>PhonePending state — Phone verification is awaiting confirmation.</summary>
    PhonePending = EntityState.PhonePending,

    /// <summary>PendingApproval state — Entity is awaiting approval from authorized personnel.</summary>
    PendingApproval = EntityState.PendingApproval,

    /// <summary>Approved state — Entity has been approved and is authorized for use.</summary>
    Approved = EntityState.Approved,

    /// <summary>Rejected state — Entity has been rejected and requires remediation.</summary>
    Rejected = EntityState.Rejected,

    /// <summary>Draft state — Document is in draft state and not yet submitted.</summary>
    Draft = EntityState.Draft,

    /// <summary>Submitted state — Document has been submitted for processing.</summary>
    Submitted = EntityState.Submitted,

    /// <summary>InReview state — Document is currently under review.</summary>
    InReview = EntityState.InReview,

    /// <summary>Completed state — Document processing is complete.</summary>
    Completed = EntityState.Completed,

    /// <summary>PasswordReset state — Password reset process has been initiated.</summary>
    PasswordReset = EntityState.PasswordReset,

    /// <summary>PasswordResetting state — Password is currently being reset.</summary>
    PasswordResetting = EntityState.PasswordResetting,

    /// <summary>Locked state — Account is locked due to security concerns.</summary>
    Locked = EntityState.Locked,

    /// <summary>Suspended state — Account is suspended, preventing normal operations.</summary>
    Suspended = EntityState.Suspended,

    /// <summary>PaymentPending state — Payment is awaiting processing.</summary>
    PaymentPending = EntityState.PaymentPending,

    /// <summary>PaymentReceived state — Payment has been successfully received.</summary>
    PaymentReceived = EntityState.PaymentReceived,

    /// <summary>PaymentFailed state — Payment processing has failed.</summary>
    PaymentFailed = EntityState.PaymentFailed,

    /// <summary>OrderPlaced state — Order has been placed.</summary>
    OrderPlaced = EntityState.OrderPlaced,

    /// <summary>OrderConfirmed state — Order has been confirmed.</summary>
    OrderConfirmed = EntityState.OrderConfirmed,

    /// <summary>Shipped state — Order has been shipped.</summary>
    Shipped = EntityState.Shipped,

    /// <summary>Delivered state — Order has been delivered.</summary>
    Delivered = EntityState.Delivered,

    /// <summary>Returned state — Order has been returned.</summary>
    Returned = EntityState.Returned,

    /// <summary>Cancelled state — Order has been cancelled.</summary>
    Cancelled = EntityState.Cancelled,

    /// <summary>OutOfStock state — Item is out of stock and unavailable for purchase.</summary>
    OutOfStock = EntityState.OutOfStock,

    /// <summary>LowStock state — Item stock level is below minimum threshold.</summary>
    LowStock = EntityState.LowStock,

    /// <summary>Restocking state — Item is currently being restocked.</summary>
    Restocking = EntityState.Restocking,

    /// <summary>BackOrder state — Item is on back order pending availability.</summary>
    BackOrder = EntityState.BackOrder,

    /// <summary>Read state — Message has been read by recipient.</summary>
    Read = EntityState.Read,

    /// <summary>Unread state — Message is unread by recipient.</summary>
    Unread = EntityState.Unread,

    /// <summary>Forwarded state — Message has been forwarded to other recipients.</summary>
    Forwarded = EntityState.Forwarded,

    /// <summary>Replied state — Reply has been sent to the message.</summary>
    Replied = EntityState.Replied,

    /// <summary>Trial state — Subscription is in trial period.</summary>
    Trial = EntityState.Trial,

    /// <summary>SubscriptionActive state — Subscription is active and current.</summary>
    SubscriptionActive = EntityState.SubscriptionActive,

    /// <summary>Paused state — Subscription is paused but can be resumed.</summary>
    Paused = EntityState.Paused,

    /// <summary>SubscriptionCancelled state — Subscription has been cancelled.</summary>
    SubscriptionCancelled = EntityState.SubscriptionCancelled,

    /// <summary>Expired state — Subscription has expired.</summary>
    Expired = EntityState.Expired,

    /// <summary>EmailVerificationPending state — Email verification is pending.</summary>
    EmailVerificationPending = EntityState.EmailVerificationPending,

    /// <summary>PhoneVerificationPending state — Phone verification is pending.</summary>
    PhoneVerificationPending = EntityState.PhoneVerificationPending,

    /// <summary>KYCPending state — Know Your Customer (KYC) verification is pending.</summary>
    KYCPending = EntityState.KYCPending,

    /// <summary>KYCApproved state — KYC verification has been approved.</summary>
    KYCApproved = EntityState.KYCApproved,

    /// <summary>KYCRejected state — KYC verification has been rejected.</summary>
    KYCRejected = EntityState.KYCRejected,

    /// <summary>AccountSuspended state — Account has been suspended.</summary>
    AccountSuspended = EntityState.AccountSuspended,

    /// <summary>ReturnRequested state — Return has been requested by customer.</summary>
    ReturnRequested = EntityState.ReturnRequested,

    /// <summary>ReturnApproved state — Return has been approved.</summary>
    ReturnApproved = EntityState.ReturnApproved,

    /// <summary>ReturnRejected state — Return has been rejected.</summary>
    ReturnRejected = EntityState.ReturnRejected,

    /// <summary>ReturnInProgress state — Return is currently in progress.</summary>
    ReturnInProgress = EntityState.ReturnInProgress,

    /// <summary>ReturnCompleted state — Return has been completed.</summary>
    ReturnCompleted = EntityState.ReturnCompleted,

    /// <summary>Assigned state — Task has been assigned to a resource.</summary>
    Assigned = EntityState.Assigned,

    /// <summary>InProgress state — Task is currently being worked on.</summary>
    InProgress = EntityState.InProgress,

    /// <summary>OnHold state — Task is on hold pending action.</summary>
    OnHold = EntityState.OnHold,

    /// <summary>TaskCompleted state — Task has been completed.</summary>
    TaskCompleted = EntityState.TaskCompleted,

    /// <summary>Failed state — Task has failed.</summary>
    Failed = EntityState.Failed,

    /// <summary>PendingReview state — Review is awaiting approval.</summary>
    PendingReview = EntityState.PendingReview,

    /// <summary>UnderReview state — Review is currently under review.</summary>
    UnderReview = EntityState.UnderReview,

    /// <summary>ReviewApproved state — Review has been approved.</summary>
    ReviewApproved = EntityState.ReviewApproved,

    /// <summary>ReviewRejected state — Review has been rejected.</summary>
    ReviewRejected = EntityState.ReviewRejected,

    /// <summary>InTransit state — Order is in transit to destination.</summary>
    InTransit = EntityState.InTransit,

    /// <summary>OutForDelivery state — Order is out for delivery.</summary>
    OutForDelivery = EntityState.OutForDelivery,

    /// <summary>DeliveredShipping state — Order has been delivered.</summary>
    DeliveredShipping = EntityState.DeliveredShipping
}
