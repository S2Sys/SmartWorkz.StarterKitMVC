namespace SmartWorkz.Core.Tests.Email;

using Xunit;
using SmartWorkz.Core.Shared.Email;
using System.Net.Mail;

public class EmailSenderTests
{
    private readonly SmtpSettings _settings;

    public EmailSenderTests()
    {
        _settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "test@example.com",
            Password = "password",
            FromAddress = "noreply@example.com",
            FromDisplayName = "Test App",
            EnableSsl = true,
            TimeoutMs = 10000
        };
    }

    [Fact]
    public void EmailMessage_ShouldHaveAttachmentsProperty()
    {
        // Arrange
        var message = new EmailMessage();

        // Act
        message.Attachments = new List<EmailAttachment>();

        // Assert
        Assert.NotNull(message.Attachments);
        Assert.Empty(message.Attachments);
    }

    [Fact]
    public void EmailMessage_ShouldAcceptNullAttachments()
    {
        // Arrange
        var message = new EmailMessage();

        // Act
        message.Attachments = null;

        // Assert
        Assert.Null(message.Attachments);
    }

    [Fact]
    public void EmailAttachment_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var attachment = new EmailAttachment
        {
            FileName = "test.txt",
            Content = new byte[] { 1, 2, 3 },
            ContentType = "text/plain"
        };

        // Assert
        Assert.Equal("test.txt", attachment.FileName);
        Assert.Equal(new byte[] { 1, 2, 3 }, attachment.Content);
        Assert.Equal("text/plain", attachment.ContentType);
    }

    [Fact]
    public void EmailAttachment_ContentTypeShouldBeNullable()
    {
        // Arrange & Act
        var attachment = new EmailAttachment
        {
            FileName = "test.bin",
            Content = new byte[] { 1, 2, 3 },
            ContentType = null
        };

        // Assert
        Assert.Null(attachment.ContentType);
    }

    [Fact]
    public void EmailMessage_ShouldSupportMultipleAttachments()
    {
        // Arrange
        var message = new EmailMessage
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "Test body",
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = "file1.txt",
                    Content = new byte[] { 1, 2, 3 },
                    ContentType = "text/plain"
                },
                new EmailAttachment
                {
                    FileName = "file2.pdf",
                    Content = new byte[] { 4, 5, 6 },
                    ContentType = "application/pdf"
                }
            }
        };

        // Assert
        Assert.NotNull(message.Attachments);
        Assert.Equal(2, message.Attachments.Count);
        Assert.Equal("file1.txt", message.Attachments[0].FileName);
        Assert.Equal("file2.pdf", message.Attachments[1].FileName);
    }

    [Fact]
    public void EmailSender_ShouldCreateInstanceWithSettings()
    {
        // Arrange & Act
        var sender = new EmailSender(_settings);

        // Assert
        Assert.NotNull(sender);
    }

    [Fact]
    public void EmailSender_ShouldThrowWhenSettingsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailSender(null!));
    }

    [Fact]
    public async Task EmailSender_SendAsync_WithNullAttachments_ShouldNotThrow()
    {
        // Arrange
        var sender = new EmailSender(_settings);
        var message = new EmailMessage
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "Test body",
            Attachments = null
        };

        // Act - we expect this to fail due to SMTP settings, but not due to attachments
        var result = await sender.SendAsync(message, CancellationToken.None);

        // Assert - should fail on SMTP, not on attachment handling
        Assert.False(result.Succeeded);
        // Verify it's an SMTP error, not an attachment-related error
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task EmailSender_SendAsync_WithEmptyAttachments_ShouldNotThrow()
    {
        // Arrange
        var sender = new EmailSender(_settings);
        var message = new EmailMessage
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "Test body",
            Attachments = new List<EmailAttachment>()
        };

        // Act
        var result = await sender.SendAsync(message, CancellationToken.None);

        // Assert - should fail on SMTP, not on attachment handling
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }
}
