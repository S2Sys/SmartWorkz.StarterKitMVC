namespace SmartWorkz.Core.Tests.Services.BackgroundJobs;

using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using SmartWorkz.Core.Services.BackgroundJobs;
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
    public async Task EnqueueAsync_WithValidAction_ReturnsNonEmptyJobId()
    {
        // Arrange
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-123");

        // Act
        var result = await _service.EnqueueAsync<DummyJob>(x => Task.CompletedTask);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ScheduleAsync_WithFutureDate_ReturnsNonEmptyJobId()
    {
        // Arrange
        var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("scheduled-job-123");

        // Act
        var result = await _service.ScheduleAsync<DummyJob>(x => Task.CompletedTask, scheduledTime);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WithCronExpression_ReturnsJobId()
    {
        // Arrange
        var jobId = "recurring-job";
        var cron = "0 0 * * *";

        // Act
        var result = await _service.AddOrUpdateRecurringAsync<DummyJob>(jobId, x => Task.CompletedTask, cron);

        // Assert
        Assert.Equal(jobId, result);
    }

    [Fact]
    public async Task DeleteAsync_WithJobId_Completes()
    {
        // Act & Assert - should not throw
        await _service.DeleteAsync("job-123");
    }

    [Fact]
    public async Task RequeueAsync_WithJobId_ReturnsNonEmptyJobId()
    {
        // Arrange
        _mockJobClient
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("requeued-job-124");

        // Act
        var result = await _service.RequeueAsync("job-123");

        // Assert
        Assert.NotEmpty(result);
    }

    private class DummyJob
    {
        public async Task Execute() => await Task.CompletedTask;
    }
}
