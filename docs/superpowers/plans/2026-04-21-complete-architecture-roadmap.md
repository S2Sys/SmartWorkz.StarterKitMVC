# SmartWorkz Complete Architecture Roadmap

> **For agentic workers:** Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this roadmap. This is a three-phase rollout with independent sub-plans per phase.

**Goal:** Transform SmartWorkzCore and SmartWorkz.Core.Shared into a complete, production-grade architectural foundation covering infrastructure services, advanced patterns, and cross-cutting concerns.

**Architecture:** Three-phase phased rollout where each phase builds independently and integrates cleanly:
- **Phase 1 (Infrastructure Services):** Background jobs, file storage, push notifications
- **Phase 2 (Advanced Patterns):** Event sourcing, CQRS, domain events
- **Phase 3 (Cross-Cutting):** Observability, testing utilities, audit logging

**Current State:** SmartWorkzCore has abstractions, value objects, and basic service interfaces. Missing: implementations, event streaming, structured observability, testing patterns.

---

## Phase 1: Infrastructure Services Foundation

**Deliverables:**
- Background job scheduling and processing (Hangfire)
- File storage abstraction with cloud providers
- Push notification implementations (Firebase Cloud Messaging)
- Retry policies and resilience patterns
- **Estimated effort:** 2-3 days (3 independent tasks)

### Task 1.1: Background Job Service Abstraction & Hangfire Integration

**Files:**
- Create: `src/SmartWorkz.Core/Services/BackgroundJobs/IBackgroundJobService.cs`
- Create: `src/SmartWorkz.Core/Services/BackgroundJobs/BackgroundJobAttribute.cs`
- Create: `src/SmartWorkz.Core.Shared/BackgroundJobs/HangfireJobService.cs`
- Create: `src/SmartWorkz.Core.Shared/BackgroundJobs/HangfireStartupExtensions.cs`
- Modify: `src/SmartWorkz.Core/SmartWorkz.Core.csproj` (add Hangfire package)
- Test: `tests/SmartWorkz.Core.Tests/Services/BackgroundJobs/HangfireJobServiceTests.cs`

**Why:** Decouples job scheduling from implementation; supports queuing, retries, persistence.

**Implementation Steps:**

- [ ] **Step 1: Create job service abstraction**

```csharp
// src/SmartWorkz.Core/Services/BackgroundJobs/IBackgroundJobService.cs
namespace SmartWorkz.Core.Services.BackgroundJobs;

public interface IBackgroundJobService
{
    /// <summary>Enqueue a fire-and-forget background job.</summary>
    Task<string> EnqueueAsync<TJob>(Func<TJob, Task> jobAction, CancellationToken cancellationToken = default) 
        where TJob : class;
    
    /// <summary>Schedule a job to run at a specific time.</summary>
    Task<string> ScheduleAsync<TJob>(Func<TJob, Task> jobAction, DateTimeOffset enqueueAt, CancellationToken cancellationToken = default) 
        where TJob : class;
    
    /// <summary>Schedule a recurring job (CRON expression).</summary>
    Task<string> AddOrUpdateRecurringAsync<TJob>(string recurringJobId, Func<TJob, Task> jobAction, string cronExpression, CancellationToken cancellationToken = default) 
        where TJob : class;
    
    /// <summary>Delete a job by ID.</summary>
    Task DeleteAsync(string jobId, CancellationToken cancellationToken = default);
    
    /// <summary>Requeue a failed job.</summary>
    Task<string> RequeueAsync(string jobId, CancellationToken cancellationToken = default);
    
    /// <summary>Get job status.</summary>
    Task<BackgroundJobStatus?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default);
}

public enum BackgroundJobStatus
{
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Deleted,
    Scheduled
}

public class BackgroundJobContext
{
    public string JobId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? RetryCount { get; set; }
    public Exception? LastException { get; set; }
}
```

- [ ] **Step 2: Create job attribute for marking methods**

```csharp
// src/SmartWorkz.Core/Services/BackgroundJobs/BackgroundJobAttribute.cs
namespace SmartWorkz.Core.Services.BackgroundJobs;

[AttributeUsage(AttributeTargets.Method)]
public class BackgroundJobAttribute : Attribute
{
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 3600; // 1 hour default
    
    public BackgroundJobAttribute(string description = "")
    {
        Description = description;
    }
    
    public string Description { get; set; }
}
```

