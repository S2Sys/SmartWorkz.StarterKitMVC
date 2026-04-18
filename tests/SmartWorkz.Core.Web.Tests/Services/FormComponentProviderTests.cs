namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Core.Web.Services.Components;
using Xunit;

public class FormComponentProviderTests
{
    private readonly FormComponentProvider _formComponentProvider;

    public FormComponentProviderTests()
    {
        _formComponentProvider = new FormComponentProvider();
    }

    [Fact]
    public void GetConfiguration_ReturnsDefaultFormComponentConfigWithAllDefaultBootstrapClasses()
    {
        // Act
        var config = _formComponentProvider.GetConfiguration();

        // Assert
        Assert.NotNull(config);
        Assert.Equal("form-control", config.InputClass);
        Assert.Equal("form-control-sm", config.InputSmallClass);
        Assert.Equal("form-control-lg", config.InputLargeClass);
        Assert.Equal("form-label", config.LabelClass);
        Assert.Equal("btn", config.ButtonClass);
        Assert.Equal("btn-primary", config.ButtonPrimaryClass);
        Assert.Equal("btn-secondary", config.ButtonSecondaryClass);
        Assert.Equal("btn-danger", config.ButtonDangerClass);
        Assert.Equal("btn-success", config.ButtonSuccessClass);
        Assert.Equal("btn-warning", config.ButtonWarningClass);
        Assert.Equal("is-invalid", config.ValidationErrorClass);
        Assert.Equal("is-valid", config.ValidationSuccessClass);
        Assert.Equal("mb-3", config.FormGroupClass);
        Assert.Equal("alert-success", config.AlertSuccessClass);
        Assert.Equal("alert-danger", config.AlertErrorClass);
        Assert.Equal("alert-warning", config.AlertWarningClass);
        Assert.Equal("alert-info", config.AlertInfoClass);
    }

    [Fact]
    public void UpdateConfiguration_ChangesTheConfigurationCorrectly()
    {
        // Arrange
        var newConfig = new FormComponentConfig
        {
            InputClass = "custom-input",
            InputSmallClass = "custom-input-sm",
            InputLargeClass = "custom-input-lg",
            LabelClass = "custom-label",
            ButtonClass = "custom-btn",
            ButtonPrimaryClass = "custom-btn-primary",
            ButtonSecondaryClass = "custom-btn-secondary",
            ButtonDangerClass = "custom-btn-danger",
            ButtonSuccessClass = "custom-btn-success",
            ButtonWarningClass = "custom-btn-warning",
            ValidationErrorClass = "custom-error",
            ValidationSuccessClass = "custom-success",
            FormGroupClass = "custom-form-group",
            AlertSuccessClass = "custom-alert-success",
            AlertErrorClass = "custom-alert-danger",
            AlertWarningClass = "custom-alert-warning",
            AlertInfoClass = "custom-alert-info"
        };

        // Act
        _formComponentProvider.UpdateConfiguration(newConfig);
        var result = _formComponentProvider.GetConfiguration();

        // Assert
        Assert.Equal("custom-input", result.InputClass);
        Assert.Equal("custom-input-sm", result.InputSmallClass);
        Assert.Equal("custom-input-lg", result.InputLargeClass);
        Assert.Equal("custom-label", result.LabelClass);
        Assert.Equal("custom-btn", result.ButtonClass);
        Assert.Equal("custom-btn-primary", result.ButtonPrimaryClass);
        Assert.Equal("custom-btn-secondary", result.ButtonSecondaryClass);
        Assert.Equal("custom-btn-danger", result.ButtonDangerClass);
        Assert.Equal("custom-btn-success", result.ButtonSuccessClass);
        Assert.Equal("custom-btn-warning", result.ButtonWarningClass);
        Assert.Equal("custom-error", result.ValidationErrorClass);
        Assert.Equal("custom-success", result.ValidationSuccessClass);
        Assert.Equal("custom-form-group", result.FormGroupClass);
        Assert.Equal("custom-alert-success", result.AlertSuccessClass);
        Assert.Equal("custom-alert-danger", result.AlertErrorClass);
        Assert.Equal("custom-alert-warning", result.AlertWarningClass);
        Assert.Equal("custom-alert-info", result.AlertInfoClass);
    }

    [Fact]
    public void UpdateConfiguration_WithNullThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => _formComponentProvider.UpdateConfiguration(null!));
        Assert.Equal("config", exception.ParamName);
    }

    [Fact]
    public void GetConfiguration_ReturnsTheUpdatedConfigAfterUpdateConfiguration()
    {
        // Arrange
        var originalConfig = _formComponentProvider.GetConfiguration();
        Assert.Equal("form-control", originalConfig.InputClass);

        var newConfig = new FormComponentConfig
        {
            InputClass = "updated-form-control"
        };

        // Act
        _formComponentProvider.UpdateConfiguration(newConfig);
        var updatedConfig = _formComponentProvider.GetConfiguration();

        // Assert
        Assert.Equal("updated-form-control", updatedConfig.InputClass);
    }

    [Fact]
    public void FormComponentConfig_PropertiesHaveCorrectDefaultValues()
    {
        // Act
        var config = new FormComponentConfig();

        // Assert - Verify all 18 properties have correct default values
        Assert.Equal("form-control", config.InputClass);
        Assert.Equal("form-control-sm", config.InputSmallClass);
        Assert.Equal("form-control-lg", config.InputLargeClass);
        Assert.Equal("form-label", config.LabelClass);
        Assert.Equal("btn", config.ButtonClass);
        Assert.Equal("btn-primary", config.ButtonPrimaryClass);
        Assert.Equal("btn-secondary", config.ButtonSecondaryClass);
        Assert.Equal("btn-danger", config.ButtonDangerClass);
        Assert.Equal("btn-success", config.ButtonSuccessClass);
        Assert.Equal("btn-warning", config.ButtonWarningClass);
        Assert.Equal("is-invalid", config.ValidationErrorClass);
        Assert.Equal("is-valid", config.ValidationSuccessClass);
        Assert.Equal("mb-3", config.FormGroupClass);
        Assert.Equal("alert-success", config.AlertSuccessClass);
        Assert.Equal("alert-danger", config.AlertErrorClass);
        Assert.Equal("alert-warning", config.AlertWarningClass);
        Assert.Equal("alert-info", config.AlertInfoClass);
    }
}
