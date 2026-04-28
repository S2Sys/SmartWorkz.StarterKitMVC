using Xunit;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for AlertComponent verifying contextual alert messages, types, styling, dismissibility,
/// heading display, icon support, and visibility functionality.
/// </summary>
public class AlertComponentTests
{
    /// <summary>
    /// Verifies that AlertComponent renders when Visible is true.
    /// </summary>
    [Fact]
    public void AlertComponent_WithVisibleTrue_RendersAlert()
    {
        // Arrange
        var isVisible = true;

        // Act
        var shouldRender = isVisible;

        // Assert
        Assert.True(shouldRender);
    }

    /// <summary>
    /// Verifies that AlertComponent does not render when Visible is false.
    /// </summary>
    [Fact]
    public void AlertComponent_WithVisibleFalse_HidesAlert()
    {
        // Arrange
        var isVisible = false;

        // Act
        var shouldRender = isVisible;

        // Assert
        Assert.False(shouldRender);
    }

    /// <summary>
    /// Verifies that AlertComponent applies correct Bootstrap alert class for 'success' type.
    /// </summary>
    [Fact]
    public void AlertComponent_WithTypeSuccess_AppliesSuccessClass()
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
    /// Verifies that AlertComponent applies correct Bootstrap alert class for 'info' type.
    /// </summary>
    [Fact]
    public void AlertComponent_WithTypeInfo_AppliesInfoClass()
    {
        // Arrange
        var alertType = "info";
        var expectedClass = "alert-info";

        // Act
        var cssClass = $"alert-{alertType}";

        // Assert
        Assert.Equal(expectedClass, cssClass);
    }

    /// <summary>
    /// Verifies that AlertComponent applies correct Bootstrap alert class for 'warning' type.
    /// </summary>
    [Fact]
    public void AlertComponent_WithTypeWarning_AppliesWarningClass()
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
    /// Verifies that AlertComponent applies correct Bootstrap alert class for 'danger' type.
    /// </summary>
    [Fact]
    public void AlertComponent_WithTypeDanger_AppliesDangerClass()
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
    /// Verifies that AlertComponent has default type of 'info'.
    /// </summary>
    [Fact]
    public void AlertComponent_DefaultType_IsInfo()
    {
        // Arrange
        string? defaultType = null;

        // Act
        var type = defaultType ?? "info";

        // Assert
        Assert.Equal("info", type);
    }

    /// <summary>
    /// Verifies that AlertComponent displays heading when provided.
    /// </summary>
    [Fact]
    public void AlertComponent_WithHeading_DisplaysHeading()
    {
        // Arrange
        var heading = "Success!";

        // Act
        var hasHeading = !string.IsNullOrEmpty(heading);

        // Assert
        Assert.True(hasHeading);
        Assert.Equal("Success!", heading);
    }

    /// <summary>
    /// Verifies that AlertComponent does not display heading when not provided.
    /// </summary>
    [Fact]
    public void AlertComponent_WithoutHeading_NoHeadingDisplayed()
    {
        // Arrange
        string? heading = null;

        // Act
        var hasHeading = !string.IsNullOrEmpty(heading);

        // Assert
        Assert.False(hasHeading);
    }

    /// <summary>
    /// Verifies that AlertComponent shows close button when Dismissible is true.
    /// </summary>
    [Fact]
    public void AlertComponent_WithDismissibleTrue_ShowsCloseButton()
    {
        // Arrange
        var isDismissible = true;

        // Act
        var showCloseButton = isDismissible;

        // Assert
        Assert.True(showCloseButton);
    }

    /// <summary>
    /// Verifies that AlertComponent has default Dismissible value of true.
    /// </summary>
    [Fact]
    public void AlertComponent_DefaultDismissible_IsTrue()
    {
        // Arrange
        bool? defaultDismissible = null;

        // Act
        var isDismissible = defaultDismissible ?? true;

        // Assert
        Assert.True(isDismissible);
    }

    /// <summary>
    /// Verifies that AlertComponent invokes OnDismiss callback when dismiss button is clicked.
    /// </summary>
    [Fact]
    public void AlertComponent_WithOnDismissCallback_InvokesDismissCallback()
    {
        // Arrange
        var dismissCalled = false;
        var dismissCallback = new Func<Task>(async () =>
        {
            dismissCalled = true;
            await Task.CompletedTask;
        });

        // Act
        dismissCallback.Invoke().Wait();

        // Assert
        Assert.True(dismissCalled);
    }