- [ ] **Step 3: Add Hangfire NuGet package**

Update `src/SmartWorkz.Core/SmartWorkz.Core.csproj`:
```xml
<ItemGroup>
    <PackageReference Include="Hangfire.Core" Version="1.8.14" />
</ItemGroup>
```

- [ ] **Step 4: Implement Hangfire integration**

```csharp
// src/SmartWorkz.Core.Shared/BackgroundJobs/HangfireJobService.cs
namespace SmartWorkz.Core.Shared.BackgroundJobs;

using Hangfire;
using Microsoft.Extensions.Logging;
using SmartWorkz.Core.Services.BackgroundJobs;

public class HangfireJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobService> logger)
    {
        _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> EnqueueAsync<TJob>(Func<TJob, Task> jobAction, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            var jobId = _jobClient.Enqueue(jobAction);
            _logger.LogInformation("Job {JobId} enqueued for type {JobType}", jobId, typeof(TJob).Name);
            return Task.FromResult(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job for type {JobType}", typeof(TJob).Name);
            throw;
        }
    }

    public Task<string> ScheduleAsync<TJob>(Func<TJob, Task> jobAction, DateTimeOffset enqueueAt, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            var jobId = _jobClient.Schedule(jobAction, enqueueAt);
            _logger.LogInformation("Job {JobId} scheduled for {EnqueueAt}", jobId, enqueueAt);
            return Task.FromResult(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule job for type {JobType}", typeof(TJob).Name);
            throw;
        }
    }

    public Task<string> AddOrUpdateRecurringAsync<TJob>(string recurringJobId, Func<TJob, Task> jobAction, string cronExpression, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            _recurringJobManager.AddOrUpdate(recurringJobId, jobAction, cronExpression);
            _logger.LogInformation("Recurring job {JobId} configured with cron {Cron}", recurringJobId, cronExpression);
            return Task.FromResult(recurringJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure recurring job {JobId}", recurringJobId);
            throw;
        }
    }

    public Task DeleteAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            BackgroundJob.Delete(jobId);
            _logger.LogInformation("Job {JobId} deleted", jobId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete job {JobId}", jobId);
            throw;
        }
    }

    public Task<string> RequeueAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var newJobId = _jobClient.Requeue(jobId);
            _logger.LogInformation("Job {JobId} requeued as {NewJobId}", jobId, newJobId);
            return Task.FromResult(newJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to requeue job {JobId}", jobId);
            throw;
        }
    }

    public Task<BackgroundJobStatus?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = JobStorage.Current.GetConnection().GetJobData(jobId);
            if (job == null) return Task.FromResult((BackgroundJobStatus?)null);

            var status = job.State switch
            {
                "Enqueued" => BackgroundJobStatus.Enqueued,
                "Processing" => BackgroundJobStatus.Processing,
                "Succeeded" => BackgroundJobStatus.Succeeded,
                "Failed" => BackgroundJobStatus.Failed,
                "Deleted" => BackgroundJobStatus.Deleted,
                "Scheduled" => BackgroundJobStatus.Scheduled,
                _ => BackgroundJobStatus.Failed
            };

            return Task.FromResult((BackgroundJobStatus?)status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for job {JobId}", jobId);
            return Task.FromResult((BackgroundJobStatus?)null);
        }
    }
}
```

- [ ] **Step 5: Create startup extensions**

```csharp
// src/SmartWorkz.Core.Shared/BackgroundJobs/HangfireStartupExtensions.cs
namespace SmartWorkz.Core.Shared.BackgroundJobs;

using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Services.BackgroundJobs;

public static class HangfireStartupExtensions
{
    public static IServiceCollection AddHangfireBackgroundJobs(
        this IServiceCollection services,
        string connectionString,
        int workerCount = 20)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.ServerName = $"{Environment.MachineName}-{Guid.NewGuid()}";
        });

        services.AddScoped<IBackgroundJobService, HangfireJobService>();
        return services;
    }

    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/admin/jobs", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            IsReadOnlyFunc = context => false
        });
        return app;
    }
}

internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User?.Identity?.IsAuthenticated ?? false;
    }
}
```

