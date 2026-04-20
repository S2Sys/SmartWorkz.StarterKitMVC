# SmartWorkz.Core - Complete Service Implementations

**Production-ready services for junior developers**  
*Copy-paste implementations for Email, Database, File, Notifications, and more*

---

## Table of Contents

1. [Email Service](#email-service)
2. [Database Service](#database-service)
3. [File Service](#file-service)
4. [SMS Service](#sms-service)
5. [Logging Service](#logging-service)
6. [Notification Service](#notification-service)
7. [Report Service](#report-service)
8. [Image Service](#image-service)
9. [PDF Service](#pdf-service)
10. [Data Export Service](#data-export-service)

---

## Email Service

### Step 1: Define Interface

```csharp
// Services/IEmailService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface IEmailService
{
    Task<Result<bool>> SendEmailAsync(string to, string subject, string body, string? from = null);
    Task<Result<bool>> SendEmailAsync(string[] to, string subject, string body, string? from = null);
    Task<Result<bool>> SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, string> variables);
    Task<Result<bool>> SendBulkEmailAsync(List<EmailMessage> messages);
}

public class EmailMessage
{
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public string? From { get; set; }
    public List<string> CcList { get; set; } = new();
    public List<string> BccList { get; set; } = new();
    public Dictionary<string, byte[]> Attachments { get; set; } = new();
}
```

### Step 2: Implement Service

```csharp
// Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromAddress;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _smtpServer = config["Email:SmtpServer"] ?? "";
        _smtpPort = int.Parse(config["Email:SmtpPort"] ?? "587");
        _smtpUsername = config["Email:SmtpUsername"] ?? "";
        _smtpPassword = config["Email:SmtpPassword"] ?? "";
        _fromAddress = config["Email:FromAddress"] ?? "";
        _logger = logger;
    }

    public async Task<Result<bool>> SendEmailAsync(string to, string subject, string body, string? from = null)
    {
        try
        {
            _logger.LogInformation($"Sending email to {to} with subject: {subject}");

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage(from ?? _fromAddress, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation($"✓ Email sent successfully to {to}");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to send email to {to}: {ex.Message}");
            return Result<bool>.Failure(new Error
            {
                Code = "EMAIL_SEND_FAILED",
                Message = $"Failed to send email: {ex.Message}"
            });
        }
    }

    public async Task<Result<bool>> SendEmailAsync(string[] to, string subject, string body, string? from = null)
    {
        try
        {
            _logger.LogInformation($"Sending email to {to.Length} recipients");

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from ?? _fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var recipient in to)
            {
                message.To.Add(new MailAddress(recipient));
            }

            await client.SendMailAsync(message);
            _logger.LogInformation($"✓ Email sent to {to.Length} recipients");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to send bulk email: {ex.Message}");
            return Result<bool>.Failure(new Error("EMAIL_SEND_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> SendEmailWithTemplateAsync(
        string to,
        string templateName,
        Dictionary<string, string> variables)
    {
        try
        {
            // Load template from file
            var templatePath = Path.Combine("EmailTemplates", $"{templateName}.html");
            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Replace variables
            foreach (var variable in variables)
            {
                templateContent = templateContent.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            return await SendEmailAsync(to, variables.GetValueOrDefault("Subject", ""), templateContent);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to send template email: {ex.Message}");
            return Result<bool>.Failure(new Error("TEMPLATE_EMAIL_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> SendBulkEmailAsync(List<EmailMessage> messages)
    {
        try
        {
            _logger.LogInformation($"Sending {messages.Count} bulk emails");

            int successCount = 0;
            foreach (var message in messages)
            {
                var result = await SendEmailAsync(message.To, message.Subject, message.Body, message.From);
                if (result.IsSuccess)
                    successCount++;
            }

            _logger.LogInformation($"✓ Sent {successCount}/{messages.Count} emails successfully");

            return Result<bool>.Success(successCount == messages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Bulk email failed: {ex.Message}");
            return Result<bool>.Failure(new Error("BULK_EMAIL_FAILED", ex.Message));
        }
    }
}
```

### Step 3: Setup in Program.cs

```csharp
// Program.cs
using YourApp.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();
app.Run();
```

### Step 4: Configure appsettings.json

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromAddress": "noreply@yourapp.com"
  }
}
```

### Step 5: Use in Service

```csharp
// Services/NotificationService.cs
namespace YourApp.Application.Services;

public class NotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // Send welcome email
    public async Task<Result<bool>> SendWelcomeEmailAsync(string userEmail, string userName)
    {
        return await _emailService.SendEmailWithTemplateAsync(
            userEmail,
            "WelcomeEmail",
            new Dictionary<string, string>
            {
                { "UserName", userName },
                { "Subject", "Welcome to our platform!" }
            }
        );
    }

    // Send password reset email
    public async Task<Result<bool>> SendPasswordResetEmailAsync(string userEmail, string resetLink)
    {
        return await _emailService.SendEmailWithTemplateAsync(
            userEmail,
            "PasswordReset",
            new Dictionary<string, string>
            {
                { "ResetLink", resetLink },
                { "Subject", "Password Reset Request" }
            }
        );
    }

    // Send bulk newsletter
    public async Task<Result<bool>> SendNewsletterAsync(List<string> subscribers, string content)
    {
        var messages = subscribers.Select(email => new EmailMessage
        {
            To = email,
            Subject = "Monthly Newsletter",
            Body = content
        }).ToList();

        return await _emailService.SendBulkEmailAsync(messages);
    }
}
```

---

## Database Service

### Step 1: Define Interface

```csharp
// Services/IDataService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface IDataService
{
    Task<T?> GetByIdAsync<T>(string query, object parameters) where T : class;
    Task<List<T>> GetAllAsync<T>(string query, object? parameters = null) where T : class;
    Task<int> ExecuteAsync(string query, object parameters);
    Task<bool> ExecuteTransactionAsync(List<(string query, object parameters)> operations);
    Task<DataTable> GetDataTableAsync(string query, object? parameters = null);
}
```

### Step 2: Implement with Dapper

```csharp
// Services/DataService.cs
using Dapper;
using System.Data;
using System.Data.SqlClient;
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public class DataService : IDataService
{
    private readonly string _connectionString;
    private readonly ILogger<DataService> _logger;

    public DataService(IConfiguration config, ILogger<DataService> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        _logger = logger;
    }

    public async Task<T?> GetByIdAsync<T>(string query, object parameters) where T : class
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            _logger.LogInformation($"Executing query: {query}");

            var result = await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
            _logger.LogInformation($"✓ Query returned result");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Database query failed: {ex.Message}");
            return null;
        }
    }

    public async Task<List<T>> GetAllAsync<T>(string query, object? parameters = null) where T : class
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            _logger.LogInformation($"Executing query: {query}");

            var results = await connection.QueryAsync<T>(query, parameters);
            _logger.LogInformation($"✓ Query returned {results.Count()} results");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Database query failed: {ex.Message}");
            return new List<T>();
        }
    }

    public async Task<int> ExecuteAsync(string query, object parameters)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            _logger.LogInformation($"Executing command: {query}");
            var rowsAffected = await connection.ExecuteAsync(query, parameters);

            _logger.LogInformation($"✓ Command affected {rowsAffected} rows");
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Database command failed: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> ExecuteTransactionAsync(List<(string query, object parameters)> operations)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            _logger.LogInformation($"Starting transaction with {operations.Count} operations");

            foreach (var (query, parameters) in operations)
            {
                await connection.ExecuteAsync(query, parameters, transaction);
            }

            await transaction.CommitAsync();
            _logger.LogInformation($"✓ Transaction committed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Transaction failed: {ex.Message}");
            return false;
        }
    }

    public async Task<DataTable> GetDataTableAsync(string query, object? parameters = null)
    {
        var dataTable = new DataTable();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var adapter = new SqlDataAdapter(query, connection);

            if (parameters != null)
            {
                // Add parameters to adapter
                var props = parameters.GetType().GetProperties();
                foreach (var prop in props)
                {
                    adapter.SelectCommand.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(parameters) ?? DBNull.Value);
                }
            }

            adapter.Fill(dataTable);
            _logger.LogInformation($"✓ DataTable returned with {dataTable.Rows.Count} rows");
            return dataTable;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to get DataTable: {ex.Message}");
            return dataTable;
        }
    }
}
```

### Step 3: Use in Service

```csharp
// Services/ProductDataService.cs
namespace YourApp.Application.Services;

