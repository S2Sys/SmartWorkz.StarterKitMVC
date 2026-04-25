using Xunit;
using SmartWorkz.Mobile.State;
using SmartWorkz.Mobile.State.Store;
using SmartWorkz.Mobile.State.Actions;
using Moq;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Mobile.Tests.State;

public class AppStoreTests
{
    private readonly Mock<IReducerRegistry> _mockReducerRegistry;
    private readonly Mock<ILogger<AppStore>> _mockLogger;

    public AppStoreTests()
    {
        _mockReducerRegistry = new Mock<IReducerRegistry>();
        _mockLogger = new Mock<ILogger<AppStore>>();
    }

    [Fact]
    public void Dispatch_WithValidAction_UpdatesState()
    {
        // Arrange
        var initialState = new AppState();
        var testAction = new TestAction { Value = 42 };
        var expectedState = new AppState { TestValue = 42 };

        _mockReducerRegistry
            .Setup(r => r.Reduce(initialState, testAction))
            .Returns(expectedState);

        var store = new AppStore(initialState, _mockReducerRegistry.Object, _mockLogger.Object);

        // Act
        store.Dispatch(testAction);

        // Assert
        Assert.NotNull(store.GetState());
        Assert.Equal(42, store.GetState().TestValue);
    }

    [Fact]
    public void Subscribe_WithStateChange_NotifiesSubscriber()
    {
        // Arrange
        var initialState = new AppState();
        var testAction = new TestAction { Value = 123 };
        var updatedState = new AppState { TestValue = 123 };

        _mockReducerRegistry
            .Setup(r => r.Reduce(initialState, testAction))
            .Returns(updatedState);

        var store = new AppStore(initialState, _mockReducerRegistry.Object, _mockLogger.Object);
        var stateChanges = new List<AppState>();

        store.Subscribe(state => stateChanges.Add(state));

        // Act
        store.Dispatch(testAction);

        // Assert
        Assert.Single(stateChanges);
        Assert.Equal(123, stateChanges[0].TestValue);
    }

    [Fact]
    public void Subscribe_ReturnsUnsubscribeFunction()
    {
        // Arrange
        var initialState = new AppState();
        var testAction = new TestAction { Value = 100 };
        var updatedState = new AppState { TestValue = 100 };

        _mockReducerRegistry
            .Setup(r => r.Reduce(initialState, testAction))
            .Returns(updatedState);

        var store = new AppStore(initialState, _mockReducerRegistry.Object, _mockLogger.Object);
        var callCount = 0;

        var unsubscribe = store.Subscribe(_ => callCount++);

        // Act - First dispatch should notify
        store.Dispatch(testAction);
        Assert.Equal(1, callCount);

        // Unsubscribe
        unsubscribe();

        // Second dispatch should not notify
        store.Dispatch(testAction);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetState_ReturnsCurrentState()
    {
        // Arrange
        var initialState = new AppState { TestValue = 999 };
        var store = new AppStore(initialState, _mockReducerRegistry.Object, _mockLogger.Object);

        // Act
        var state = store.GetState();

        // Assert
        Assert.NotNull(state);
        Assert.Equal(999, state.TestValue);
    }

    [Fact]
    public void Dispatch_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new AppStore(new AppState(), _mockReducerRegistry.Object, _mockLogger.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => store.Dispatch(null!));
    }

    [Fact]
    public void Subscribe_WithNullListener_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new AppStore(new AppState(), _mockReducerRegistry.Object, _mockLogger.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => store.Subscribe(null!));
    }

    [Fact]
    public void Dispatch_NoStateChange_DoesNotNotifyListeners()
    {
        // Arrange
        var initialState = new AppState();
        var testAction = new TestAction { Value = 50 };

        // Return same state (no change)
        _mockReducerRegistry
            .Setup(r => r.Reduce(initialState, testAction))
            .Returns(initialState);

        var store = new AppStore(initialState, _mockReducerRegistry.Object, _mockLogger.Object);
        var callCount = 0;

        store.Subscribe(_ => callCount++);

        // Act
        store.Dispatch(testAction);

        // Assert
        Assert.Equal(0, callCount); // No notification if state didn't change
    }
}

public class TestAction : IAction
{
    public int Value { get; set; }
}