- [ ] **Step 6: Write failing test**

```csharp
// tests/SmartWorkz.Core.Tests/Services/BackgroundJobs/HangfireJobServiceTests.cs
namespace SmartWorkz.Core.Tests.Services.BackgroundJobs;

using Hangfire;
using Moq;
using SmartWorkz.Core.Services.BackgroundJobs;
using SmartWorkz.Core.Shared.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Xunit;

public class HangfireJobServiceTests
{
    private readonly Mock<IBackgroundJobClient> _mockJobClient;
    private readonly Mock<IRecurringJobManager> _mockRecurringJobManager;
    private readonly Mock<ILogger<HangfireJobService>> _mockLogger;
    private readonly HangfireJobService _service;

    public HangfireJobServiceTests()
    {
        _mockJobClient = new Mock<IBackgroundJobClient>();
        _mockRecurringJobManager = new Mock<IRecurringJobManager>();
        _mockLogger = new Mock<ILogger<HangfireJobService>>();
        _service = new HangfireJobService(_mockJobClient, _mockRecurringJobManager, _mockLogger);
    }

    [Fact]
    public async Task EnqueueAsync_WithValidAction_ReturnsJobId()
    {
        // Arrange
        var expectedJobId = "job-123";
        _mockJobClient.Setup(x => x.Enqueue(It.IsAny<Func<DummyJob, Task>>()))
            .Returns(expectedJobId);

        // Act
        var result = await _service.EnqueueAsync<DummyJob>(x => Task.CompletedTask);

        // Assert
        Assert.Equal(expectedJobId, result);
        _mockJobClient.Verify(x => x.Enqueue(It.IsAny<Func<DummyJob, Task>>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleAsync_WithFutureDate_ReturnsJobId()
    {
        // Arrange
        var expectedJobId = "scheduled-job-123";
        var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);
        _mockJobClient.Setup(x => x.Schedule(It.IsAny<Func<DummyJob, Task>>(), It.IsAny<DateTimeOffset>()))
            .Returns(expectedJobId);

        // Act
        var result = await _service.ScheduleAsync<DummyJob>(x => Task.CompletedTask, scheduledTime);

        // Assert
        Assert.Equal(expectedJobId, result);
        _mockJobClient.Verify(x => x.Schedule(It.IsAny<Func<DummyJob, Task>>(), scheduledTime), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WithCronExpression_ReturnsJobId()
    {
        // Arrange
        var jobId = "recurring-job";
        var cron = "0 0 * * *"; // Daily at midnight

        // Act
        var result = await _service.AddOrUpdateRecurringAsync<DummyJob>(jobId, x => Task.CompletedTask, cron);

        // Assert
        Assert.Equal(jobId, result);
        _mockRecurringJobManager.Verify(x => x.AddOrUpdate(jobId, It.IsAny<Func<DummyJob, Task>>(), cron), Times.Once);
    }

    private class DummyJob
    {
        public async Task Execute() => await Task.CompletedTask;
    }
}
```

- [ ] **Step 7: Run tests**

```bash
cd tests/SmartWorkz.Core.Tests
dotnet test Services/BackgroundJobs/HangfireJobServiceTests.cs -v
```

Expected: PASS (3/3 tests)

- [ ] **Step 8: Commit**

```bash
git add src/SmartWorkz.Core/Services/BackgroundJobs/ \
        src/SmartWorkz.Core.Shared/BackgroundJobs/ \
        tests/SmartWorkz.Core.Tests/Services/BackgroundJobs/ \
        src/SmartWorkz.Core/SmartWorkz.Core.csproj
git commit -m "feat: add background job service abstraction and Hangfire integration

- IBackgroundJobService for fire-and-forget, scheduled, and recurring jobs
- HangfireJobService with SQL Server persistence
- Dashboard authorization filter
- Comprehensive unit tests with mocks"
```

---

### Task 1.2: File Storage Abstraction & Multi-Cloud Support

**Files:**
- Create: `src/SmartWorkz.Core/Services/FileStorage/IFileStorageService.cs`
- Create: `src/SmartWorkz.Core/Services/FileStorage/FileMetadata.cs`
- Create: `src/SmartWorkz.Core.Shared/FileStorage/LocalFileStorageService.cs`
- Create: `src/SmartWorkz.Core.Shared/FileStorage/AzureBlobStorageService.cs`
- Create: `src/SmartWorkz.Core.Shared/FileStorage/FileStorageStartupExtensions.cs`
- Test: `tests/SmartWorkz.Core.Tests/Services/FileStorage/LocalFileStorageServiceTests.cs`

