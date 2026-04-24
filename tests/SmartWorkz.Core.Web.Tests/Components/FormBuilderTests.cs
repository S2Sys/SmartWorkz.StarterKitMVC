using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

public class FormBuilderTests
{
    [Fact]
    public void FormDefinition_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var form = new FormDefinition
        {
            Title = "Test Form",
            Description = "Test Description"
        };

        // Assert
        Assert.NotEmpty(form.Id);
        Assert.Equal("Test Form", form.Title);
        Assert.Equal("Test Description", form.Description);
        Assert.NotNull(form.Fields);
        Assert.Empty(form.Fields);
        Assert.NotNull(form.SubmitConfig);
        Assert.False(form.IsDisabled);
    }

    [Fact]
    public void FormDefinition_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var form1 = new FormDefinition { Title = "Form 1" };
        var form2 = new FormDefinition { Title = "Form 2" };

        // Assert
        Assert.NotEqual(form1.Id, form2.Id);
    }

    [Fact]
    public void FormField_ShouldSupportMultipleFieldTypes()
    {
        // Arrange
        var fieldTypes = new[] { "text", "email", "password", "number", "select", "checkbox", "textarea", "date" };

        // Act & Assert
        foreach (var type in fieldTypes)
        {
            var field = new FormField { Name = $"field_{type}", Type = type };
            Assert.Equal(type, field.Type);
        }
    }

    [Fact]
    public void FormField_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var field = new FormField { Name = "testField", Label = "Test Field" };

        // Assert
        Assert.Equal("testField", field.Name);
        Assert.Equal("Test Field", field.Label);
        Assert.Equal("text", field.Type);
        Assert.False(field.IsRequired);
        Assert.False(field.IsDisabled);
        Assert.True(field.IsVisible);
        Assert.NotNull(field.ValidationRules);
        Assert.NotNull(field.Options);
    }

    [Fact]
    public void FormField_ShouldSupportConditionalVisibility()
    {
        // Arrange
        var field = new FormField
        {
            Name = "address",
            DependsOn = "hasAddress",
            DependsOnValue = "true"
        };

        // Act & Assert
        Assert.Equal("hasAddress", field.DependsOn);
        Assert.Equal("true", field.DependsOnValue?.ToString());
    }

    [Fact]
    public void FormFieldOption_ShouldCreateOptions()
    {
        // Arrange & Act
        var option = new FormFieldOption
        {
            Label = "Option 1",
            Value = "opt1",
            IsDisabled = false
        };

        // Assert
        Assert.Equal("Option 1", option.Label);
        Assert.Equal("opt1", option.Value?.ToString());
        Assert.False(option.IsDisabled);
    }

    [Fact]
    public void FormValidationRule_ShouldSupportMultipleRuleTypes()
    {
        // Arrange
        var ruleTypes = new[] { "required", "email", "minLength", "maxLength", "pattern", "custom", "min", "max" };

        // Act & Assert
        foreach (var type in ruleTypes)
        {
            var rule = new FormValidationRule { Type = type, Message = $"{type} validation failed" };
            Assert.Equal(type, rule.Type);
        }
    }

    [Fact]
    public void FormValidationRule_ShouldStoreValidationValues()
    {
        // Arrange & Act
        var rule = new FormValidationRule
        {
            Type = "minLength",
            Value = "5",
            Message = "Minimum 5 characters required"
        };

        // Assert
        Assert.Equal("minLength", rule.Type);
        Assert.Equal("5", rule.Value);
        Assert.Equal("Minimum 5 characters required", rule.Message);
    }

    [Fact]
    public void FormSubmitConfig_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var config = new FormSubmitConfig();

        // Assert
        Assert.Equal("Submit", config.SubmitButtonText);
        Assert.Equal("Cancel", config.CancelButtonText);
        Assert.True(config.ShowCancelButton);
        Assert.False(config.ShowResetButton);
        Assert.Equal("btn btn-primary", config.SubmitButtonClass);
        Assert.Equal("btn btn-secondary", config.CancelButtonClass);
    }

    [Fact]
    public void FormSubmitConfig_ShouldAllowCustomization()
    {
        // Arrange & Act
        var config = new FormSubmitConfig
        {
            SubmitButtonText = "Save",
            CancelButtonText = "Discard",
            ShowResetButton = true
        };

        // Assert
        Assert.Equal("Save", config.SubmitButtonText);
        Assert.Equal("Discard", config.CancelButtonText);
        Assert.True(config.ShowResetButton);
    }

    [Fact]
    public void FormSubmissionResult_ShouldTrackSuccess()
    {
        // Arrange & Act
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            Message = "Submitted successfully",
            Data = new Dictionary<string, object?> { { "name", "John" } }
        };

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Submitted successfully", result.Message);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data["name"]);
    }

    [Fact]
    public void FormSubmissionResult_ShouldTrackErrors()
    {
        // Arrange & Act
        var result = new FormSubmissionResult
        {
            IsSuccess = false,
            Message = "Validation failed",
            FieldErrors = new Dictionary<string, List<string>>
            {
                { "email", new List<string> { "Invalid email format" } },
                { "name", new List<string> { "Name is required", "Must be at least 3 characters" } }
            }
        };

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.FieldErrors.Count);
        Assert.Single(result.FieldErrors["email"]);
        Assert.Equal(2, result.FieldErrors["name"].Count);
    }

    [Fact]
    public void FormSubmissionResult_ShouldSupportMetadata()
    {
        // Arrange & Act
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            Metadata = new Dictionary<string, object?>
            {
                { "submittedAt", DateTime.UtcNow },
                { "userId", 123 }
            }
        };

        // Assert
        Assert.Equal(2, result.Metadata.Count);
        Assert.Equal(123, result.Metadata["userId"]);
    }

    [Fact]
    public void FormValidationError_ShouldCaptureFieldErrors()
    {
        // Arrange & Act
        var error = new FormValidationError
        {
            FieldName = "email",
            Message = "Invalid email format",
            ErrorCode = "INVALID_EMAIL"
        };

        // Assert
        Assert.Equal("email", error.FieldName);
        Assert.Equal("Invalid email format", error.Message);
        Assert.Equal("INVALID_EMAIL", error.ErrorCode);
    }

    [Fact]
    public void FormDefinition_ShouldSupportCompleteWorkflow()
    {
        // Arrange
        var form = new FormDefinition
        {
            Title = "Contact Form",
            Description = "Please fill in your details",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "name",
                    Label = "Full Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "3",
                            Message = "Name must be at least 3 characters"
                        }
                    }
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "email",
                            Message = "Please enter a valid email"
                        }
                    }
                },
                new FormField
                {
                    Name = "message",
                    Label = "Message",
                    Type = "textarea",
                    IsRequired = false,
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "maxLength",
                            Value = "500",
                            Message = "Message must not exceed 500 characters"
                        }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Send Message",
                ShowCancelButton = true
            }
        };

        // Act
        var validFields = form.Fields.Where(f => f.IsRequired).ToList();
        var orderedFields = form.Fields.OrderBy(f => f.Order).ToList();

        // Assert
        Assert.Equal(3, form.Fields.Count);
        Assert.Equal(2, validFields.Count);
        Assert.Equal("name", orderedFields[0].Name);
        Assert.Equal("email", orderedFields[1].Name);
        Assert.Equal("message", orderedFields[2].Name);
        Assert.Equal("Send Message", form.SubmitConfig.SubmitButtonText);
    }

    [Fact]
    public void FormField_ShouldSupportSelectOptionsWithDisabledStates()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "status",
            Type = "select",
            Options = new List<FormFieldOption>
            {
                new FormFieldOption { Label = "Active", Value = "active" },
                new FormFieldOption { Label = "Inactive", Value = "inactive", IsDisabled = true },
                new FormFieldOption { Label = "Pending", Value = "pending" }
            }
        };

        // Assert
        Assert.Equal(3, field.Options.Count);
        Assert.False(field.Options[0].IsDisabled);
        Assert.True(field.Options[1].IsDisabled);
        Assert.False(field.Options[2].IsDisabled);
    }

    [Fact]
    public void FormField_ShouldSupportDependentFieldLogic()
    {
        // Arrange
        var fields = new List<FormField>
        {
            new FormField
            {
                Name = "country",
                Label = "Country",
                Type = "select",
                Value = "US",
                Order = 1
            },
            new FormField
            {
                Name = "state",
                Label = "State",
                Type = "select",
                DependsOn = "country",
                DependsOnValue = "US",
                Order = 2
            },
            new FormField
            {
                Name = "province",
                Label = "Province",
                Type = "select",
                DependsOn = "country",
                DependsOnValue = "CA",
                Order = 3
            }
        };

        // Act
        var countryField = fields.First(f => f.Name == "country");
        var visibleFields = fields.Where(f =>
            string.IsNullOrEmpty(f.DependsOn) ||
            (f.DependsOn == countryField.Name && f.DependsOnValue?.ToString() == countryField.Value?.ToString())
        ).ToList();

        // Assert
        Assert.Equal(2, visibleFields.Count);
        Assert.Contains(visibleFields, f => f.Name == "state");
        Assert.DoesNotContain(visibleFields, f => f.Name == "province");
    }

    [Fact]
    public void FormSubmissionResult_ShouldSupportRedirectUrl()
    {
        // Arrange & Act
        var result = new FormSubmissionResult
        {
            IsSuccess = true,
            Message = "Form submitted",
            RedirectUrl = "/confirmation"
        };

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("/confirmation", result.RedirectUrl);
    }
}