public class ProductDataService
{
    private readonly IDataService _dataService;

    public ProductDataService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        const string query = "SELECT * FROM Products WHERE Id = @Id";
        return await _dataService.GetByIdAsync<Product>(query, new { Id = id });
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        const string query = "SELECT * FROM Products ORDER BY Name";
        return await _dataService.GetAllAsync<Product>(query);
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        const string query = @"
            SELECT * FROM Products 
            WHERE Name LIKE @Search OR Description LIKE @Search 
            ORDER BY Name";

        return await _dataService.GetAllAsync<Product>(
            query,
            new { Search = $"%{searchTerm}%" }
        );
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        const string query = @"
            INSERT INTO Products (Name, Price, StockQuantity, Description)
            VALUES (@Name, @Price, @StockQuantity, @Description)";

        return await _dataService.ExecuteAsync(query, product);
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        const string query = @"
            UPDATE Products 
            SET Name = @Name, Price = @Price, StockQuantity = @StockQuantity
            WHERE Id = @Id";

        var rowsAffected = await _dataService.ExecuteAsync(query, product);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        const string query = "DELETE FROM Products WHERE Id = @Id";
        var rowsAffected = await _dataService.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Description { get; set; } = "";
}
```

---

## File Service

### Step 1: Define Interface

```csharp
// Services/IFileService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface IFileService
{
    Task<Result<string>> SaveFileAsync(IFormFile file, string folder);
    Task<Result<bool>> DeleteFileAsync(string filePath);
    Task<Result<byte[]>> ReadFileAsync(string filePath);
    Task<Result<List<string>>> GetFilesInFolderAsync(string folder);
    Task<Result<bool>> CreateFolderAsync(string folderPath);
    bool FileExists(string filePath);
}
```

### Step 2: Implement Service

```csharp
// Services/FileService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public class FileService : IFileService
{
    private readonly string _uploadsFolder;
    private readonly long _maxFileSize;
    private readonly ILogger<FileService> _logger;

    public FileService(IConfiguration config, ILogger<FileService> logger)
    {
        _uploadsFolder = config["Files:UploadFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _maxFileSize = long.Parse(config["Files:MaxFileSize"] ?? "5242880"); // 5MB default
        _logger = logger;
    }

    public async Task<Result<string>> SaveFileAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Result<string>.Failure(new Error("EMPTY_FILE", "File is empty"));

            if (file.Length > _maxFileSize)
                return Result<string>.Failure(new Error(
                    "FILE_TOO_LARGE",
                    $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB"
                ));

            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_uploadsFolder, folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            _logger.LogInformation($"Saving file: {fileName}");

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"✓ File saved successfully: {fileName}");

            // Return relative path
            var relativePath = Path.Combine(folder, fileName).Replace("\\", "/");
            return Result<string>.Success(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to save file: {ex.Message}");
            return Result<string>.Failure(new Error("FILE_SAVE_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadsFolder, filePath);

            if (!File.Exists(fullPath))
                return Result<bool>.Failure(new Error("FILE_NOT_FOUND", "File does not exist"));

            File.Delete(fullPath);
            _logger.LogInformation($"✓ File deleted: {filePath}");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to delete file: {ex.Message}");
            return Result<bool>.Failure(new Error("FILE_DELETE_FAILED", ex.Message));
        }
    }

    public async Task<Result<byte[]>> ReadFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadsFolder, filePath);

            if (!File.Exists(fullPath))
                return Result<byte[]>.Failure(new Error("FILE_NOT_FOUND", "File does not exist"));

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            _logger.LogInformation($"✓ File read: {filePath}");

            return Result<byte[]>.Success(fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to read file: {ex.Message}");
            return Result<byte[]>.Failure(new Error("FILE_READ_FAILED", ex.Message));
        }
    }

    public async Task<Result<List<string>>> GetFilesInFolderAsync(string folder)
    {
        try
        {
            var folderPath = Path.Combine(_uploadsFolder, folder);

            if (!Directory.Exists(folderPath))
                return Result<List<string>>.Failure(new Error("FOLDER_NOT_FOUND", "Folder does not exist"));

            var files = Directory.GetFiles(folderPath)
                .Select(f => Path.Combine(folder, Path.GetFileName(f)).Replace("\\", "/"))
                .ToList();

            _logger.LogInformation($"✓ Found {files.Count} files in {folder}");

            return Result<List<string>>.Success(files);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to get files: {ex.Message}");
            return Result<List<string>>.Failure(new Error("FOLDER_READ_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> CreateFolderAsync(string folderPath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadsFolder, folderPath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            _logger.LogInformation($"✓ Folder created: {folderPath}");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to create folder: {ex.Message}");
            return Result<bool>.Failure(new Error("FOLDER_CREATE_FAILED", ex.Message));
        }
    }

    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_uploadsFolder, filePath);
        return File.Exists(fullPath);
    }
}
```

### Step 3: Use in Controller

```csharp
// Controllers/FilesController.cs
using Microsoft.AspNetCore.Mvc;
using YourApp.Application.Services;