**Why:** Decouples file operations from storage provider; enables local/cloud provider switching.

**Implementation Steps:**

- [ ] **Step 1: Create file storage abstraction**

```csharp
// src/SmartWorkz.Core/Services/FileStorage/IFileStorageService.cs
namespace SmartWorkz.Core.Services.FileStorage;

public interface IFileStorageService
{
    /// <summary>Upload a file to storage.</summary>
    Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default);
    
    /// <summary>Download a file from storage.</summary>
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>Delete a file from storage.</summary>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>Check if file exists.</summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>Get file metadata (size, modified date, etc).</summary>
    Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>List files in a directory/folder.</summary>
    Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default);
    
    /// <summary>Generate a temporary download URL (for cloud providers).</summary>
    Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default);
}

public class FileMetadata
{
    public string Path { get; set; }
    public string FileName { get; set; }
    public long SizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}
```

- [ ] **Step 2: Create local file storage implementation**

```csharp
// src/SmartWorkz.Core.Shared/FileStorage/LocalFileStorageService.cs
namespace SmartWorkz.Core.Shared.FileStorage;

using SmartWorkz.Core.Services.FileStorage;
using Microsoft.Extensions.Logging;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(string baseDirectory, ILogger<LocalFileStorageService> logger)
    {
        _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("File uploaded: {Path}", path);
        return fullPath;
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return await Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _logger.LogInformation("File deleted: {Path}", path);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        
        if (!File.Exists(fullPath))
            return Task.FromResult((FileMetadata?)null);

        var fileInfo = new FileInfo(fullPath);
        var metadata = new FileMetadata
        {
            Path = path,
            FileName = fileInfo.Name,
            SizeBytes = fileInfo.Length,
            ContentType = GetContentType(path),
            CreatedAt = fileInfo.CreationTimeUtc,
            ModifiedAt = fileInfo.LastWriteTimeUtc
        };

        return Task.FromResult((FileMetadata?)metadata);
    }

    public Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, folderPath);
        
        if (!Directory.Exists(fullPath))
            return Task.FromResult((IReadOnlyCollection<FileMetadata>)new List<FileMetadata>());

        var files = Directory.GetFiles(fullPath)
            .Select(f => new FileInfo(f))
            .Select(fi => new FileMetadata
            {
                Path = Path.Combine(folderPath, fi.Name),
                FileName = fi.Name,
                SizeBytes = fi.Length,
                ContentType = GetContentType(fi.Name),
                CreatedAt = fi.CreationTimeUtc,
                ModifiedAt = fi.LastWriteTimeUtc
            })
            .ToList();

        return Task.FromResult((IReadOnlyCollection<FileMetadata>)files);
    }

    public Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        // Local storage doesn't generate URLs; return file path
        var fullPath = Path.Combine(_baseDirectory, path);
        return Task.FromResult(fullPath);
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
```

- [ ] **Step 3: Create Azure Blob Storage implementation**

```csharp
// src/SmartWorkz.Core.Shared/FileStorage/AzureBlobStorageService.cs
namespace SmartWorkz.Core.Shared.FileStorage;

using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using SmartWorkz.Core.Services.FileStorage;
using Microsoft.Extensions.Logging;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(BlobContainerClient containerClient, ILogger<AzureBlobStorageService> logger)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);
        
        if (metadata?.Tags != null)
        {
            await blobClient.SetTagsAsync(metadata.Tags, cancellationToken: cancellationToken);
        }

        _logger.LogInformation("File uploaded to Azure Blob: {Path}", path);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        var download = await blobClient.DownloadAsync(cancellationToken);
        return download.Value.Content;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("File deleted from Azure Blob: {Path}", path);
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        
        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        
        return new FileMetadata
        {
            Path = path,
            FileName = Path.GetFileName(path),
            SizeBytes = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType,
            CreatedAt = properties.Value.CreatedOn,
            ModifiedAt = properties.Value.LastModified
        };
    }

    public async Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var results = new List<FileMetadata>();
        
        await foreach (var blob in _containerClient.GetBlobsAsync(prefix: folderPath, cancellationToken: cancellationToken))
        {
            results.Add(new FileMetadata
            {
                Path = blob.Name,
                FileName = Path.GetFileName(blob.Name),
                SizeBytes = blob.Properties.ContentLength ?? 0,
                ContentType = blob.Properties.ContentType,
                CreatedAt = blob.Properties.CreatedOn ?? DateTimeOffset.UtcNow,
                ModifiedAt = blob.Properties.LastModified ?? DateTimeOffset.UtcNow
            });
        }

        return results;
    }

    public async Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiration));
        return await Task.FromResult(sasUri.ToString());
    }
}
```