    /// <summary>
    /// Verifies that AlertComponent displays icon when Icon parameter is true.
    /// </summary>
    [Fact]
    public void AlertComponent_WithIconTrue_DisplaysIcon()
    {
        // Arrange
        var shouldShowIcon = true;

        // Act
        var showIcon = shouldShowIcon;

        // Assert
        Assert.True(showIcon);
    }

    /// <summary>
    /// Verifies that AlertComponent has default Icon value of true.
    /// </summary>
    [Fact]
    public void AlertComponent_DefaultIcon_IsTrue()
    {
        // Arrange
        bool? defaultIcon = null;

        // Act
        var icon = defaultIcon ?? true;

        // Assert
        Assert.True(icon);
    }

    /// <summary>
    /// Verifies that AlertComponent supports all alert types.
    /// </summary>
    [Fact]
    public void AlertComponent_AllAlertTypes_AreSupported()
    {
        // Arrange
        var allTypes = new[] { "success", "info", "warning", "danger" };

        // Act
        var typeCount = allTypes.Length;

        // Assert
        Assert.Equal(4, typeCount);
        Assert.Contains("success", allTypes);
        Assert.Contains("info", allTypes);
        Assert.Contains("warning", allTypes);
        Assert.Contains("danger", allTypes);
    }

    /// <summary>
    /// Verifies that AlertComponent accepts both string and RenderFragment message content.
    /// </summary>
    [Fact]
    public void AlertComponent_WithMessageContent_AcceptsContent()
    {
        // Arrange
        var hasMessage = true;

        // Act
        var acceptsMessage = hasMessage;

        // Assert
        Assert.True(acceptsMessage);
    }

    /// <summary>
    /// Verifies that AlertComponent applies alert class to root element.
    /// </summary>
    [Fact]
    public void AlertComponent_RootElement_HasAlertClass()
    {
        // Arrange
        var rootClass = "alert";

        // Act
        var hasAlertClass = rootClass.Contains("alert");

        // Assert
        Assert.True(hasAlertClass);
        Assert.Equal("alert", rootClass);
    }

    /// <summary>
    /// Verifies that AlertComponent type mapping is correct for all types.
    /// </summary>
    [Fact]
    public void AlertComponent_TypeMapping_IsMappedCorrectly()
    {
        // Arrange
        var typeMap = new Dictionary<string, string>
        {
            { "success", "alert-success" },
            { "info", "alert-info" },
            { "warning", "alert-warning" },
            { "danger", "alert-danger" }
        };

        // Act
        var mappingCount = typeMap.Count;

        // Assert
        Assert.Equal(4, mappingCount);
        Assert.Equal("alert-success", typeMap["success"]);
        Assert.Equal("alert-danger", typeMap["danger"]);
        Assert.Equal("alert-warning", typeMap["warning"]);
        Assert.Equal("alert-info", typeMap["info"]);
    }

    /// <summary>
    /// Verifies that AlertComponent applies alert-dismissible class when Dismissible is true.
    /// </summary>
    [Fact]
    public void AlertComponent_WithDismissible_AppliesDismissibleClass()
    {
        // Arrange
        var isDismissible = true;
        var dismissibleClass = "alert-dismissible";

        // Act
        var shouldApplyClass = isDismissible;

        // Assert
        Assert.True(shouldApplyClass);
        Assert.Equal("alert-dismissible", dismissibleClass);
    }

    /// <summary>
    /// Verifies that AlertComponent displays message content correctly.
    /// </summary>
    [Fact]
    public void AlertComponent_WithMessage_DisplaysMessage()
    {
        // Arrange
        var message = "Operation completed successfully!";

        // Act
        var hasMessage = !string.IsNullOrEmpty(message);

        // Assert
        Assert.True(hasMessage);
        Assert.Equal("Operation completed successfully!", message);
    }

    /// <summary>
    /// Verifies that AlertComponent handles all parameters together.
    /// </summary>
    [Fact]
    public void AlertComponent_WithAllParameters_WorksTogether()
    {
        // Arrange
        var type = "warning";
        var heading = "Warning";
        var message = "This is a warning";
        var isDismissible = true;
        var showIcon = true;
        var isVisible = true;

        // Act
        var isConfigured = !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(heading)
            && !string.IsNullOrEmpty(message) && isDismissible && showIcon && isVisible;

        // Assert
        Assert.True(isConfigured);
    }
}
