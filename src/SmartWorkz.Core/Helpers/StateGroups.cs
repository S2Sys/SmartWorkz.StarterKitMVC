namespace SmartWorkz.Core.Helpers;

/// <summary>
/// Helper class that manages valid state collections per entity type using a master/subset pattern.
///
/// MASTER/SUBSET PATTERN:
/// This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
/// Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
/// related states for a specific domain category or entity type.
///
/// Benefits:
/// - Single source of truth prevents state value inconsistencies
/// - Type safety: states are strongly typed as EntityState enums
/// - Immutable collections: returned as IReadOnlySet to prevent external modification
/// - Easy validation: can verify entity states against correct subset for type
/// - Clear organization: states grouped by business domain/entity category
///
/// Usage:
/// 1. Get all valid states: StateGroups.AllValidStates
/// 2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
/// 3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
/// 4. Validate state: StateGroups.OrderStates.Contains(someState)
/// </summary>
public static class StateGroups
{
    /// <summary>
    /// Master collection of all 64 valid states across all entity types and categories.
    /// This is the single source of truth for valid EntityState values.
    ///
    /// Contains states from all 15 categories:
    /// - Lifecycle (4 states): Active, Inactive, Archived, Deleted
    /// - Verification (4 states): EmailVerified, EmailPending, PhoneVerified, PhonePending
    /// - Approval (3 states): PendingApproval, Approved, Rejected
    /// - Document (4 states): Draft, Submitted, InReview, Completed
    /// - Security (4 states): PasswordReset, PasswordResetting, Locked, Suspended
    /// - Payment (3 states): PaymentPending, PaymentReceived, PaymentFailed
    /// - Order (6 states): OrderPlaced, OrderConfirmed, Shipped, Delivered, Returned, Cancelled
    /// - Inventory (4 states): OutOfStock, LowStock, Restocking, BackOrder
    /// - Communication (4 states): Read, Unread, Forwarded, Replied
    /// - Subscription (5 states): Trial, SubscriptionActive, Paused, SubscriptionCancelled, Expired
    /// - User Account (6 states): EmailVerificationPending, PhoneVerificationPending, KYCPending, KYCApproved, KYCRejected, AccountSuspended
    /// - Return/Refund (5 states): ReturnRequested, ReturnApproved, ReturnRejected, ReturnInProgress, ReturnCompleted
    /// - Task/Job (5 states): Assigned, InProgress, OnHold, TaskCompleted, Failed
    /// - Review/Rating (4 states): PendingReview, UnderReview, ReviewApproved, ReviewRejected
    /// - Shipping (3 states): InTransit, OutForDelivery, DeliveredShipping
    /// </summary>
    public static readonly IReadOnlySet<EntityState> AllValidStates;

    /// <summary>
    /// Lifecycle states - fundamental entity lifecycle management.
    /// Used to track basic entity presence and visibility in the system.
    ///
    /// States: Active, Inactive, Archived, Deleted
    ///
    /// Typical progression:
    /// Active -> Inactive -> Archived (or Active -> Deleted)
    /// </summary>
    public static readonly IReadOnlySet<EntityState> LifecycleStates;

    /// <summary>
    /// Verification states - email and phone verification tracking.
    /// Used to manage identity verification workflows.
    ///
    /// States: EmailVerified, EmailPending, PhoneVerified, PhonePending
    /// </summary>
    public static readonly IReadOnlySet<EntityState> VerificationStates;

    /// <summary>
    /// Approval states - entity approval and authorization workflows.
    /// Used when entities require approval from authorized personnel before proceeding.
    ///
    /// States: PendingApproval, Approved, Rejected
    /// </summary>
    public static readonly IReadOnlySet<EntityState> ApprovalStates;

    /// <summary>
    /// Document states - document lifecycle and processing workflow.
    /// Used to track document creation, submission, review, and completion.
    ///
    /// States: Draft, Submitted, InReview, Completed
    /// </summary>
    public static readonly IReadOnlySet<EntityState> DocumentStates;

    /// <summary>
    /// Security states - account security and access control.
    /// Used to manage account locks, suspensions, and password resets.
    ///
    /// States: PasswordReset, PasswordResetting, Locked, Suspended
    /// </summary>
    public static readonly IReadOnlySet<EntityState> SecurityStates;

    /// <summary>
    /// Payment states - payment processing workflow.
    /// Used to track payment initiation, confirmation, and failure.
    ///
    /// States: PaymentPending, PaymentReceived, PaymentFailed
    /// </summary>
    public static readonly IReadOnlySet<EntityState> PaymentStates;

