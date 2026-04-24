using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

public class FormBuilderComponentTests
{
    [Fact]
    public void FormBuilderComponent_ShouldInitializeWithFormDefinition()
    {
        // Arrange
        var form = new FormDefinition
        {
            Title = "User Registration",
            Description = "Please fill in your details",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "firstName",
                    Label = "First Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 1
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    Order = 2
                }
            }
        };

        // Act
        var fieldCount = form.Fields.Count;
        var requiredFields = form.Fields.Where(f => f.IsRequired).Count();

        // Assert
        Assert.Equal(2, fieldCount);
        Assert.Equal(2, requiredFields);
        Assert.Equal("User Registration", form.Title);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportFieldTypeVariety()
    {
        // Arrange
        var form = new FormDefinition
        {
            Title = "Complete Form",
            Fields = new List<FormField>
            {
                new FormField { Name = "name", Type = "text" },
                new FormField { Name = "email", Type = "email" },
                new FormField { Name = "password", Type = "password" },
                new FormField { Name = "age", Type = "number" },
                new FormField { Name = "country", Type = "select" },
                new FormField { Name = "subscribe", Type = "checkbox" },
                new FormField { Name = "comments", Type = "textarea" },
                new FormField { Name = "dob", Type = "date" }
            }
        };

        // Act & Assert
        Assert.All(form.Fields, field => Assert.False(string.IsNullOrEmpty(field.Type)));
        Assert.Equal(8, form.Fields.Count);
        Assert.Single(form.Fields.Where(f => f.Type == "email"));
        Assert.Single(form.Fields.Where(f => f.Type == "checkbox"));
    }

    [Fact]
    public void FormBuilderComponent_ShouldHandleConditionalVisibility()
    {
        // Arrange
        var form = new FormDefinition
        {
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "hasAddress",
                    Label = "Do you have an address?",
                    Type = "checkbox",
                    Order = 1
                },
                new FormField
                {
                    Name = "address",
                    Label = "Street Address",
                    Type = "text",
                    DependsOn = "hasAddress",
                    DependsOnValue = true,
                    Order = 2
                }
            }
        };

        // Act
        var dependentField = form.Fields.First(f => f.Name == "address");
        var isConditional = !string.IsNullOrEmpty(dependentField.DependsOn);

        // Assert
        Assert.True(isConditional);
        Assert.Equal("hasAddress", dependentField.DependsOn);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportMultipleValidationRules()
    {
        // Arrange
        var form = new FormDefinition
        {
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "username",
                    Type = "text",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "3",
                            Message = "Username must be at least 3 characters"
                        },
                        new FormValidationRule
                        {
                            Type = "maxLength",
                            Value = "20",
                            Message = "Username cannot exceed 20 characters"
                        },
                        new FormValidationRule
                        {
                            Type = "pattern",
                            Value = "^[a-zA-Z0-9_]*$",
                            Message = "Username can only contain letters, numbers, and underscores"
                        }
                    }
                }
            }
        };

        // Act
        var field = form.Fields.First();
        var ruleCount = field.ValidationRules.Count;

        // Assert
        Assert.Equal(3, ruleCount);
        Assert.True(field.ValidationRules.Any(r => r.Type == "minLength"));
        Assert.True(field.ValidationRules.Any(r => r.Type == "maxLength"));
        Assert.True(field.ValidationRules.Any(r => r.Type == "pattern"));
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportSelectOptions()
    {
        // Arrange
        var form = new FormDefinition
        {
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "status",
                    Type = "select",
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Active", Value = "active" },
                        new FormFieldOption { Label = "Inactive", Value = "inactive" },
                        new FormFieldOption { Label = "Pending", Value = "pending" }
                    }
                }
            }
        };

        // Act
        var field = form.Fields.First();
        var optionCount = field.Options.Count;

        // Assert
        Assert.Equal(3, optionCount);
        Assert.Contains(field.Options, o => o.Value?.ToString() == "active");
        Assert.Contains(field.Options, o => o.Label == "Pending");
    }

    [Fact]
    public void FormBuilderComponent_ShouldTrackFormSubmissionResult()
    {
        // Arrange
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            Message = "Form submitted successfully",
            Data = new Dictionary<string, object?>
            {
                { "firstName", "John" },
                { "lastName", "Doe" },
                { "email", "john@example.com" }
            }
        };

        // Act
        var success = result.IsSuccess;
        var dataCount = result.Data.Count;

        // Assert
        Assert.True(success);
        Assert.Equal(3, dataCount);
        Assert.Equal("John", result.Data["firstName"]);
    }

    [Fact]
    public void FormBuilderComponent_ShouldTrackFieldValidationErrors()
    {
        // Arrange
        var result = new FormSubmissionResult
        {
            IsSuccess = false,
            Message = "Validation failed",
            FieldErrors = new Dictionary<string, List<string>>
            {
                { "email", new List<string> { "Invalid email format" } },
                { "password", new List<string> { "Password must be at least 8 characters", "Must contain uppercase letter" } },
                { "firstName", new List<string> { "First name is required" } }
            }
        };

        // Act
        var errorCount = result.FieldErrors.Count;
        var passwordErrors = result.FieldErrors["password"];

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(3, errorCount);
        Assert.Equal(2, passwordErrors.Count);
        Assert.Contains("uppercase", passwordErrors[1], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportFieldOrdering()
    {
        // Arrange
        var form = new FormDefinition
        {
            Fields = new List<FormField>
            {
                new FormField { Name = "email", Order = 3 },
                new FormField { Name = "firstName", Order = 1 },
                new FormField { Name = "lastName", Order = 2 }
            }
        };

        // Act
        var orderedFields = form.Fields.OrderBy(f => f.Order).Select(f => f.Name).ToList();

        // Assert
        Assert.Equal(new[] { "firstName", "lastName", "email" }, orderedFields);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportCustomCssClasses()
    {
        // Arrange
        var form = new FormDefinition
        {
            CssClass = "custom-form-class",
            Fields = new List<FormField>
            {
                new FormField { Name = "name", CssClass = "custom-field-class" }
            }
        };

        // Act
        var formCss = form.CssClass;
        var fieldCss = form.Fields.First().CssClass;

        // Assert
        Assert.Equal("custom-form-class", formCss);
        Assert.Equal("custom-field-class", fieldCss);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportFormDisabling()
    {
        // Arrange
        var form = new FormDefinition
        {
            Title = "Locked Form",
            IsDisabled = true
        };

        var field = new FormField
        {
            Name = "disabledField",
            IsDisabled = true
        };

        // Act & Assert
        Assert.True(form.IsDisabled);
        Assert.True(field.IsDisabled);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportSubmitButtonCustomization()
    {
        // Arrange
        var config = new FormSubmitConfig
        {
            SubmitButtonText = "Save Changes",
            SubmitButtonClass = "btn btn-success",
            CancelButtonText = "Go Back",
            CancelButtonClass = "btn btn-outline-danger",
            ShowResetButton = true,
            ResetButtonClass = "btn btn-warning"
        };

        // Act & Assert
        Assert.Equal("Save Changes", config.SubmitButtonText);
        Assert.Equal("btn btn-success", config.SubmitButtonClass);
        Assert.Equal("Go Back", config.CancelButtonText);
        Assert.True(config.ShowResetButton);
        Assert.Equal("btn btn-warning", config.ResetButtonClass);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportHelpTextAndPlaceholders()
    {
        // Arrange
        var field = new FormField
        {
            Name = "phone",
            Type = "text",
            Label = "Phone Number",
            Placeholder = "+1 (555) 000-0000",
            HelpText = "Enter your phone number in the format shown"
        };

        // Act & Assert
        Assert.Equal("Phone Number", field.Label);
        Assert.Equal("+1 (555) 000-0000", field.Placeholder);
        Assert.Equal("Enter your phone number in the format shown", field.HelpText);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportFormMetadata()
    {
        // Arrange
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            Data = new Dictionary<string, object?> { { "name", "Test" } },
            Metadata = new Dictionary<string, object?>
            {
                { "submittedAt", DateTime.UtcNow },
                { "userId", 42 },
                { "source", "mobile-app" }
            }
        };

        // Act
        var metadataCount = result.Metadata.Count;
        var userId = result.Metadata["userId"];

        // Assert
        Assert.Equal(3, metadataCount);
        Assert.Equal(42, userId);
    }

    [Fact]
    public void FormBuilderComponent_ShouldSupportRedirectAfterSubmission()
    {
        // Arrange
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            RedirectUrl = "/thank-you"
        };

        // Act & Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("/thank-you", result.RedirectUrl);
    }

    [Fact]
    public void FormBuilderComponent_ShouldTrackValidationErrorDetails()
    {
        // Arrange
        var error = new FormValidationError
        {
            FieldName = "email",
            Message = "Invalid email format",
            ErrorCode = "INVALID_EMAIL"
        };

        // Act & Assert
        Assert.Equal("email", error.FieldName);
        Assert.Equal("Invalid email format", error.Message);
        Assert.Equal("INVALID_EMAIL", error.ErrorCode);
    }
}
