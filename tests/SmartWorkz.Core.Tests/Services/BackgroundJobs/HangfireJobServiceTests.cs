namespace SmartWorkz.Core.Tests.Services.BackgroundJobs;

using Hangfire;
using Hangfire.Common;
using Hangfire.States;
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
        _service = new HangfireJobService(_mockJobClient.Object, _mockRecurringJobManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task EnqueueAsync_WithValidAction_CallsJobClientCreateWithEnqueuedState()
    {
        // Arrange
        const string expectedJobId = "job-123";
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns(expectedJobId);

        // Act
        var result = await _service.EnqueueAsync<DummyJob>(x => x.Execute());

        // Assert
        Assert.Equal(expectedJobId, result);
        _mockJobClient.Verify(
            x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()),
            Times.Once,
            "EnqueueAsync should call _jobClient.Create with EnqueuedState exactly once");
    }

    [Fact]
    public async Task ScheduleAsync_WithFutureDate_CallsJobClientCreateWithScheduledState()
    {
        // Arrange
        const string expectedJobId = "scheduled-job-123";
        var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()))
            .Returns(expectedJobId);

        // Act
        var result = await _service.ScheduleAsync<DummyJob>(x => x.Execute(), scheduledTime);

        // Assert
        Assert.Equal(expectedJobId, result);
        _mockJobClient.Verify(
            x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()),
            Times.Once,
            "ScheduleAsync should call _jobClient.Create with ScheduledState exactly once");
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WithCronExpression_ReturnsJobId()
    {
        // Arrange
        const string jobId = "recurring-job";
        const string cron = "0 0 * * *";

        // Act
        var result = await _service.AddOrUpdateRecurringAsync<DummyJob>(jobId, x => x.Execute(), cron);

        // Assert
        Assert.Equal(jobId, result);
        // The mock will be called, but since AddOrUpdate returns void,
        // we just verify the method returns the job ID
    }

    [Fact]
    public async Task EnqueueAsync_WhenJobClientThrows_PropagatesException()
    {
        // Arrange
        var exception = new InvalidOperationException("Enqueue failed");
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Throws(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EnqueueAsync<DummyJob>(x => x.Execute()));
        Assert.Equal(exception.Message, ex.Message);
    }

    [Fact]
    public async Task ScheduleAsync_WhenJobClientThrows_PropagatesException()
    {
        // Arrange
        var exception = new InvalidOperationException("Schedule failed");
        var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);

        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()))
            .Throws(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ScheduleAsync<DummyJob>(x => x.Execute(), scheduledTime));
        Assert.Equal(exception.Message, ex.Message);
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WithValidCron_DoesNotThrow()
    {
        // Arrange
        const string jobId = "recurring-job";
        const string cron = "0 0 * * *";

        // Act
        var result = await _service.AddOrUpdateRecurringAsync<DummyJob>(jobId, x => x.Execute(), cron);

        // Assert - should not throw and should return the job ID
        Assert.NotNull(result);
        Assert.Equal(jobId, result);
    }

    private class DummyJob
    {
        public async Task Execute() => await Task.CompletedTask;
    }
}
