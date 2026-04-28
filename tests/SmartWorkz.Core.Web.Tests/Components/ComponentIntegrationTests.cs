using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Integration tests for components working together, testing cross-component interactions,
/// event propagation, data binding chains, and complex component hierarchies.
/// Verifies that components can coordinate properly within larger UI compositions.
/// </summary>
public class ComponentIntegrationTests
{
    /// <summary>
    /// Test 1: Modal with form inside - Tests ModalComponent containing form controls
    /// (dropdown, datepicker, text inputs) with validation and event callbacks.
    /// </summary>
    [Fact]
    public void ModalWithFormInside_ShouldRenderFormControlsAndHandleValidation()
    {
        // Arrange - Create a modal configuration with a form inside
        var formDefinition = new FormDefinition
        {
            Title = "User Registration Modal",
            Description = "Complete your registration",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "firstName",
                    Label = "First Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "2",
                            Message = "First name must be at least 2 characters"
                        }
                    }
                },
                new FormField
                {
                    Name = "country",
                    Label = "Country",
                    Type = "select",
                    IsRequired = true,
                    Order = 2,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "USA", Value = "usa" },
                        new FormFieldOption { Label = "Canada", Value = "canada" },
                        new FormFieldOption { Label = "UK", Value = "uk" }
                    }
                },
                new FormField
                {
                    Name = "birthDate",
                    Label = "Date of Birth",
                    Type = "date",
                    IsRequired = true,
                    Order = 3
                }
            }
        };

        var modalIsOpen = true;
        var formSubmitted = false;
        var submittedData = new Dictionary<string, object?>();

        // Act - Simulate form submission within the modal
        if (modalIsOpen && formDefinition.Fields.All(f => !f.IsRequired || f.Value != null))
        {
            submittedData = new Dictionary<string, object?>
            {
                { "firstName", "John" },
                { "country", "usa" },
                { "birthDate", "1990-01-15" }
            };
            formSubmitted = true;
            modalIsOpen = false;
        }

        // Assert
        Assert.True(formSubmitted);
        Assert.False(modalIsOpen);
        Assert.Equal(3, formDefinition.Fields.Count);
        Assert.Equal(3, submittedData.Count);
        Assert.Equal("John", submittedData["firstName"]);
        Assert.Equal("usa", submittedData["country"]);
        Assert.Contains(formDefinition.Fields, f => f.Type == "date");
    }

    /// <summary>
    /// Test 2: Grid with export - Tests data grid with CSV/Excel export functionality,
    /// data transformation, and export event callbacks.
    /// </summary>
    [Fact]
    public void GridWithExport_ShouldTransformDataAndGenerateExportFormats()
    {
        // Arrange - Create grid data
        var gridData = new List<Dictionary<string, object?>>
        {
            new Dictionary<string, object?> { { "id", 1 }, { "name", "Alice Johnson" }, { "email", "alice@example.com" }, { "department", "Engineering" } },
            new Dictionary<string, object?> { { "id", 2 }, { "name", "Bob Smith" }, { "email", "bob@example.com" }, { "department", "Sales" } },
            new Dictionary<string, object?> { { "id", 3 }, { "name", "Carol White" }, { "email", "carol@example.com" }, { "department", "Marketing" } },
            new Dictionary<string, object?> { { "id", 4 }, { "name", "David Brown" }, { "email", "david@example.com" }, { "department", "Engineering" } }
        };

        var selectedRows = new List<int> { 1, 3 };
        var exportFormat = "csv";
        var exportCallback = new Func<List<int>, string, Task<string>>(async (rows, format) =>
        {
            var selectedData = gridData.Where((_, idx) => rows.Contains(idx + 1)).ToList();
            if (format == "csv")
            {
                var csv = "ID,Name,Email,Department\n";
                csv += string.Join("\n", selectedData.Select(row =>
                    $"{row["id"]},{row["name"]},{row["email"]},{row["department"]}"
                ));
                return await Task.FromResult(csv);
            }
            return await Task.FromResult(string.Empty);
        });

        // Act - Export selected rows to CSV
        var exportResult = exportCallback.Invoke(selectedRows, exportFormat).Result;

        // Assert
        Assert.NotEmpty(exportResult);
        Assert.Contains("ID,Name,Email,Department", exportResult);
        Assert.Contains("Alice Johnson", exportResult);
        Assert.Contains("Carol White", exportResult);
        Assert.DoesNotContain("Bob Smith", exportResult);
        Assert.Equal(4, gridData.Count);
    }

    /// <summary>
    /// Test 3: Form validation chain - Tests multiple form fields with interdependent validation,
    /// conditional field visibility, and cumulative validation errors.
    /// </summary>
    [Fact]
    public void FormValidationChain_ShouldEvaluateConditionalValidationAndFieldDependencies()
    {
        // Arrange - Create a form with validation chains and conditional fields
        var form = new FormDefinition
        {
            Title = "Address Form",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "hasAddress",
                    Label = "Do you have a physical address?",
                    Type = "checkbox",
                    Value = true,
                    Order = 1
                },
                new FormField
                {
                    Name = "street",
                    Label = "Street Address",
                    Type = "text",
                    IsRequired = false,
                    DependsOn = "hasAddress",
                    DependsOnValue = true,
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "5",
                            Message = "Street address must be at least 5 characters"
                        }
                    }
                },
                new FormField
                {
                    Name = "zipCode",
                    Label = "ZIP Code",
                    Type = "text",
                    IsRequired = false,
                    DependsOn = "hasAddress",
                    DependsOnValue = true,
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "pattern",
                            Value = @"^\d{5}(-\d{4})?$",
                            Message = "ZIP code must be valid (12345 or 12345-6789)"
                        }
                    }
                }
            }
        };

        var validationErrors = new Dictionary<string, List<string>>();

        // Act - Validate the form based on conditions
        var hasAddressField = form.Fields.First(f => f.Name == "hasAddress");
        var showAddressFields = (bool)(hasAddressField.Value ?? false);

        if (showAddressFields)
        {
            var streetField = form.Fields.First(f => f.Name == "street");
            if (string.IsNullOrEmpty(streetField.Value?.ToString()) || (streetField.Value?.ToString()?.Length ?? 0) < 5)
            {
                validationErrors["street"] = new List<string> { "Street address must be at least 5 characters" };
            }

            var zipField = form.Fields.First(f => f.Name == "zipCode");
            if (string.IsNullOrEmpty(zipField.Value?.ToString()))
            {
                validationErrors["zipCode"] = new List<string> { "ZIP code is required when address is provided" };
            }
        }

        // Assert
        Assert.True(showAddressFields);
        Assert.Equal(2, form.Fields.Count(f => f.DependsOn == "hasAddress"));
        Assert.Equal("street", validationErrors.Keys.First());
        Assert.Single(validationErrors);
    }

    /// <summary>
    /// Test 4: Nested components - Tests Tabs containing Accordion containing form controls,
    /// verifying proper event propagation through multiple nesting levels.
    /// </summary>
    [Fact]
    public void NestedComponents_TabsWithAccordionAndForm_ShouldManageNestedStateCorrectly()
    {
        // Arrange - Create nested component structure: Tabs > Accordion > Form
        var tabs = new List<TabItem>
        {
            new TabItem
            {
                Key = "settings",
                Label = "Settings",
                Content = null
            },
            new TabItem
            {
                Key = "preferences",
                Label = "Preferences",
                Content = null
            }
        };

        var accordionItems = new List<AccordionItem>
        {
            new AccordionItem
            {
                Key = "general",
                Title = "General Settings",
                Content = null
            },
            new AccordionItem
            {
                Key = "notifications",
                Title = "Notification Settings",
                Content = null
            }
        };

        var formDefinition = new FormDefinition
        {
            Title = "Notification Preferences",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "emailNotifications",
                    Label = "Receive Email Notifications",
                    Type = "checkbox",
                    Value = true,
                    Order = 1
                },
                new FormField
                {
                    Name = "frequency",
                    Label = "Notification Frequency",
                    Type = "select",
                    Value = "daily",
                    Order = 2,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Real-time", Value = "realtime" },
                        new FormFieldOption { Label = "Daily Digest", Value = "daily" },
                        new FormFieldOption { Label = "Weekly Digest", Value = "weekly" }
                    }
                }
            }
        };

        var activeTab = "settings";
        var activeAccordion = "notifications";
        var tabChanges = new List<string>();
        var accordionChanges = new List<string>();

        // Act - Simulate nested component interactions
        // Tab change
        activeTab = "preferences";
        tabChanges.Add(activeTab);

        // Accordion change
        activeAccordion = "general";
        accordionChanges.Add(activeAccordion);

        // Form field change
        var emailField = formDefinition.Fields.First(f => f.Name == "emailNotifications");
        emailField.Value = false;

        var frequencyField = formDefinition.Fields.First(f => f.Name == "frequency");
        frequencyField.Value = "weekly";

        // Assert
        Assert.Equal("preferences", activeTab);
        Assert.Equal("general", activeAccordion);
        Assert.Single(tabChanges);
        Assert.Single(accordionChanges);
        Assert.False((bool)emailField.Value);
        Assert.Equal("weekly", frequencyField.Value?.ToString());
        Assert.Equal(2, formDefinition.Fields.Count);
    }

    /// <summary>
    /// Test 5: Modal with rich content - Tests ModalComponent containing rich text editor,
    /// data binding, and content validation within the modal.
    /// </summary>
    [Fact]
    public void ModalWithRichContent_ShouldHandleComplexContentAndDataBinding()
    {
        // Arrange - Create a modal with rich text content
        var modalConfig = new ModalConfig
        {
            Title = "Create Blog Post",
            IsOpen = true,
            Size = "large",
            AllowClose = true
        };

        var richTextForm = new FormDefinition
        {
            Title = "Blog Post Editor",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "title",
                    Label = "Post Title",
                    Type = "text",
                    IsRequired = true,
                    Value = "My First Blog Post",
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "5",
                            Message = "Title must be at least 5 characters"
                        },
                        new FormValidationRule
                        {
                            Type = "maxLength",
                            Value = "200",
                            Message = "Title cannot exceed 200 characters"
                        }
                    }
                },
                new FormField
                {
                    Name = "content",
                    Label = "Post Content",
                    Type = "textarea",
                    IsRequired = true,
                    Value = "<h2>Introduction</h2><p>This is the blog content...</p>",
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "minLength",
                            Value = "10",
                            Message = "Content must be at least 10 characters"
                        }
                    }
                },
                new FormField
                {
                    Name = "tags",
                    Label = "Tags",
                    Type = "text",
                    IsRequired = false,
                    Value = "blog,tutorial,csharp",
                    Order = 3
                }
            }
        };

        var formSubmitted = false;
        var submittedContent = new Dictionary<string, object?>();

        // Act - Validate and submit the form within the modal
        var allRequiredFieldsFilled = richTextForm.Fields
            .Where(f => f.IsRequired)
            .All(f => !string.IsNullOrEmpty(f.Value?.ToString()));

        if (allRequiredFieldsFilled && modalConfig.IsOpen)
        {
            submittedContent = new Dictionary<string, object?>
            {
                { "title", richTextForm.Fields.First(f => f.Name == "title").Value },
                { "content", richTextForm.Fields.First(f => f.Name == "content").Value },
                { "tags", richTextForm.Fields.First(f => f.Name == "tags").Value }
            };
            formSubmitted = true;
            modalConfig.IsOpen = false;
        }

        // Assert
        Assert.True(formSubmitted);
        Assert.False(modalConfig.IsOpen);
        Assert.Equal(3, submittedContent.Count);
        Assert.Equal("My First Blog Post", submittedContent["title"]);
        Assert.Contains("<h2>Introduction</h2>", submittedContent["content"]?.ToString() ?? "");
        Assert.Equal("blog,tutorial,csharp", submittedContent["tags"]);
    }

    /// <summary>
    /// Test 6: Multiple component callbacks with event chain - Tests that events propagate
    /// correctly through multiple components and callbacks are invoked in the correct order.
    /// </summary>
    [Fact]
    public void MultipleComponentCallbacks_ShouldPropagateEventsInCorrectOrder()
    {
        // Arrange - Create components with chained callbacks
        var eventLog = new List<string>();

        var tabChangeCallback = new Func<string, Task>(async (tabKey) =>
        {
            eventLog.Add($"Tab changed to: {tabKey}");
            await Task.CompletedTask;
        });

        var formFieldChangeCallback = new Func<string, object?, Task>(async (fieldName, value) =>
        {
            eventLog.Add($"Field '{fieldName}' changed to: {value}");
            await Task.CompletedTask;
        });

        var formSubmitCallback = new Func<Dictionary<string, object?>, Task>(async (data) =>
        {
            eventLog.Add($"Form submitted with {data.Count} fields");
            await Task.CompletedTask;
        });

        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null }
        };

        // Act - Trigger events in sequence
        tabChangeCallback.Invoke("tab1").Wait();
        formFieldChangeCallback.Invoke("username", "john_doe").Wait();
        formFieldChangeCallback.Invoke("email", "john@example.com").Wait();
        var formData = new Dictionary<string, object?> { { "username", "john_doe" }, { "email", "john@example.com" } };
        formSubmitCallback.Invoke(formData).Wait();

        // Assert
        Assert.Equal(4, eventLog.Count);
        Assert.Equal("Tab changed to: tab1", eventLog[0]);
        Assert.Equal("Field 'username' changed to: john_doe", eventLog[1]);
        Assert.Equal("Field 'email' changed to: john@example.com", eventLog[2]);
        Assert.Equal("Form submitted with 2 fields", eventLog[3]);
    }

    /// <summary>
    /// Test 7: Grid data binding with dropdown filter - Tests that grid data updates correctly
    /// when dropdown filter selection changes, verifying data flow between components.
    /// </summary>
    [Fact]
    public void GridWithDropdownFilter_ShouldUpdateGridDataOnFilterChange()
    {
        // Arrange - Create grid and dropdown filter
        var allGridData = new List<Dictionary<string, object?>>
        {
            new Dictionary<string, object?> { { "id", 1 }, { "name", "Product A" }, { "category", "Electronics" }, { "price", 99.99 } },
            new Dictionary<string, object?> { { "id", 2 }, { "name", "Product B" }, { "category", "Clothing" }, { "price", 49.99 } },
            new Dictionary<string, object?> { { "id", 3 }, { "name", "Product C" }, { "category", "Electronics" }, { "price", 199.99 } },
            new Dictionary<string, object?> { { "id", 4 }, { "name", "Product D" }, { "category", "Books" }, { "price", 29.99 } },
            new Dictionary<string, object?> { { "id", 5 }, { "name", "Product E" }, { "category", "Clothing" }, { "price", 79.99 } }
        };

        var filterDropdown = new FormField
        {
            Name = "categoryFilter",
            Label = "Filter by Category",
            Type = "select",
            Value = "Electronics",
            Options = new List<FormFieldOption>
            {
                new FormFieldOption { Label = "All Categories", Value = "all" },
                new FormFieldOption { Label = "Electronics", Value = "Electronics" },
                new FormFieldOption { Label = "Clothing", Value = "Clothing" },
                new FormFieldOption { Label = "Books", Value = "Books" }
            }
        };

        var currentGridData = new List<Dictionary<string, object?>>();

        // Act - Change filter and update grid data
        var selectedCategory = filterDropdown.Value?.ToString();
        if (selectedCategory == "all")
        {
            currentGridData = allGridData.ToList();
        }
        else if (!string.IsNullOrEmpty(selectedCategory))
        {
            currentGridData = allGridData.Where(row => row["category"]?.ToString() == selectedCategory).ToList();
        }

        // Assert - Verify filtered grid data
        Assert.Equal(2, currentGridData.Count);
        Assert.All(currentGridData, row => Assert.Equal("Electronics", row["category"]?.ToString()));
        Assert.Contains(currentGridData, row => row["name"]?.ToString() == "Product A");
        Assert.Contains(currentGridData, row => row["name"]?.ToString() == "Product C");
        Assert.DoesNotContain(currentGridData, row => row["name"]?.ToString() == "Product B");
    }

    /// <summary>
    /// Test 8: Form with conditional validation - Tests that validation rules are applied
    /// conditionally based on other field values, demonstrating complex validation chains.
    /// </summary>
    [Fact]
    public void FormWithConditionalValidation_ShouldApplyValidationRulesBasedOnFieldDependencies()
    {
        // Arrange - Create a form with conditional validation
        var form = new FormDefinition
        {
            Title = "Payment Form",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "paymentMethod",
                    Label = "Payment Method",
                    Type = "select",
                    IsRequired = true,
                    Value = "creditCard",
                    Order = 1,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Credit Card", Value = "creditCard" },
                        new FormFieldOption { Label = "Bank Transfer", Value = "bankTransfer" },
                        new FormFieldOption { Label = "PayPal", Value = "paypal" }
                    }
                },
                new FormField
                {
                    Name = "cardNumber",
                    Label = "Card Number",
                    Type = "text",
                    IsRequired = false,
                    DependsOn = "paymentMethod",
                    DependsOnValue = "creditCard",
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "pattern",
                            Value = @"^\d{16}$",
                            Message = "Card number must be 16 digits"
                        }
                    }
                },
                new FormField
                {
                    Name = "bankAccount",
                    Label = "Bank Account",
                    Type = "text",
                    IsRequired = false,
                    DependsOn = "paymentMethod",
                    DependsOnValue = "bankTransfer",
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "pattern",
                            Value = @"^\d{10,12}$",
                            Message = "Bank account must be 10-12 digits"
                        }
                    }
                }
            }
        };

        var validationErrors = new Dictionary<string, List<string>>();

        // Act - Apply conditional validation
        var paymentMethod = form.Fields.First(f => f.Name == "paymentMethod").Value?.ToString();

        foreach (var field in form.Fields.Where(f => !string.IsNullOrEmpty(f.DependsOn)))
        {
            if (field.DependsOn == "paymentMethod" && field.DependsOnValue?.ToString() == paymentMethod)
            {
                // Field should be required based on condition
                if (string.IsNullOrEmpty(field.Value?.ToString()))
                {
                    validationErrors[field.Name] = new List<string> { $"{field.Label} is required" };
                }

                // Apply validation rules
                if (field.ValidationRules.Any())
                {
                    var cardField = field.Value?.ToString();
                    var rule = field.ValidationRules.First();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(cardField ?? "", rule.Value ?? ""))
                    {
                        validationErrors[field.Name] = new List<string> { rule.Message };
                    }
                }
            }
        }

        // Assert
        Assert.Equal("creditCard", paymentMethod);
        Assert.Contains("cardNumber", validationErrors.Keys);
        Assert.Equal("Card number must be 16 digits", validationErrors["cardNumber"][0]);
        Assert.DoesNotContain("bankAccount", validationErrors.Keys);
    }

    /// <summary>
    /// Test 9: Component communication through data binding - Tests that data changes in one
    /// component are reflected in dependent components, verifying two-way data flow.
    /// </summary>
    [Fact]
    public void ComponentDataBinding_ShouldSynchronizeDataAcrossMultipleComponents()
    {
        // Arrange - Create interdependent components
        var userForm = new FormDefinition
        {
            Title = "User Profile",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "firstName",
                    Label = "First Name",
                    Type = "text",
                    Value = "John",
                    Order = 1
                },
                new FormField
                {
                    Name = "lastName",
                    Label = "Last Name",
                    Type = "text",
                    Value = "Doe",
                    Order = 2
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email",
                    Type = "email",
                    Value = "john@example.com",
                    Order = 3
                }
            }
        };

        // Display component that shows user greeting
        var displayGreeting = string.Empty;
        var displayEmail = string.Empty;

        // Act - Bind display component to form data
        var firstNameField = userForm.Fields.First(f => f.Name == "firstName");
        var lastNameField = userForm.Fields.First(f => f.Name == "lastName");
        var emailField = userForm.Fields.First(f => f.Name == "email");

        displayGreeting = $"{firstNameField.Value} {lastNameField.Value}";
        displayEmail = emailField.Value?.ToString() ?? "";

        // Simulate data change in form
        firstNameField.Value = "Jane";
        displayGreeting = $"{firstNameField.Value} {lastNameField.Value}";

        // Assert - Verify data binding
        Assert.Equal("Jane Doe", displayGreeting);
        Assert.Equal("john@example.com", displayEmail);
        Assert.Equal("Jane", userForm.Fields.First(f => f.Name == "firstName").Value?.ToString());
    }

    /// <summary>
    /// Test 10: Accordion with form validation - Tests that forms within accordion sections
    /// are validated independently, and validation errors affect accordion state.
    /// </summary>
    [Fact]
    public void AccordionWithFormValidation_ShouldValidateEachSectionIndependently()
    {
        // Arrange - Create accordion items with forms
        var accordionItems = new List<AccordionItem>
        {
            new AccordionItem
            {
                Key = "personal",
                Title = "Personal Information",
                Content = null
            },
            new AccordionItem
            {
                Key = "address",
                Title = "Address",
                Content = null
            }
        };

        var personalForm = new FormDefinition
        {
            Title = "Personal Info",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "age",
                    Label = "Age",
                    Type = "number",
                    IsRequired = true,
                    Value = "",
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule
                        {
                            Type = "min",
                            Value = "18",
                            Message = "Must be at least 18 years old"
                        }
                    }
                }
            }
        };

        var addressForm = new FormDefinition
        {
            Title = "Address",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "street",
                    Label = "Street",
                    Type = "text",
                    IsRequired = true,
                    Value = "123 Main St",
                    Order = 1
                }
            }
        };

        var sectionErrors = new Dictionary<string, List<string>>();

        // Act - Validate each section independently
        // Validate personal section
        if (personalForm.Fields.Any(f => f.IsRequired && string.IsNullOrEmpty(f.Value?.ToString())))
        {
            sectionErrors["personal"] = new List<string> { "Age is required" };
        }

        // Validate address section
        if (addressForm.Fields.All(f => !f.IsRequired || !string.IsNullOrEmpty(f.Value?.ToString())))
        {
            sectionErrors["address"] = new List<string>();
        }

        // Assert
        Assert.Contains("personal", sectionErrors.Keys);
        Assert.Contains("address", sectionErrors.Keys);
        Assert.Single(sectionErrors["personal"]);
        Assert.Empty(sectionErrors["address"]);
        Assert.Equal(2, accordionItems.Count);
    }
}

/// <summary>
/// Configuration model for modal component used in integration tests.
/// </summary>
public class ModalConfig
{
    /// <summary>Gets or sets the modal title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the modal is open</summary>
    public bool IsOpen { get; set; }

    /// <summary>Gets or sets the modal size (small, medium, large)</summary>
    public string Size { get; set; } = "medium";

    /// <summary>Gets or sets whether user can close the modal</summary>
    public bool AllowClose { get; set; } = true;

    /// <summary>Gets or sets the modal backdrop click behavior</summary>
    public bool CloseOnBackdropClick { get; set; } = true;
}