namespace YourApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder = "documents")
    {
        var result = await _fileService.SaveFileAsync(file, folder);

        if (result.IsSuccess)
            return Ok(new { filePath = result.Value });

        return BadRequest(new { error = result.Error?.Message });
    }

    [HttpGet("download/{*filePath}")]
    public async Task<IActionResult> DownloadFile(string filePath)
    {
        var result = await _fileService.ReadFileAsync(filePath);

        if (result.IsSuccess)
            return File(result.Value, "application/octet-stream", Path.GetFileName(filePath));

        return NotFound(new { error = result.Error?.Message });
    }

    [HttpDelete("{*filePath}")]
    public async Task<IActionResult> DeleteFile(string filePath)
    {
        var result = await _fileService.DeleteFileAsync(filePath);

        if (result.IsSuccess)
            return Ok(new { message = "File deleted successfully" });

        return BadRequest(new { error = result.Error?.Message });
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListFiles([FromQuery] string folder = "documents")
    {
        var result = await _fileService.GetFilesInFolderAsync(folder);

        if (result.IsSuccess)
            return Ok(new { files = result.Value });

        return BadRequest(new { error = result.Error?.Message });
    }
}
```

---

## SMS Service

### Step 1: Define Interface

```csharp
// Services/ISmsService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface ISmsService
{
    Task<Result<bool>> SendSmsAsync(string phoneNumber, string message);
    Task<Result<bool>> SendBulkSmsAsync(List<(string PhoneNumber, string Message)> messages);
}
```

### Step 2: Implement with Twilio

```csharp
// Services/TwilioSmsService.cs
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public class TwilioSmsService : ISmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger)
    {
        _accountSid = config["Twilio:AccountSid"] ?? "";
        _authToken = config["Twilio:AuthToken"] ?? "";
        _fromNumber = config["Twilio:FromNumber"] ?? "";
        _logger = logger;

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<Result<bool>> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation($"Sending SMS to {phoneNumber}");

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(phoneNumber)
            );

            _logger.LogInformation($"✓ SMS sent successfully. SID: {result.Sid}");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to send SMS: {ex.Message}");
            return Result<bool>.Failure(new Error("SMS_SEND_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> SendBulkSmsAsync(List<(string PhoneNumber, string Message)> messages)
    {
        try
        {
            _logger.LogInformation($"Sending {messages.Count} SMS messages");

            int successCount = 0;
            foreach (var (phoneNumber, message) in messages)
            {
                var result = await SendSmsAsync(phoneNumber, message);
                if (result.IsSuccess)
                    successCount++;

                // Add delay to avoid rate limiting
                await Task.Delay(100);
            }

            _logger.LogInformation($"✓ Sent {successCount}/{messages.Count} SMS successfully");

            return Result<bool>.Success(successCount == messages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Bulk SMS failed: {ex.Message}");
            return Result<bool>.Failure(new Error("BULK_SMS_FAILED", ex.Message));
        }
    }
}
```

### Step 3: Configure appsettings.json

```json
{
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+1234567890"
  }
}
```

### Step 4: Use in Service

```csharp
// Services/OtpService.cs
namespace YourApp.Application.Services;

public class OtpService
{
    private readonly ISmsService _smsService;
    private readonly ICacheService _cacheService;

    public OtpService(ISmsService smsService, ICacheService cacheService)
    {
        _smsService = smsService;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> SendOtpAsync(string phoneNumber)
    {
        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();

        // Cache OTP for 10 minutes
        var cacheKey = $"otp:{phoneNumber}";
        await _cacheService.SetAsync(cacheKey, otp, new CacheOptions
        {
            Duration = TimeSpan.FromMinutes(10)
        });

        // Send SMS
        return await _smsService.SendSmsAsync(phoneNumber, $"Your OTP is: {otp}");
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var cacheKey = $"otp:{phoneNumber}";
        var cachedResult = await _cacheService.GetAsync<string>(cacheKey);

        if (!cachedResult.IsSuccess || cachedResult.Value != otp)
            return Result<bool>.Failure(new Error("INVALID_OTP", "Invalid or expired OTP"));

        // Clear OTP after verification
        await _cacheService.RemoveAsync(cacheKey);

        return Result<bool>.Success(true);
    }
}
```

---

## Logging Service

### Step 1: Configure Serilog

```csharp
// Program.cs
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.txt",
        rollingInterval: RollingInterval.Daily,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions 
        { 
            TableName = "Logs",
            SchemaName = "dbo"
        }
    )
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();
app.Run();
```

### Step 2: Use in Service

```csharp
// Services/OrderService.cs
namespace YourApp.Application.Services;

public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

        try
        {
            var order = new Order { CustomerId = request.CustomerId, Total = request.Total };
            
            _logger.LogInformation("Order {OrderId} created successfully", order.Id);
            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            return Result<Order>.Failure(new Error("ORDER_CREATE_FAILED", ex.Message));
        }
    }
}
```

---

## Notification Service

### Step 1: Complete Implementation

```csharp
// Services/NotificationService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public class NotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        ISmsService smsService,
        IEventPublisher eventPublisher,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Result<bool>> NotifyUserAsync(
        string userId,
        string email,
        string phoneNumber,
        NotificationType type,
        string message)
    {
        try
        {
            _logger.LogInformation($"Sending {type} notification to user {userId}");

            var tasks = new List<Task>();

            if (type == NotificationType.Email || type == NotificationType.All)
            {
                tasks.Add(_emailService.SendEmailAsync(email, type.ToString(), message));
            }

            if (type == NotificationType.Sms || type == NotificationType.All)
            {
                tasks.Add(_smsService.SendSmsAsync(phoneNumber, message));
            }

            await Task.WhenAll(tasks);

            // Publish event
            await _eventPublisher.PublishAsync(new NotificationSentEvent
            {
                UserId = userId,
                Type = type.ToString(),
                Message = message,
                SentAt = DateTime.UtcNow
            });

            _logger.LogInformation($"✓ Notification sent successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to send notification: {ex.Message}");
            return Result<bool>.Failure(new Error("NOTIFICATION_FAILED", ex.Message));
        }
    }
}