    /// <summary>
    /// Order states - complete order lifecycle from placement to delivery/return.
    /// Used for e-commerce order tracking.
    ///
    /// States: OrderPlaced, OrderConfirmed, Shipped, Delivered, Returned, Cancelled
    /// </summary>
    public static readonly IReadOnlySet<EntityState> OrderStates;

    /// <summary>
    /// Inventory/Stock states - product stock level and restocking management.
    /// Used to track inventory availability and restocking operations.
    ///
    /// States: OutOfStock, LowStock, Restocking, BackOrder
    /// </summary>
    public static readonly IReadOnlySet<EntityState> InventoryStates;

    /// <summary>
    /// Communication states - message and notification status tracking.
    /// Used to track message read/unread status and forwarding.
    ///
    /// States: Read, Unread, Forwarded, Replied
    /// </summary>
    public static readonly IReadOnlySet<EntityState> CommunicationStates;

    /// <summary>
    /// Subscription states - subscription lifecycle and billing status.
    /// Used for SaaS subscription management.
    ///
    /// States: Trial, SubscriptionActive, Paused, SubscriptionCancelled, Expired
    /// </summary>
    public static readonly IReadOnlySet<EntityState> SubscriptionStates;

    /// <summary>
    /// User account states - comprehensive user account status tracking.
    /// Includes verification, KYC (Know Your Customer), and suspension states.
    ///
    /// States: EmailVerificationPending, PhoneVerificationPending, KYCPending, KYCApproved, KYCRejected, AccountSuspended
    /// </summary>
    public static readonly IReadOnlySet<EntityState> UserAccountStates;

    /// <summary>
    /// Return/Refund states - return merchandise authorization and refund processing.
    /// Used for managing customer returns and refunds.
    ///
    /// States: ReturnRequested, ReturnApproved, ReturnRejected, ReturnInProgress, ReturnCompleted
    /// </summary>
    public static readonly IReadOnlySet<EntityState> ReturnRefundStates;

    /// <summary>
    /// Task/Job states - task assignment and execution workflow.
    /// Used for project management and job queue status tracking.
    ///
    /// States: Assigned, InProgress, OnHold, TaskCompleted, Failed
    /// </summary>
    public static readonly IReadOnlySet<EntityState> TaskJobStates;

    /// <summary>
    /// Review/Rating states - user-generated content moderation workflow.
    /// Used for managing customer reviews and ratings approval process.
    ///
    /// States: PendingReview, UnderReview, ReviewApproved, ReviewRejected
    /// </summary>
    public static readonly IReadOnlySet<EntityState> ReviewRatingStates;

    /// <summary>
    /// Shipping states - order shipping and delivery status.
    /// Used for tracking order shipping progress.
    ///
    /// States: InTransit, OutForDelivery, DeliveredShipping
    /// </summary>
    public static readonly IReadOnlySet<EntityState> ShippingStates;

    /// <summary>
    /// Cached mapping of entity type names to their corresponding state collections.
    /// Initialized once in the static constructor for efficient O(1) lookups.
    /// </summary>
    private static readonly Dictionary<string, IReadOnlySet<EntityState>> TypeToStatesMap;