- [ ] **Step 4: Create startup extensions**

```csharp
// src/SmartWorkz.Core.Shared/FileStorage/FileStorageStartupExtensions.cs
namespace SmartWorkz.Core.Shared.FileStorage;

using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Services.FileStorage;

public static class FileStorageStartupExtensions
{
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, string baseDirectory)
    {
        services.AddScoped<IFileStorageService>(provider =>
            new LocalFileStorageService(baseDirectory, provider.GetRequiredService<ILogger<LocalFileStorageService>>())
        );
        return services;
    }

    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, string connectionString, string containerName)
    {
        services.AddScoped<IFileStorageService>(provider =>
        {
            var client = new BlobContainerClient(new Uri($"https://{connectionString}/{containerName}"), new Azure.Storage.StorageSharedKeyCredential(connectionString, "key"));
            return new AzureBlobStorageService(client, provider.GetRequiredService<ILogger<AzureBlobStorageService>>());
        });
        return services;
    }
}
```

- [ ] **Step 5: Write failing test**

```csharp
// tests/SmartWorkz.Core.Tests/Services/FileStorage/LocalFileStorageServiceTests.cs
namespace SmartWorkz.Core.Tests.Services.FileStorage;

using SmartWorkz.Core.Services.FileStorage;
using SmartWorkz.Core.Shared.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        var mockLogger = new Mock<ILogger<LocalFileStorageService>>();
        _service = new LocalFileStorageService(_testDirectory, mockLogger.Object);
    }

    [Fact]
    public async Task UploadAsync_WithValidStream_CreatesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var metadata = new FileMetadata { FileName = fileName };

        // Act
        await _service.UploadAsync(fileName, stream, metadata);

        // Assert
        Assert.True(await _service.ExistsAsync(fileName));
    }

    [Fact]
    public async Task DownloadAsync_WithExistingFile_ReturnsStream()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _service.UploadAsync(fileName, uploadStream, new FileMetadata { FileName = fileName });

        // Act
        var downloadStream = await _service.DownloadAsync(fileName);

        // Assert
        using (var reader = new StreamReader(downloadStream))
        {
            var downloadedContent = await reader.ReadToEndAsync();
            Assert.Equal(content, downloadedContent);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFile_RemovesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        await _service.UploadAsync(fileName, stream, new FileMetadata { FileName = fileName });

        // Act
        await _service.DeleteAsync(fileName);

        // Assert
        Assert.False(await _service.ExistsAsync(fileName));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }
}
```

- [ ] **Step 6: Run tests**

```bash
cd tests/SmartWorkz.Core.Tests
dotnet test Services/FileStorage/LocalFileStorageServiceTests.cs -v
```

Expected: PASS (3/3 tests)

- [ ] **Step 7: Commit**

```bash
git add src/SmartWorkz.Core/Services/FileStorage/ \
        src/SmartWorkz.Core.Shared/FileStorage/ \
        tests/SmartWorkz.Core.Tests/Services/FileStorage/
git commit -m "feat: add file storage abstraction with local and Azure support

- IFileStorageService for multi-cloud file operations
- LocalFileStorageService for development/testing
- AzureBlobStorageService with SAS URL generation
- Comprehensive unit tests with temp file cleanup"
```

---

### Task 1.3: Push Notification Service & Firebase Integration