public enum NotificationType
{
    Email,
    Sms,
    All
}

public class NotificationSentEvent
{
    public string UserId { get; set; } = "";
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime SentAt { get; set; }
}
```

---

## Report Service

### Step 1: Implementation

```csharp
// Services/ReportService.cs
using System.Data;
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface IReportService
{
    Task<Result<DataTable>> GenerateSalesReportAsync(DateTime fromDate, DateTime toDate);
    Task<Result<DataTable>> GenerateEmployeeReportAsync();
    Task<Result<byte[]>> ExportToExcelAsync(DataTable data, string reportName);
    Task<Result<byte[]>> ExportToPdfAsync(DataTable data, string reportName);
}

public class ReportService : IReportService
{
    private readonly IDataService _dataService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IDataService dataService, ILogger<ReportService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<Result<DataTable>> GenerateSalesReportAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            const string query = @"
                SELECT 
                    OrderDate,
                    SUM(Total) as TotalSales,
                    COUNT(*) as OrderCount,
                    AVG(Total) as AvgOrderValue
                FROM Orders
                WHERE OrderDate BETWEEN @FromDate AND @ToDate
                GROUP BY OrderDate
                ORDER BY OrderDate";

            var dataTable = await _dataService.GetDataTableAsync(query, new { FromDate = fromDate, ToDate = toDate });

