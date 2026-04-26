using SmartWorkz.Mobile.Diagnostics;

namespace SmartWorkz.Mobile.Tests.Diagnostics;

public class CrashContextTests
{
    [Fact]
    public void Constructor_WithRequiredParameters_InitializesProperties()
    {
        // Arrange
        var userId = "test-user-123";
        var sessionId = "session-456";
        var appVersion = "1.0.0";
        var environment = "Development";

        // Act
        var context = new CrashContext(userId, sessionId, appVersion, environment);

        // Assert
        Assert.Equal(userId, context.UserId);
        Assert.Equal(sessionId, context.SessionId);
        Assert.Equal(appVersion, context.AppVersion);
        Assert.Equal(environment, context.Environment);
        Assert.NotNull(context.CustomData);
        Assert.Empty(context.CustomData);
        Assert.NotNull(context.Tags);
        Assert.Empty(context.Tags);
        Assert.NotNull(context.Extra);
        Assert.Empty(context.Extra);
    }

    [Fact]
    public void CustomData_CanAddAndRetrieveData()
    {
        // Arrange
        var context = new CrashContext("user-123", "session-456", "1.0.0", "Production");
        var key = "test-key";
        var value = (object)"test-value";

        // Act
        context.CustomData[key] = value;

        // Assert
        Assert.True(context.CustomData.ContainsKey(key));
        Assert.Equal(value, context.CustomData[key]);
    }

    [Fact]
    public void Tags_CanAddAndRetrieveTags()
    {
        // Arrange
        var context = new CrashContext("user-123", "session-456", "1.0.0", "Staging");
        var tagKey = "feature";
        var tagValue = "payment";

        // Act
        context.Tags[tagKey] = tagValue;

        // Assert
        Assert.True(context.Tags.ContainsKey(tagKey));
        Assert.Equal(tagValue, context.Tags[tagKey]);
    }

    [Fact]
    public void Extra_CanAddAndRetrieveExtra()
    {
        // Arrange
        var context = new CrashContext("user-123", "session-456", "1.0.0", "Development");
        var extraKey = "stackTrace";
        var extraValue = (object)"some.Function() -> other.Function()";

        // Act
        context.Extra[extraKey] = extraValue;

        // Assert
        Assert.True(context.Extra.ContainsKey(extraKey));
        Assert.Equal(extraValue, context.Extra[extraKey]);
    }

    [Fact]
    public void Constructor_WithNullValues_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CrashContext(null!, "session", "1.0", "Development"));
        Assert.Throws<ArgumentNullException>(() => new CrashContext("user", null!, "1.0", "Development"));
        Assert.Throws<ArgumentNullException>(() => new CrashContext("user", "session", null!, "Development"));
        Assert.Throws<ArgumentNullException>(() => new CrashContext("user", "session", "1.0", null!));
    }

    [Fact]
    public void Constructor_WithEmptyStrings_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CrashContext("", "session", "1.0", "Development"));
        Assert.Throws<ArgumentException>(() => new CrashContext("user", "", "1.0", "Development"));
        Assert.Throws<ArgumentException>(() => new CrashContext("user", "session", "", "Development"));
        Assert.Throws<ArgumentException>(() => new CrashContext("user", "session", "1.0", ""));
    }
}

public class ICrashReportingServiceTests
{
    [Fact]
    public async Task InitializeAsync_WithValidDsn_SetsInitializedFlag()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var dsn = "https://test@sentry.io/123456";
        var environment = "Development";

        // Act
        await service.InitializeAsync(dsn, environment);