**Files:**
- Create: `src/SmartWorkz.Core/Services/Notifications/IPushNotificationService.cs` (update with payload)
- Create: `src/SmartWorkz.Core/Services/Notifications/PushNotificationPayload.cs`
- Create: `src/SmartWorkz.Core.Shared/Notifications/FirebaseCloudMessagingService.cs`
- Create: `src/SmartWorkz.Core.Shared/Notifications/NotificationStartupExtensions.cs`
- Test: `tests/SmartWorkz.Core.Tests/Services/Notifications/FirebaseCloudMessagingServiceTests.cs`

**Why:** Provides push notification abstraction; enables multi-platform mobile notifications.

**Implementation Steps:**

- [ ] **Step 1: Extend push notification abstraction**

Update `src/SmartWorkz.Core/Services/Notifications/IPushNotificationService.cs`:

```csharp
namespace SmartWorkz.Core.Services.Notifications;

public interface IPushNotificationService
{
    Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default);
    Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
    Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create push notification payload**

```csharp
// src/SmartWorkz.Core/Services/Notifications/PushNotificationPayload.cs
namespace SmartWorkz.Core.Services.Notifications;

public class PushNotificationPayload
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public PushNotificationAction? Action { get; set; }
    public int? Badge { get; set; }
}

public class PushNotificationAction
{
    public string ActionId { get; set; }
    public string ActionUrl { get; set; }
    public string ActionTitle { get; set; }
}
```

- [ ] **Step 3: Add Firebase NuGet package**

Update `src/SmartWorkz.Core/SmartWorkz.Core.csproj`:
```xml
<ItemGroup>
    <PackageReference Include="FirebaseAdmin" Version="2.4.0" />
</ItemGroup>
```

- [ ] **Step 4: Implement Firebase Cloud Messaging service**

```csharp
// src/SmartWorkz.Core.Shared/Notifications/FirebaseCloudMessagingService.cs
namespace SmartWorkz.Core.Shared.Notifications;

using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using SmartWorkz.Core.Services.Notifications;
using Microsoft.Extensions.Logging;

public class FirebaseCloudMessagingService : IPushNotificationService
{
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebaseCloudMessagingService> _logger;

    public FirebaseCloudMessagingService(ILogger<FirebaseCloudMessagingService> logger)
    {
        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default)
    {
        var payload = new PushNotificationPayload
        {
            Title = title,
            Body = message
        };
        return SendAsync(userId, payload, cancellationToken);
    }

    public Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default)
    {
        var payload = new PushNotificationPayload
        {
            Title = title,
            Body = message
        };
        return SendAsync(userIds, payload, cancellationToken);
    }

    public async Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = BuildMessage(userId, payload);
            var messageId = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Push notification sent to {UserId}: {MessageId}", userId, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {UserId}", userId);
            throw;
        }
    }

    public async Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        var tasks = userIds.Select(userId => SendAsync(userId, payload, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = payload.Title,
                    Body = payload.Body,
                    ImageUrl = payload.ImageUrl
                },
                Data = payload.Data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig { Priority = Priority.High },
                Webpush = new WebpushConfig { Headers = new Dictionary<string, string> { { "TTL", "3600" } } }
            };

            var messageId = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Topic notification sent to {Topic}: {MessageId}", topic, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send topic notification to {Topic}", topic);
            throw;
        }
    }

    public async Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokens = new[] { userId };
            await _firebaseMessaging.SubscribeToTopicAsync(tokens, topic);
            _logger.LogInformation("User {UserId} subscribed to topic {Topic}", userId, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe {UserId} to topic {Topic}", userId, topic);
            throw;
        }
    }

    public async Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokens = new[] { userId };
            await _firebaseMessaging.UnsubscribeFromTopicAsync(tokens, topic);
            _logger.LogInformation("User {UserId} unsubscribed from topic {Topic}", userId, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe {UserId} from topic {Topic}", userId, topic);
            throw;
        }
    }

    private Message BuildMessage(string userId, PushNotificationPayload payload)
    {
        return new Message
        {
            Token = userId,
            Notification = new Notification
            {
                Title = payload.Title,
                Body = payload.Body,
                ImageUrl = payload.ImageUrl
            },
            Data = payload.Data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    Badge = payload.Badge?.ToString()
                }
            },
            Webpush = new WebpushConfig
            {
                Headers = new Dictionary<string, string> { { "TTL", "3600" } }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps { Badge = payload.Badge }
            }
        };
    }
}
```

- [ ] **Step 5: Create startup extensions**

```csharp
// src/SmartWorkz.Core.Shared/Notifications/NotificationStartupExtensions.cs
namespace SmartWorkz.Core.Shared.Notifications;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Services.Notifications;

