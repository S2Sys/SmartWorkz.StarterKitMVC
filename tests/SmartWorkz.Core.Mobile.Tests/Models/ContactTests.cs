namespace SmartWorkz.Mobile.Tests.Models;

/// <summary>
/// Unit tests for Contact model covering display name generation and contact info validation.
///
/// Test Coverage:
/// 1. DisplayName_WithFirstAndLastName_CombinesNames
/// 2. HasContactInfo_WithEmailOrPhone_ReturnsTrue
/// 3. HasContactInfo_WithoutEmailOrPhone_ReturnsFalse
/// </summary>
public class ContactTests
{
    [Fact]
    public void DisplayName_WithFirstAndLastName_CombinesNames()
    {
        // Arrange
        var contact = new Contact("1", "John", "Doe", "john@example.com", "555-1234", null);

        // Act
        var displayName = contact.DisplayName;

        // Assert
        Assert.Equal("John Doe", displayName);
    }

    [Fact]
    public void DisplayName_WithoutLastName_ReturnsFirstNameOnly()
    {
        // Arrange
        var contact = new Contact("1", "John", null, "john@example.com", "555-1234", null);

        // Act
        var displayName = contact.DisplayName;

        // Assert
        Assert.Equal("John", displayName);
    }

    [Fact]
    public void HasContactInfo_WithEmail_ReturnsTrue()
    {
        // Arrange
        var contact = new Contact("1", "John", "Doe", "john@example.com", null, null);

        // Act & Assert
        Assert.True(contact.HasContactInfo);
    }

    [Fact]
    public void HasContactInfo_WithPhoneNumber_ReturnsTrue()
    {
        // Arrange
        var contact = new Contact("1", "John", "Doe", null, "555-1234", null);

        // Act & Assert
        Assert.True(contact.HasContactInfo);
    }

    [Fact]
    public void HasContactInfo_WithoutEmailOrPhone_ReturnsFalse()
    {
        // Arrange
        var contact = new Contact("1", "John", "Doe", null, null, null);

        // Act & Assert
        Assert.False(contact.HasContactInfo);
    }

    [Fact]
    public void Contact_WithAllProperties_StoresCorrectly()
    {
        // Arrange
        const string id = "123";
        const string firstName = "John";
        const string lastName = "Doe";
        const string email = "john@example.com";
        const string phone = "555-1234";
        const string address = "123 Main St";

        // Act
        var contact = new Contact(id, firstName, lastName, email, phone, address);

        // Assert
        Assert.Equal(id, contact.Id);
        Assert.Equal(firstName, contact.FirstName);
        Assert.Equal(lastName, contact.LastName);
        Assert.Equal(email, contact.Email);
        Assert.Equal(phone, contact.PhoneNumber);
        Assert.Equal(address, contact.Address);
    }
}