        // Assert
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var dsn = "https://test@sentry.io/123456";
        var environment = "Development";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.InitializeAsync(dsn, environment, cts.Token));
    }

    [Fact]
    public async Task SetUserAsync_WithValidUser_StoresUserContext()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var userId = "user-123";
        var email = "user@example.com";

        // Act
        await service.SetUserAsync(userId, email);

        // Assert
        Assert.Equal(userId, service.CurrentUserId);
        Assert.Equal(email, service.CurrentUserEmail);
    }

    [Fact]
    public async Task SetContextAsync_WithValidContext_StoresContext()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var context = new CrashContext("user-123", "session-456", "1.0.0", "Production");

        // Act
        await service.SetContextAsync(context);

        // Assert
        Assert.NotNull(service.CurrentContext);
        Assert.Equal(context.UserId, service.CurrentContext.UserId);
    }

    [Fact]
    public async Task AddBreadcrumbAsync_WithMessage_AddsBreadcrumb()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var message = "User clicked button";
        var category = "ui.click";
        var level = "info";

        // Act
        await service.AddBreadcrumbAsync(message, category, level);

        // Assert
        Assert.Contains(message, service.Breadcrumbs.Select(b => b.Message));
    }

    [Fact]
    public async Task CaptureExceptionAsync_WithException_CapturesAndReturnsTaskCompletion()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        await service.CaptureExceptionAsync(exception);
        Assert.Contains(exception.Message, service.CapturedErrors);
    }

    [Fact]
    public async Task CaptureMessageAsync_WithMessage_CapturesMessage()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var message = "Important event occurred";
        var level = "warning";

        // Act
        await service.CaptureMessageAsync(message, level);

        // Assert
        Assert.Contains(message, service.CapturedMessages);
    }

    [Fact]
    public async Task CaptureExceptionAsync_WithContextAndException_CapturesWithContext()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var exception = new InvalidOperationException("Test exception");
        var context = new CrashContext("user-123", "session-456", "1.0.0", "Production");

        // Act
        await service.CaptureExceptionAsync(exception, context);

        // Assert
        Assert.Contains(exception.Message, service.CapturedErrors);
        Assert.NotNull(service.ContextForLastError);
        Assert.Equal(context.UserId, service.ContextForLastError.UserId);
    }

    [Fact]
    public async Task CaptureMessageAsync_WithContextAndMessage_CapturesWithContext()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var message = "Important event occurred";
        var level = "warning";
        var context = new CrashContext("user-456", "session-789", "2.0.0", "Staging");

        // Act
        await service.CaptureMessageAsync(message, level, context);

        // Assert
        Assert.Contains(message, service.CapturedMessages);
        Assert.NotNull(service.ContextForLastMessage);
        Assert.Equal(context.UserId, service.ContextForLastMessage.UserId);
    }

    [Fact]
    public async Task MultipleOperations_WithCancellationToken_RespectCancellation()
    {
        // Arrange
        var service = new MockCrashReportingService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.SetUserAsync("user", "email@test.com", cts.Token));
    }
}

public class BreadcrumbLoggerTests
{
    [Fact]
    public void AddBreadcrumb_WithMessage_StoresBreadcrumb()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();
        var message = "User navigated to home";
        var category = "navigation";
        var level = "info";

        // Act
        logger.AddBreadcrumb(message, category, level);

        // Assert
        Assert.NotEmpty(logger.Breadcrumbs);
        var breadcrumb = logger.Breadcrumbs.First();
        Assert.Equal(message, breadcrumb.Message);
        Assert.Equal(category, breadcrumb.Category);
        Assert.Equal(level, breadcrumb.Level);
    }

    [Fact]
    public void AddBreadcrumb_MultipleCalls_MaintainsOrder()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();

        // Act
        logger.AddBreadcrumb("First", "test", "info");
        logger.AddBreadcrumb("Second", "test", "info");
        logger.AddBreadcrumb("Third", "test", "info");

        // Assert
        Assert.Equal(3, logger.Breadcrumbs.Count);
        Assert.Equal("First", logger.Breadcrumbs[0].Message);
        Assert.Equal("Second", logger.Breadcrumbs[1].Message);
        Assert.Equal("Third", logger.Breadcrumbs[2].Message);
    }

    [Fact]
    public void AddBreadcrumb_ExceededMaximum_RemovesOldest()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger(maxBreadcrumbs: 3);

        // Act
        logger.AddBreadcrumb("First", "test", "info");
        logger.AddBreadcrumb("Second", "test", "info");
        logger.AddBreadcrumb("Third", "test", "info");
        logger.AddBreadcrumb("Fourth", "test", "info");

        // Assert
        Assert.Equal(3, logger.Breadcrumbs.Count);
        Assert.Equal("Second", logger.Breadcrumbs[0].Message);
        Assert.Equal("Fourth", logger.Breadcrumbs[2].Message);
    }

    [Fact]
    public void GetBreadcrumbs_ReturnsAllBreadcrumbs()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();
        logger.AddBreadcrumb("Breadcrumb 1", "test", "info");
        logger.AddBreadcrumb("Breadcrumb 2", "test", "warning");

        // Act
        var breadcrumbs = logger.GetBreadcrumbs();

        // Assert
        Assert.Equal(2, breadcrumbs.Count);
    }

    [Fact]
    public void Clear_RemovesAllBreadcrumbs()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();
        logger.AddBreadcrumb("Breadcrumb 1", "test", "info");
        logger.AddBreadcrumb("Breadcrumb 2", "test", "warning");

        // Act
        logger.Clear();

        // Assert
        Assert.Empty(logger.GetBreadcrumbs());
    }

    [Fact]
    public void AddBreadcrumb_TimestampsAreRecorded()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();
        var beforeTime = DateTime.UtcNow;

        // Act
        logger.AddBreadcrumb("Test", "test", "info");
        var afterTime = DateTime.UtcNow;

        // Assert
        var breadcrumb = logger.Breadcrumbs.First();
        Assert.True(breadcrumb.Timestamp >= beforeTime && breadcrumb.Timestamp <= afterTime);
    }

    [Fact]
    public void AddBreadcrumb_IsThreadSafe()
    {
        // Arrange
        var logger = new MockBreadcrumbLogger();
        var tasks = new Task[100];

        // Act
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
                logger.AddBreadcrumb($"Breadcrumb {index}", "test", "info"));
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.Equal(100, logger.Breadcrumbs.Count);
    }
}