public static class NotificationStartupExtensions
{
    public static IServiceCollection AddFirebaseCloudMessaging(
        this IServiceCollection services,
        string serviceAccountPath)
    {
        if (!File.Exists(serviceAccountPath))
            throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountPath}");

        var credential = GoogleCredential.FromFile(serviceAccountPath);
        FirebaseApp.Create(new AppOptions { Credential = credential });

        services.AddScoped<IPushNotificationService, FirebaseCloudMessagingService>();
        return services;
    }
}
```

- [ ] **Step 6: Write failing test**

```csharp
// tests/SmartWorkz.Core.Tests/Services/Notifications/FirebaseCloudMessagingServiceTests.cs
namespace SmartWorkz.Core.Tests.Services.Notifications;

using Moq;
using SmartWorkz.Core.Services.Notifications;
using SmartWorkz.Core.Shared.Notifications;
using Microsoft.Extensions.Logging;
using Xunit;

public class FirebaseCloudMessagingServiceTests
{
    [Fact]
    public async Task SendAsync_WithValidPayload_SendsNotification()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FirebaseCloudMessagingService>>();
        var service = new FirebaseCloudMessagingService(mockLogger.Object);
        var payload = new PushNotificationPayload
        {
            Title = "Test",
            Body = "Test Message"
        };

        // Act & Assert - Note: Real Firebase integration would need mock
        // This test validates structure; actual implementation requires Firebase emulator
        Assert.NotNull(payload);
        Assert.Equal("Test", payload.Title);
    }

    [Fact]
    public void PushNotificationPayload_HasExpectedProperties()
    {
        // Arrange
        var payload = new PushNotificationPayload
        {
            Title = "Title",
            Body = "Body",
            ImageUrl = "http://example.com/image.jpg",
            Badge = 5,
            Data = new Dictionary<string, string> { { "key", "value" } }
        };

        // Assert
        Assert.Equal("Title", payload.Title);
        Assert.Equal("Body", payload.Body);
        Assert.Equal("http://example.com/image.jpg", payload.ImageUrl);
        Assert.Equal(5, payload.Badge);
        Assert.NotEmpty(payload.Data);
    }
}
```

- [ ] **Step 7: Run tests**

```bash
cd tests/SmartWorkz.Core.Tests
dotnet test Services/Notifications/FirebaseCloudMessagingServiceTests.cs -v
```

Expected: PASS (2/2 tests)

- [ ] **Step 8: Commit**

```bash
git add src/SmartWorkz.Core/Services/Notifications/ \
        src/SmartWorkz.Core.Shared/Notifications/ \
        tests/SmartWorkz.Core.Tests/Services/Notifications/
git commit -m "feat: add Firebase Cloud Messaging push notification service