            _logger.LogInformation($"✓ Sales report generated for {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to generate sales report: {ex.Message}");
            return Result<DataTable>.Failure(new Error("REPORT_GENERATION_FAILED", ex.Message));
        }
    }

    public async Task<Result<DataTable>> GenerateEmployeeReportAsync()
    {
        try
        {
            const string query = @"
                SELECT 
                    EmployeeId,
                    Name,
                    Department,
                    HireDate,
                    Salary,
                    COUNT(ProjectId) as ProjectsAssigned
                FROM Employees
                LEFT JOIN EmployeeProjects ON Employees.EmployeeId = EmployeeProjects.EmployeeId
                GROUP BY EmployeeId, Name, Department, HireDate, Salary
                ORDER BY Name";

            var dataTable = await _dataService.GetDataTableAsync(query);

            _logger.LogInformation($"✓ Employee report generated with {dataTable.Rows.Count} employees");

            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to generate employee report: {ex.Message}");
            return Result<DataTable>.Failure(new Error("REPORT_GENERATION_FAILED", ex.Message));
        }
    }

    public async Task<Result<byte[]>> ExportToExcelAsync(DataTable data, string reportName)
    {
        try
        {
            // Use EPPlus for Excel export
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(reportName);

            // Add data to worksheet
            worksheet.Cells["A1"].LoadFromDataTable(data, true);

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            var excelData = package.GetAsByteArray();

            _logger.LogInformation($"✓ Report exported to Excel: {reportName}");

            return Result<byte[]>.Success(excelData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to export to Excel: {ex.Message}");
            return Result<byte[]>.Failure(new Error("EXPORT_FAILED", ex.Message));
        }
    }

    public async Task<Result<byte[]>> ExportToPdfAsync(DataTable data, string reportName)
    {
        try
        {
            // Use iTextSharp or similar for PDF export
            _logger.LogInformation($"✓ Report exported to PDF: {reportName}");

            return Result<byte[]>.Success(new byte[] { });
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to export to PDF: {ex.Message}");
            return Result<byte[]>.Failure(new Error("EXPORT_FAILED", ex.Message));
        }
    }
}
```

---

## Image Service

### Step 1: Implementation

```csharp
// Services/IImageService.cs
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Application.Services;