// Mock implementations for testing
public class MockCrashReportingService : ICrashReportingService
{
    public bool IsInitialized { get; private set; }
    public string? CurrentUserId { get; private set; }
    public string? CurrentUserEmail { get; private set; }
    public CrashContext? CurrentContext { get; private set; }
    public CrashContext? ContextForLastError { get; private set; }
    public CrashContext? ContextForLastMessage { get; private set; }
    public List<string> CapturedErrors { get; } = new();
    public List<string> CapturedMessages { get; } = new();
    public List<(string Message, string Category, string Level)> Breadcrumbs { get; } = new();

    public Task InitializeAsync(string dsn, string environment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsInitialized = true;
        return Task.CompletedTask;
    }

    public Task SetUserAsync(string userId, string? email = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentUserId = userId;
        CurrentUserEmail = email;
        return Task.CompletedTask;
    }

    public Task SetContextAsync(CrashContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentContext = context;
        return Task.CompletedTask;
    }

    public Task AddBreadcrumbAsync(string message, string category, string level, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Breadcrumbs.Add((message, category, level));
        return Task.CompletedTask;
    }

    public Task CaptureExceptionAsync(Exception exception, CrashContext? context = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CapturedErrors.Add(exception.Message);
        ContextForLastError = context;
        return Task.CompletedTask;
    }

    public Task CaptureMessageAsync(string message, string level, CrashContext? context = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CapturedMessages.Add(message);
        ContextForLastMessage = context;
        return Task.CompletedTask;
    }
}

public class MockBreadcrumb
{
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class MockBreadcrumbLogger
{
    private readonly List<MockBreadcrumb> _breadcrumbs = new();
    private readonly int _maxBreadcrumbs;
    private readonly object _lock = new();

    public IReadOnlyList<MockBreadcrumb> Breadcrumbs => _breadcrumbs.AsReadOnly();

    public MockBreadcrumbLogger(int maxBreadcrumbs = 50)
    {
        _maxBreadcrumbs = maxBreadcrumbs;
    }

    public void AddBreadcrumb(string message, string category, string level)
    {
        lock (_lock)
        {
            var breadcrumb = new MockBreadcrumb
            {
                Message = message,
                Category = category,
                Level = level,
                Timestamp = DateTime.UtcNow
            };

            _breadcrumbs.Add(breadcrumb);

            if (_breadcrumbs.Count > _maxBreadcrumbs)
            {
                _breadcrumbs.RemoveAt(0);
            }
        }
    }

    public IReadOnlyList<MockBreadcrumb> GetBreadcrumbs()
    {
        lock (_lock)
        {
            return _breadcrumbs.ToList().AsReadOnly();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _breadcrumbs.Clear();
        }
    }
}