    /// <summary>
    /// Static constructor initializes all state collections.
    /// All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
    /// </summary>
    static StateGroups()
    {
        // Initialize LifecycleStates (4 states)
        var lifecycleSet = new HashSet<EntityState>
        {
            EntityState.Active,
            EntityState.Inactive,
            EntityState.Archived,
            EntityState.Deleted
        };
        LifecycleStates = lifecycleSet;

        // Initialize VerificationStates (4 states)
        var verificationSet = new HashSet<EntityState>
        {
            EntityState.EmailVerified,
            EntityState.EmailPending,
            EntityState.PhoneVerified,
            EntityState.PhonePending
        };
        VerificationStates = verificationSet;

        // Initialize ApprovalStates (3 states)
        var approvalSet = new HashSet<EntityState>
        {
            EntityState.PendingApproval,
            EntityState.Approved,
            EntityState.Rejected
        };
        ApprovalStates = approvalSet;

        // Initialize DocumentStates (4 states)
        var documentSet = new HashSet<EntityState>
        {
            EntityState.Draft,
            EntityState.Submitted,
            EntityState.InReview,
            EntityState.Completed
        };
        DocumentStates = documentSet;

        // Initialize SecurityStates (4 states)
        var securitySet = new HashSet<EntityState>
        {
            EntityState.PasswordReset,
            EntityState.PasswordResetting,
            EntityState.Locked,
            EntityState.Suspended
        };
        SecurityStates = securitySet;

        // Initialize PaymentStates (3 states)
        var paymentSet = new HashSet<EntityState>
        {
            EntityState.PaymentPending,
            EntityState.PaymentReceived,
            EntityState.PaymentFailed
        };
        PaymentStates = paymentSet;

        // Initialize OrderStates (6 states)
        var orderSet = new HashSet<EntityState>
        {
            EntityState.OrderPlaced,
            EntityState.OrderConfirmed,
            EntityState.Shipped,
            EntityState.Delivered,
            EntityState.Returned,
            EntityState.Cancelled
        };
        OrderStates = orderSet;

        // Initialize InventoryStates (4 states)
        var inventorySet = new HashSet<EntityState>
        {
            EntityState.OutOfStock,
            EntityState.LowStock,
            EntityState.Restocking,
            EntityState.BackOrder
        };
        InventoryStates = inventorySet;

        // Initialize CommunicationStates (4 states)
        var communicationSet = new HashSet<EntityState>
        {
            EntityState.Read,
            EntityState.Unread,
            EntityState.Forwarded,
            EntityState.Replied
        };
        CommunicationStates = communicationSet;

        // Initialize SubscriptionStates (5 states)
        var subscriptionSet = new HashSet<EntityState>
        {
            EntityState.Trial,
            EntityState.SubscriptionActive,
            EntityState.Paused,
            EntityState.SubscriptionCancelled,
            EntityState.Expired
        };
        SubscriptionStates = subscriptionSet;

        // Initialize UserAccountStates (6 states)
        var userAccountSet = new HashSet<EntityState>
        {
            EntityState.EmailVerificationPending,
            EntityState.PhoneVerificationPending,
            EntityState.KYCPending,
            EntityState.KYCApproved,
            EntityState.KYCRejected,
            EntityState.AccountSuspended
        };
        UserAccountStates = userAccountSet;

        // Initialize ReturnRefundStates (5 states)
        var returnRefundSet = new HashSet<EntityState>
        {
            EntityState.ReturnRequested,
            EntityState.ReturnApproved,
            EntityState.ReturnRejected,
            EntityState.ReturnInProgress,
            EntityState.ReturnCompleted
        };
        ReturnRefundStates = returnRefundSet;

        // Initialize TaskJobStates (5 states)
        var taskJobSet = new HashSet<EntityState>
        {
            EntityState.Assigned,
            EntityState.InProgress,
            EntityState.OnHold,
            EntityState.TaskCompleted,
            EntityState.Failed
        };
        TaskJobStates = taskJobSet;

        // Initialize ReviewRatingStates (4 states)
        var reviewRatingSet = new HashSet<EntityState>
        {
            EntityState.PendingReview,
            EntityState.UnderReview,
            EntityState.ReviewApproved,
            EntityState.ReviewRejected
        };
        ReviewRatingStates = reviewRatingSet;

        // Initialize ShippingStates (3 states)
        var shippingSet = new HashSet<EntityState>
        {
            EntityState.InTransit,
            EntityState.OutForDelivery,
            EntityState.DeliveredShipping
        };
        ShippingStates = shippingSet;

        // Initialize AllValidStates as master collection combining all subsets
        // Total: 4 + 4 + 3 + 4 + 4 + 3 + 6 + 4 + 4 + 5 + 6 + 5 + 5 + 4 + 3 = 64 states
        var allSet = new HashSet<EntityState>
        {
            // Lifecycle (4)
            EntityState.Active,
            EntityState.Inactive,
            EntityState.Archived,
            EntityState.Deleted,
            // Verification (4)
            EntityState.EmailVerified,
            EntityState.EmailPending,
            EntityState.PhoneVerified,
            EntityState.PhonePending,
            // Approval (3)
            EntityState.PendingApproval,
            EntityState.Approved,
            EntityState.Rejected,
            // Document (4)
            EntityState.Draft,
            EntityState.Submitted,
            EntityState.InReview,
            EntityState.Completed,
            // Security (4)
            EntityState.PasswordReset,
            EntityState.PasswordResetting,
            EntityState.Locked,
            EntityState.Suspended,
            // Payment (3)
            EntityState.PaymentPending,
            EntityState.PaymentReceived,
            EntityState.PaymentFailed,
            // Order (6)
            EntityState.OrderPlaced,
            EntityState.OrderConfirmed,
            EntityState.Shipped,
            EntityState.Delivered,
            EntityState.Returned,
            EntityState.Cancelled,
            // Inventory (4)
            EntityState.OutOfStock,
            EntityState.LowStock,
            EntityState.Restocking,
            EntityState.BackOrder,
            // Communication (4)
            EntityState.Read,
            EntityState.Unread,
            EntityState.Forwarded,
            EntityState.Replied,
            // Subscription (5)
            EntityState.Trial,
            EntityState.SubscriptionActive,
            EntityState.Paused,
            EntityState.SubscriptionCancelled,
            EntityState.Expired,
            // User Account (6)
            EntityState.EmailVerificationPending,
            EntityState.PhoneVerificationPending,
            EntityState.KYCPending,
            EntityState.KYCApproved,
            EntityState.KYCRejected,
            EntityState.AccountSuspended,
            // Return/Refund (5)
            EntityState.ReturnRequested,
            EntityState.ReturnApproved,
            EntityState.ReturnRejected,
            EntityState.ReturnInProgress,
            EntityState.ReturnCompleted,
            // Task/Job (5)
            EntityState.Assigned,
            EntityState.InProgress,
            EntityState.OnHold,
            EntityState.TaskCompleted,
            EntityState.Failed,
            // Review/Rating (4)
            EntityState.PendingReview,
            EntityState.UnderReview,
            EntityState.ReviewApproved,
            EntityState.ReviewRejected,
            // Shipping (3)
            EntityState.InTransit,
            EntityState.OutForDelivery,
            EntityState.DeliveredShipping
        };
        AllValidStates = allSet;

        // Initialize TypeToStatesMap for efficient entity type to state collection lookups
        // This dictionary is created once and reused, avoiding allocation on every GetValidStatesFor call
        TypeToStatesMap = new Dictionary<string, IReadOnlySet<EntityState>>
        {
            // Entity types that map to specific state categories
            { "User", UserAccountStates },
            { "ApplicationUser", UserAccountStates },
            { "Order", OrderStates },
            { "Product", InventoryStates },
            { "Subscription", SubscriptionStates },
            { "Payment", PaymentStates },
            { "Document", DocumentStates },
            { "Review", ReviewRatingStates },
            { "Rating", ReviewRatingStates },
            { "ShipmentTracking", ShippingStates },
            { "Shipment", ShippingStates },
            { "ReturnRequest", ReturnRefundStates },
            { "Refund", ReturnRefundStates },
            { "Message", CommunicationStates },
            { "Notification", CommunicationStates },
            { "Email", VerificationStates },
            { "PhoneNumber", VerificationStates },
            { "Account", LifecycleStates },
            { "Task", TaskJobStates },
            { "TaskEntity", TaskJobStates },
            { "Job", TaskJobStates }
        };
    }

