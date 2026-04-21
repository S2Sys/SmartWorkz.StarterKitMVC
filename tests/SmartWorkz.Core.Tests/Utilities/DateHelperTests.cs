namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Shared.Utilities;

public class DateHelperTests
{
    #region GetAge Tests

    [Fact]
    public void GetAge_WithValidBirthDate_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-25).AddDays(-10);

        // Act
        var result = DateHelper.GetAge(birthDate);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(25, result.Data);
    }

    [Fact]
    public void GetAge_WithBirthdayToday_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-30);

        // Act
        var result = DateHelper.GetAge(birthDate);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(30, result.Data);
    }

    [Fact]
    public void GetAge_WithFutureBirthDate_ReturnsFail()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(1);

        // Act
        var result = DateHelper.GetAge(birthDate);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.MessageKey);
    }

    #endregion

    #region GetRelativeTime Tests

    [Fact]
    public void GetRelativeTime_WithJustNow_ReturnsJustNow()
    {
        // Arrange
        var date = DateTime.UtcNow.AddSeconds(-30);

        // Act
        var result = DateHelper.GetRelativeTime(date);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("just now", result.Data);
    }

    [Fact]
    public void GetRelativeTime_WithMinutesAgo_ReturnsMinutesString()
    {
        // Arrange
        var date = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var result = DateHelper.GetRelativeTime(date);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("5 minutes ago", result.Data!);
    }

    [Fact]
    public void GetRelativeTime_WithHoursAgo_ReturnsHoursString()
    {
        // Arrange
        var date = DateTime.UtcNow.AddHours(-3);

        // Act
        var result = DateHelper.GetRelativeTime(date);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("3 hours ago", result.Data!);
    }

    [Fact]
    public void GetRelativeTime_WithDaysAgo_ReturnsDaysString()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-2);

        // Act
        var result = DateHelper.GetRelativeTime(date);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("2 days ago", result.Data!);
    }

    [Fact]
    public void GetRelativeTime_WithWeeksAgo_ReturnsWeeksString()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-14);

        // Act
        var result = DateHelper.GetRelativeTime(date);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("week", result.Data!);
    }

    #endregion

    #region StartOfDay Tests

    [Fact]
    public void StartOfDay_ReturnsDateAt000000()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15, 14, 30, 45);

        // Act
        var result = DateHelper.StartOfDay(date);

        // Assert
        Assert.Equal(new DateTime(2024, 4, 15, 0, 0, 0), result);
    }

    [Fact]
    public void StartOfDay_WithAlreadyStartOfDay_ReturnsUnchanged()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15, 0, 0, 0);

        // Act
        var result = DateHelper.StartOfDay(date);

        // Assert
        Assert.Equal(date, result);
    }

    #endregion

    #region EndOfDay Tests

    [Fact]
    public void EndOfDay_ReturnsDateAt235959999()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15, 14, 30, 45);

        // Act
        var result = DateHelper.EndOfDay(date);

        // Assert
        Assert.True(result.Hour == 23 && result.Minute == 59 && result.Second == 59);
    }

    [Fact]
    public void EndOfDay_IsJustBeforeMidnight()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15);

        // Act
        var result = DateHelper.EndOfDay(date);
        var nextDay = DateHelper.StartOfDay(date.AddDays(1));

        // Assert
        Assert.True(result < nextDay);
    }

    #endregion

    #region IsWeekend Tests

    [Fact]
    public void IsWeekend_WithSaturday_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 4, 20); // Saturday

        // Act
        var result = DateHelper.IsWeekend(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWeekend_WithSunday_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 4, 21); // Sunday

        // Act
        var result = DateHelper.IsWeekend(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWeekend_WithMonday_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 4, 22); // Monday

        // Act
        var result = DateHelper.IsWeekend(date);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWeekend_WithFriday_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 4, 19); // Friday

        // Act
        var result = DateHelper.IsWeekend(date);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetDayOfWeekName Tests

    [Fact]
    public void GetDayOfWeekName_ReturnsDayName()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15); // Monday

        // Act
        var result = DateHelper.GetDayOfWeekName(date);

        // Assert
        Assert.Equal("Monday", result);
    }

    [Fact]
    public void GetDayOfWeekName_WithDifferentDays_ReturnsCorrectNames()
    {
        // Arrange & Act & Assert
        Assert.Equal("Monday", DateHelper.GetDayOfWeekName(new DateTime(2024, 4, 15)));
        Assert.Equal("Saturday", DateHelper.GetDayOfWeekName(new DateTime(2024, 4, 20)));
        Assert.Equal("Sunday", DateHelper.GetDayOfWeekName(new DateTime(2024, 4, 21)));
    }

    #endregion

    #region DaysBetween Tests

    [Fact]
    public void DaysBetween_WithSameDates_ReturnsZero()
    {
        // Arrange
        var date = new DateTime(2024, 4, 15);

        // Act
        var result = DateHelper.DaysBetween(date, date);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DaysBetween_WithDifferentDates_ReturnsCorrectDays()
    {
        // Arrange
        var from = new DateTime(2024, 4, 15);
        var to = new DateTime(2024, 4, 22);

        // Act
        var result = DateHelper.DaysBetween(from, to);

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void DaysBetween_WithReversedDates_ReturnsNegativeDays()
    {
        // Arrange
        var from = new DateTime(2024, 4, 22);
        var to = new DateTime(2024, 4, 15);

        // Act
        var result = DateHelper.DaysBetween(from, to);

        // Assert
        Assert.Equal(-7, result);
    }

    #endregion
}