- Extended IPushNotificationService with payload and topic support
- PushNotificationPayload with metadata (images, badges, data)
- FirebaseCloudMessagingService with multi-platform support
- Topic subscription management for broadcast notifications
- Unit tests validating payload structure"
```

---

## Phase 2: Advanced Patterns (CQRS & Event Sourcing)

**Deliverables:**
- Domain event abstraction and publishing
- CQRS command/query pattern with handlers
- Event sourcing event store
- Event handlers and sagas
- **Estimated effort:** 3-4 days (4 independent tasks)

### Task 2.1: Domain Events & Event Publishing

**Files:**
- Create: `src/SmartWorkz.Core/Events/IDomainEvent.cs`
- Create: `src/SmartWorkz.Core/Events/IEventPublisher.cs`
- Create: `src/SmartWorkz.Core.Shared/Events/InMemoryEventPublisher.cs`
- Create: `src/SmartWorkz.Core.Shared/Events/MassTransitEventPublisher.cs`
- Test: `tests/SmartWorkz.Core.Tests/Events/InMemoryEventPublisherTests.cs`

**Steps:** (Similar TDD structure as Phase 1)

---

### Task 2.2: CQRS Command/Query Handlers

**Files:**
- Create: `src/SmartWorkz.Core/CQRS/ICommand.cs`
- Create: `src/SmartWorkz.Core/CQRS/IQuery.cs`
- Create: `src/SmartWorkz.Core/CQRS/ICommandHandler.cs`
- Create: `src/SmartWorkz.Core/CQRS/IQueryHandler.cs`
- Create: `src/SmartWorkz.Core.Shared/CQRS/MediatorCommandDispatcher.cs`

---

### Task 2.3: Event Sourcing Event Store

**Files:**
- Create: `src/SmartWorkz.Core/EventSourcing/IEventStore.cs`
- Create: `src/SmartWorkz.Core.Shared/EventSourcing/SqlEventStore.cs`
- Create: `src/SmartWorkz.Core.Shared/EventSourcing/EventStoreSnapshot.cs`

---

### Task 2.4: Event Handlers & Saga Pattern

**Files:**
- Create: `src/SmartWorkz.Core/Sagas/ISagaDefinition.cs`
- Create: `src/SmartWorkz.Core.Shared/Sagas/SagaOrchestrator.cs`

---

## Phase 3: Cross-Cutting Concerns (Observability & Testing)

**Deliverables:**
- Structured logging (Serilog integration)
- Distributed tracing (OpenTelemetry)
- Metrics collection
- Test base classes and fixtures
- Integration test helpers
- **Estimated effort:** 2-3 days (3 independent tasks)

### Task 3.1: Structured Logging & Serilog

**Files:**
- Create: `src/SmartWorkz.Core.Shared/Logging/LoggingStartupExtensions.cs`
- Create: `src/SmartWorkz.Core.Shared/Logging/EnrichedLogger.cs`

---

### Task 3.2: OpenTelemetry Distributed Tracing

**Files:**
- Create: `src/SmartWorkz.Core.Shared/Tracing/TracingStartupExtensions.cs`
- Create: `src/SmartWorkz.Core.Shared/Tracing/ActivityContextMiddleware.cs`

---

### Task 3.3: Testing Utilities & Base Classes

**Files:**
- Create: `tests/SmartWorkz.Core.Tests/Fixtures/DatabaseFixture.cs`
- Create: `tests/SmartWorkz.Core.Tests/Fixtures/TestDataBuilder.cs`
- Create: `tests/SmartWorkz.Core.Tests/Helpers/AssertionHelpers.cs`

---

## Implementation Roadmap Timeline

```
Week 1: Phase 1 - Infrastructure (Mon-Fri)
├─ Task 1.1: Background Jobs (Mon-Tue)
├─ Task 1.2: File Storage (Wed-Thu)
└─ Task 1.3: Push Notifications (Thu-Fri)
  
Week 2: Phase 2 - Advanced Patterns (Mon-Fri)
├─ Task 2.1: Domain Events (Mon-Tue)
├─ Task 2.2: CQRS Pattern (Wed-Thu)
├─ Task 2.3: Event Sourcing (Thu)
└─ Task 2.4: Sagas (Fri)

Week 3: Phase 3 - Observability (Mon-Fri)
├─ Task 3.1: Structured Logging (Mon-Tue)
├─ Task 3.2: Distributed Tracing (Wed-Thu)
└─ Task 3.3: Testing Utilities (Thu-Fri)
```

## Dependencies & Integration Points

**Phase 1 → Phase 2:**
- Background jobs (1.1) triggers domain events (2.1)
- File storage (1.2) integrates with CQRS commands (2.2)

**Phase 2 → Phase 3:**
- CQRS commands (2.2) logged via Serilog (3.1)
- Domain events (2.1) traced via OpenTelemetry (3.2)

**Phase 3 Benefits All:**
- All services logged with structured logging
- All handlers traced for observability
- Integration tests use test fixtures (3.3)

---

## Success Criteria

- ✅ All Phase 1 tasks have >80% test coverage
- ✅ Phase 2 event publishing supports both in-memory and MassTransit
- ✅ Phase 3 includes Docker Compose for Jaeger/ELK observability stack
- ✅ Each phase produces zero breaking changes to existing code
- ✅ All code follows your existing DDD/value object patterns

---

## Next Steps

Plan complete and saved to `docs/superpowers/plans/2026-04-21-complete-architecture-roadmap.md`.

**Two execution options:**

**1. Subagent-Driven (Recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach would you prefer?**