    /// <summary>
    /// Gets the valid state subset for a specific entity type.
    ///
    /// Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
    /// This provides O(1) lookup performance without reflection or repeated allocation.
    ///
    /// Supported entity type mappings:
    /// - User, ApplicationUser -> UserAccountStates
    /// - Order -> OrderStates
    /// - Product -> InventoryStates
    /// - Task, Job -> TaskJobStates
    /// - Subscription -> SubscriptionStates
    /// - And others (see TypeToStatesMap for complete list)
    /// </summary>
    /// <param name="entityType">The entity type to get states for</param>
    /// <returns>IReadOnlySet of valid EntityState values for the given entity type</returns>
    /// <exception cref="ArgumentNullException">Thrown if entityType is null</exception>
    /// <exception cref="ArgumentException">Thrown if entityType is not recognized</exception>
    public static IReadOnlySet<EntityState> GetValidStatesFor(Type entityType)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType), "Entity type cannot be null");

        var typeName = entityType.Name;

        // Use the pre-cached dictionary for O(1) lookup
        if (TypeToStatesMap.TryGetValue(typeName, out var states))
            return states;

        // If no match found, throw ArgumentException
        throw new ArgumentException(
            $"Entity type '{typeName}' is not recognized. " +
            $"Supported types include: User, ApplicationUser, Order, Product, Subscription, Payment, " +
            $"Document, Review, Rating, ShipmentTracking, Shipment, ReturnRequest, Refund, Message, " +
            $"Notification, Email, PhoneNumber, Account, Task, TaskEntity, and Job. " +
            $"Consider adding a mapping for '{typeName}' in StateGroups.TypeToStatesMap.",
            nameof(entityType));
    }
}