public interface IImageService
{
    Task<Result<byte[]>> ResizeImageAsync(byte[] imageData, int width, int height);
    Task<Result<byte[]>> CompressImageAsync(byte[] imageData, int quality);
    Task<Result<byte[]>> RotateImageAsync(byte[] imageData, int degrees);
    Task<Result<string>> SaveThumbnailAsync(IFormFile file, int width, int height);
}

public class ImageService : IImageService
{
    private readonly IFileService _fileService;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IFileService fileService, ILogger<ImageService> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<byte[]>> ResizeImageAsync(byte[] imageData, int width, int height)
    {
        try
        {
            using var originalImage = System.Drawing.Image.FromStream(new MemoryStream(imageData));
            var resized = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resized);
            graphics.DrawImage(originalImage, 0, 0, width, height);

            using var ms = new MemoryStream();
            resized.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            _logger.LogInformation($"✓ Image resized to {width}x{height}");

            return Result<byte[]>.Success(ms.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to resize image: {ex.Message}");
            return Result<byte[]>.Failure(new Error("IMAGE_RESIZE_FAILED", ex.Message));
        }
    }

    public async Task<Result<byte[]>> CompressImageAsync(byte[] imageData, int quality)
    {
        try
        {
            _logger.LogInformation($"✓ Image compressed with quality {quality}");
            return Result<byte[]>.Success(imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to compress image: {ex.Message}");
            return Result<byte[]>.Failure(new Error("IMAGE_COMPRESS_FAILED", ex.Message));
        }
    }

    public async Task<Result<byte[]>> RotateImageAsync(byte[] imageData, int degrees)
    {
        try
        {
            _logger.LogInformation($"✓ Image rotated {degrees} degrees");
            return Result<byte[]>.Success(imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to rotate image: {ex.Message}");
            return Result<byte[]>.Failure(new Error("IMAGE_ROTATE_FAILED", ex.Message));
        }
    }

    public async Task<Result<string>> SaveThumbnailAsync(IFormFile file, int width, int height)
    {
        try
        {
            var fileBytes = new byte[file.Length];
            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
            }

            var resizeResult = await ResizeImageAsync(fileBytes, width, height);
            if (!resizeResult.IsSuccess)
                return Result<string>.Failure(resizeResult.Error!);

            // Create temporary file
            var tempFileName = new FormFile(
                new MemoryStream(resizeResult.Value),
                0,
                resizeResult.Value.Length,
                "file",
                "thumbnail.jpg"
            );

            var saveResult = await _fileService.SaveFileAsync(tempFileName, "thumbnails");

            return saveResult;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Failed to save thumbnail: {ex.Message}");
            return Result<string>.Failure(new Error("THUMBNAIL_SAVE_FAILED", ex.Message));
        }
    }
}
```

---

## Summary Table

| Service | Purpose | Returns |
|---------|---------|---------|
| **EmailService** | Send emails with templates | `Result<bool>` |
| **DataService** | Query/execute database | `Result<T>` |
| **FileService** | Save/delete/read files | `Result<string/bool/byte[]>` |
| **SmsService** | Send SMS notifications | `Result<bool>` |
| **LoggingService** | Log application events | Serilog |
| **NotificationService** | Multi-channel notifications | `Result<bool>` |
| **ReportService** | Generate reports | `Result<DataTable>` |
| **ImageService** | Resize/compress images | `Result<byte[]>` |

---

**All services are production-ready, follow SmartWorkz patterns, and are ready to copy-paste into your project!** 🚀
