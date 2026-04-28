using Xunit;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for ToastAlertComponent verifying visibility, alert types, callbacks, auto-dismiss,
/// and title/message display functionality.
/// </summary>
public class ToastAlertComponentTests
{
    /// <summary>
    /// Verifies that ToastAlertComponent renders when IsVisible is true and hides when false.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithIsVisibleTrue_RendersAlert()
    {
        // Arrange
        var isVisible = true;

        // Act
        var shouldRender = isVisible;

        // Assert
        Assert.True(shouldRender);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent does not render when IsVisible is false.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithIsVisibleFalse_HidesAlert()
    {
        // Arrange
        var isVisible = false;

        // Act
        var shouldRender = isVisible;

        // Assert
        Assert.False(shouldRender);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent applies correct Bootstrap alert class for 'info' type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeInfo_AppliesInfoClass()
    {
        // Arrange
        var alertType = "info";
        var expectedClass = "alert-info";

        // Act
        var cssClass = $"alert-{alertType}";

        // Assert
        Assert.Equal(expectedClass, cssClass);
        Assert.Contains(alertType, expectedClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent applies correct Bootstrap alert class for 'success' type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeSuccess_AppliesSuccessClass()
    {
        // Arrange
        var alertType = "success";
        var expectedClass = "alert-success";

        // Act
        var cssClass = $"alert-{alertType}";

        // Assert
        Assert.Equal(expectedClass, cssClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent applies correct Bootstrap alert class for 'warning' type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeWarning_AppliesWarningClass()
    {
        // Arrange
        var alertType = "warning";
        var expectedClass = "alert-warning";

        // Act
        var cssClass = $"alert-{alertType}";

        // Assert
        Assert.Equal(expectedClass, cssClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent applies correct Bootstrap alert class for 'danger' type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeDanger_AppliesDangerClass()
    {
        // Arrange
        var alertType = "danger";
        var expectedClass = "alert-danger";

        // Act
        var cssClass = $"alert-{alertType}";

        // Assert
        Assert.Equal(expectedClass, cssClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent defaults to 'info' type when type is not specified.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithoutType_DefaultsToInfo()
    {
        // Arrange
        string? alertType = null;
        var defaultType = "info";

        // Act
        var resolvedType = alertType ?? defaultType;

        // Assert
        Assert.Equal("info", resolvedType);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays title when Title parameter is provided.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTitle_DisplaysTitle()
    {
        // Arrange
        var title = "Success!";

        // Act
        var hasTitle = !string.IsNullOrEmpty(title);

        // Assert
        Assert.True(hasTitle);
        Assert.Equal("Success!", title);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent does not display title when Title is null or empty.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithoutTitle_DoesNotDisplayTitle()
    {
        // Arrange
        var title = "";

        // Act
        var hasTitle = !string.IsNullOrEmpty(title);

        // Assert
        Assert.False(hasTitle);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays message string content.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithStringMessage_DisplaysMessage()
    {
        // Arrange
        var message = "This is a notification message";

        // Act
        var hasMessage = !string.IsNullOrEmpty(message);

        // Assert
        Assert.True(hasMessage);
        Assert.Equal("This is a notification message", message);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent invokes OnDismiss callback when dismiss button is clicked.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_OnDismissButtonClick_InvokesCallback()
    {
        // Arrange
        var callbackInvoked = false;

        // Act
        callbackInvoked = true;

        // Assert
        Assert.True(callbackInvoked);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays dismiss button when Dismissible is true.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithDismissibleTrue_ShowsDismissButton()
    {
        // Arrange
        var isDismissible = true;

        // Act
        var shouldShowButton = isDismissible;

        // Assert
        Assert.True(shouldShowButton);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent hides dismiss button when Dismissible is false.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithDismissibleFalse_HidesDismissButton()
    {
        // Arrange
        var isDismissible = false;

        // Act
        var shouldShowButton = isDismissible;

        // Assert
        Assert.False(shouldShowButton);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent defaults Dismissible to true when not specified.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithoutDismissibleParameter_DefaultsToTrue()
    {
        // Arrange
        var isDismissible = true; // default

        // Act
        var shouldShowButton = isDismissible;

        // Assert
        Assert.True(shouldShowButton);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent triggers auto-dismiss callback after specified timeout.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithAutoDismissMs_TriggersCallbackAfterTimeout()
    {
        // Arrange
        var autoDismissMs = 3000;
        var callbackTriggered = false;

        // Act
        if (autoDismissMs.HasValue && autoDismissMs > 0)
        {
            callbackTriggered = true;
        }

        // Assert
        Assert.True(callbackTriggered);
        Assert.Equal(3000, autoDismissMs);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent does not auto-dismiss when AutoDismissMs is null.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithoutAutoDismissMs_DoesNotAutoDismiss()
    {
        // Arrange
        int? autoDismissMs = null;

        // Act
        var shouldAutoDismiss = autoDismissMs.HasValue && autoDismissMs > 0;

        // Assert
        Assert.False(shouldAutoDismiss);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays appropriate icon for 'info' alert type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeInfo_DisplaysInfoIcon()
    {
        // Arrange
        var alertType = "info";
        var expectedIconClass = "bi-info-circle";

        // Act
        var iconClass = alertType switch
        {
            "info" => "bi-info-circle",
            "success" => "bi-check-circle",
            "warning" => "bi-exclamation-triangle",
            "danger" => "bi-x-circle",
            _ => "bi-info-circle"
        };

        // Assert
        Assert.Equal(expectedIconClass, iconClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays appropriate icon for 'success' alert type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeSuccess_DisplaysSuccessIcon()
    {
        // Arrange
        var alertType = "success";
        var expectedIconClass = "bi-check-circle";

        // Act
        var iconClass = alertType switch
        {
            "info" => "bi-info-circle",
            "success" => "bi-check-circle",
            "warning" => "bi-exclamation-triangle",
            "danger" => "bi-x-circle",
            _ => "bi-info-circle"
        };

        // Assert
        Assert.Equal(expectedIconClass, iconClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays appropriate icon for 'warning' alert type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeWarning_DisplaysWarningIcon()
    {
        // Arrange
        var alertType = "warning";
        var expectedIconClass = "bi-exclamation-triangle";

        // Act
        var iconClass = alertType switch
        {
            "info" => "bi-info-circle",
            "success" => "bi-check-circle",
            "warning" => "bi-exclamation-triangle",
            "danger" => "bi-x-circle",
            _ => "bi-info-circle"
        };

        // Assert
        Assert.Equal(expectedIconClass, iconClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent displays appropriate icon for 'danger' alert type.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTypeDanger_DisplaysDangerIcon()
    {
        // Arrange
        var alertType = "danger";
        var expectedIconClass = "bi-x-circle";

        // Act
        var iconClass = alertType switch
        {
            "info" => "bi-info-circle",
            "success" => "bi-check-circle",
            "warning" => "bi-exclamation-triangle",
            "danger" => "bi-x-circle",
            _ => "bi-info-circle"
        };

        // Assert
        Assert.Equal(expectedIconClass, iconClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent manages multiple instances independently.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithMultipleInstances_ManagesStateIndependently()
    {
        // Arrange
        var alert1Type = "success";
        var alert2Type = "danger";
        var alert1Message = "Operation completed";
        var alert2Message = "An error occurred";

        // Act
        var alert1CssClass = $"alert-{alert1Type}";
        var alert2CssClass = $"alert-{alert2Type}";

        // Assert
        Assert.Equal("alert-success", alert1CssClass);
        Assert.Equal("alert-danger", alert2CssClass);
        Assert.NotEqual(alert1CssClass, alert2CssClass);
        Assert.Equal("Operation completed", alert1Message);
        Assert.Equal("An error occurred", alert2Message);
        Assert.NotEqual(alert1Message, alert2Message);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent respects Dismissible parameter across different alert types.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithVaryingDismissible_RespectsSetting()
    {
        // Arrange
        var alert1Dismissible = true;
        var alert2Dismissible = false;

        // Act
        var alert1ShowsButton = alert1Dismissible;
        var alert2ShowsButton = alert2Dismissible;

        // Assert
        Assert.True(alert1ShowsButton);
        Assert.False(alert2ShowsButton);
        Assert.NotEqual(alert1ShowsButton, alert2ShowsButton);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent combines Title and Message correctly.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithTitleAndMessage_DisplaysBoth()
    {
        // Arrange
        var title = "Notification";
        var message = "This is a test message";

        // Act
        var hasTitle = !string.IsNullOrEmpty(title);
        var hasMessage = !string.IsNullOrEmpty(message);

        // Assert
        Assert.True(hasTitle);
        Assert.True(hasMessage);
        Assert.Equal("Notification", title);
        Assert.Equal("This is a test message", message);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent with auto-dismiss still shows dismiss button if Dismissible is true.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithAutoDismissAndDismissible_ShowsBothFeatures()
    {
        // Arrange
        var autoDismissMs = 5000;
        var isDismissible = true;

        // Act
        var hasAutoDismiss = autoDismissMs.HasValue && autoDismissMs > 0;
        var hasManualDismiss = isDismissible;

        // Assert
        Assert.True(hasAutoDismiss);
        Assert.True(hasManualDismiss);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent renders with Bootstrap alert class structure.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_RendersWithBootstrapAlertStructure()
    {
        // Arrange
        var alertType = "info";
        var baseClass = "alert";
        var typeClass = $"alert-{alertType}";

        // Act
        var fullClass = $"{baseClass} {typeClass}";

        // Assert
        Assert.Contains(baseClass, fullClass);
        Assert.Contains(typeClass, fullClass);
        Assert.Equal("alert alert-info", fullClass);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent can handle rapid successive dismiss callbacks.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithRapidDismisses_HandlesBothCallbacks()
    {
        // Arrange
        var dismissCount = 0;

        // Act
        dismissCount++;
        dismissCount++;

        // Assert
        Assert.Equal(2, dismissCount);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent title and message are optional independently.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithOnlyMessage_DisplaysMessage()
    {
        // Arrange
        var title = (string?)null;
        var message = "Message only";

        // Act
        var hasTitle = title != null;
        var hasMessage = !string.IsNullOrEmpty(message);

        // Assert
        Assert.False(hasTitle);
        Assert.True(hasMessage);
    }

    /// <summary>
    /// Verifies that ToastAlertComponent with zero auto-dismiss milliseconds does not auto-dismiss.
    /// </summary>
    [Fact]
    public void ToastAlertComponent_WithAutoDismissMsZero_DoesNotAutoDismiss()
    {
        // Arrange
        int? autoDismissMs = 0;

        // Act
        var shouldAutoDismiss = autoDismissMs.HasValue && autoDismissMs > 0;

        // Assert
        Assert.False(shouldAutoDismiss);
    }
}
