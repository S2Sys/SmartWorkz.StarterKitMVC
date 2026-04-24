using FluentAssertions;
using SmartWorkz.Windows.ViewModels;
using Xunit;

namespace SmartWorkz.Windows.Tests.ViewModels;

/// <summary>
/// Unit tests for MainViewModel MVVM state management.
/// Tests cover initialization, connection state, sync state, and notifications.
/// </summary>
public class MainViewModelTests
{
    private MainViewModel CreateViewModel()
    {
        return new MainViewModel();
    }

    [Fact]
    public void Constructor_InitializesPropertiesWithDefaultValues()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert - default state
        viewModel.IsConnected.Should().BeFalse("initially disconnected from server");
        viewModel.IsSyncing.Should().BeFalse("initially not syncing");
        viewModel.UnreadNotificationCount.Should().Be(0, "should start with no unread notifications");
        viewModel.CurrentUser.Should().BeNull("should have no user on startup");
        viewModel.Title.Should().NotBeNullOrWhiteSpace("should have a default window title");
    }

    [Fact]
    public void IsConnected_WhenChanged_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsConnected))
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.IsConnected = true;

        // Assert
        propertyChangedRaised.Should().BeTrue("PropertyChanged event should be raised");
        changedPropertyName.Should().Be(nameof(MainViewModel.IsConnected));
        viewModel.IsConnected.Should().BeTrue("property should be updated");
    }

    [Fact]
    public void IsSyncing_WhenToggled_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var syncPropertyChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsSyncing))
            {
                syncPropertyChangedCount++;
            }
        };

        // Act
        viewModel.IsSyncing = true;
        viewModel.IsSyncing = false;

        // Assert
        syncPropertyChangedCount.Should().Be(2, "PropertyChanged should be raised for each change");
        viewModel.IsSyncing.Should().BeFalse("final state should reflect last assignment");
    }

    [Fact]
    public void UnreadNotificationCount_WhenIncremented_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var notificationChangedCount = 0;

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.UnreadNotificationCount))
            {
                notificationChangedCount++;
            }
        };

        // Act
        viewModel.UnreadNotificationCount = 1;
        viewModel.UnreadNotificationCount = 3;
        viewModel.UnreadNotificationCount = 0;

        // Assert
        notificationChangedCount.Should().Be(3, "PropertyChanged should be raised for each count change");
        viewModel.UnreadNotificationCount.Should().Be(0, "notifications should reset to 0");
    }
}
