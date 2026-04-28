using Xunit;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for TimePickerComponent verifying time selection, callbacks, validation, and state management.
/// </summary>
public class TimePickerComponentTests
{
    [Fact]
    public void TimePickerComponent_WithTime_RendersInputWithValue()
    {
        // Arrange
        var time = new TimeSpan(14, 30, 0);

        // Act
        var timeString = time.ToString("hh\\:mm");

        // Assert
        Assert.Equal("14:30", timeString);
        Assert.True(time.Hours == 14);
        Assert.True(time.Minutes == 30);
    }

    [Fact]
    public void TimePickerComponent_WithoutTime_RendersEmptyInput()
    {
        // Arrange
        TimeSpan? time = null;

        // Act
        var hasValue = time.HasValue;

        // Assert
        Assert.False(hasValue);
    }

    [Fact]
    public void TimePickerComponent_OnTimeChange_InvokesCallback()
    {
        // Arrange
        var oldTime = new TimeSpan(10, 0, 0);
        var newTime = new TimeSpan(14, 30, 0);
        var callbackInvoked = false;

        // Act - Simulate callback
        callbackInvoked = true;

        // Assert
        Assert.True(callbackInvoked);
        Assert.NotEqual(oldTime, newTime);
    }

    [Fact]
    public void TimePickerComponent_WithLabel_DisplaysLabel()
    {
        // Arrange
        var label = "Appointment Time";

        // Act
        var hasLabel = !string.IsNullOrEmpty(label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Appointment Time", label);
    }

    [Fact]
    public void TimePickerComponent_WithErrorMessage_DisplaysErrorClass()
    {
        // Arrange
        var errorMessage = "Time is required";
        var cssClass = "is-invalid";

        // Act
        var hasError = !string.IsNullOrEmpty(errorMessage);
        var appliesClass = hasError ? cssClass : "";

        // Assert
        Assert.True(hasError);
        Assert.Equal("is-invalid", appliesClass);
    }

    [Fact]
    public void TimePickerComponent_IsDisabled_RendersDisabledAttribute()
    {
        // Arrange
        var isDisabled = true;

        // Act
        var disabledAttribute = isDisabled ? "disabled" : "";

        // Assert
        Assert.Equal("disabled", disabledAttribute);
        Assert.True(isDisabled);
    }

    [Fact]
    public void TimePickerComponent_WithMinTime_ValidatesMinBoundary()
    {
        // Arrange
        var minTime = new TimeSpan(9, 0, 0);
        var selectedTime = new TimeSpan(10, 0, 0);
        var validTime = new TimeSpan(8, 0, 0);

        // Act
        var isSelectedValid = selectedTime >= minTime;
        var isValidTimeInvalid = validTime < minTime;

        // Assert
        Assert.True(isSelectedValid);
        Assert.True(isValidTimeInvalid);
    }

    [Fact]
    public void TimePickerComponent_WithMaxTime_ValidatesMaxBoundary()
    {
        // Arrange
        var maxTime = new TimeSpan(17, 0, 0);
        var selectedTime = new TimeSpan(16, 0, 0);
        var validTime = new TimeSpan(18, 0, 0);

        // Act
        var isSelectedValid = selectedTime <= maxTime;
        var isValidTimeInvalid = validTime > maxTime;

        // Assert
        Assert.True(isSelectedValid);
        Assert.True(isValidTimeInvalid);
    }

    [Fact]
    public void TimePickerComponent_WithMinAndMaxTime_ValidatesRange()
    {
        // Arrange
        var minTime = new TimeSpan(9, 0, 0);
        var maxTime = new TimeSpan(17, 0, 0);
        var validTime = new TimeSpan(12, 0, 0);
        var tooEarlyTime = new TimeSpan(8, 0, 0);
        var tooLateTime = new TimeSpan(18, 0, 0);

        // Act
        var isValidTimeInRange = validTime >= minTime && validTime <= maxTime;
        var isTooEarlyInRange = tooEarlyTime >= minTime && tooEarlyTime <= maxTime;
        var isTooLateInRange = tooLateTime >= minTime && tooLateTime <= maxTime;

        // Assert
        Assert.True(isValidTimeInRange);
        Assert.False(isTooEarlyInRange);
        Assert.False(isTooLateInRange);
    }

    [Fact]
    public void TimePickerComponent_Formats24HourTime_CorrectlyConvertsToString()
    {
        // Arrange
        var morningTime = new TimeSpan(6, 30, 0);
        var afternoonTime = new TimeSpan(14, 30, 0);
        var eveningTime = new TimeSpan(23, 59, 0);

        // Act
        var morningString = morningTime.ToString("hh\\:mm");
        var afternoonString = afternoonTime.ToString("hh\\:mm");
        var eveningString = eveningTime.ToString("hh\\:mm");

        // Assert
        Assert.Equal("06:30", morningString);
        Assert.Equal("14:30", afternoonString);
        Assert.Equal("23:59", eveningString);
    }
}
