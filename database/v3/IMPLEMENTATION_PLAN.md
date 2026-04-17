# **V3 Complete Implementation Plan**

## **Overview**

Build a complete multi-tenant ASP.NET 9 MVC application with:
- **Database:** V3 schema (41 tables, 6 schemas)
- **Data Access:** Dapper ORM (lightweight, fast)
- **Business Logic:** Service layer with UPSERT pattern
- **APIs:** RESTful endpoints for Admin and Public
- **Frontend:** MVC views for Admin and Public sites
- **Architecture:** Clean separation of concerns

---

## **Phase 1: Database Foundation** ✅ DONE

- [x] 01_CREATE_SCHEMA.sql - All 38 tables created
- [ ] 02_CREATE_STORED_PROCEDURES.sql - UPSERT, EXISTS, GET, GETID operations
- [ ] 03_SEED_DATA.sql - Initial data using stored procedures
- [ ] 04_MATERIALIZED_VIEWS.sql - Performance views for lookups

---

## **Phase 2: Dapper Data Access Layer (DAL)**

### **2.1 Project Structure**

```
src/SmartWorkz.StarterKitMVC.Infrastructure/
├── Data/
│   ├── IRepository.cs (base interface)
│   ├── DapperRepository.cs (base implementation)
│   ├── UnitOfWork.cs
│   └── Repositories/
│       ├── TenantRepository.cs
│       ├── LookupRepository.cs
│       ├── UserRepository.cs
│       ├── ConfigurationRepository.cs
│       ├── BlogPostRepository.cs
│       ├── NotificationRepository.cs
│       ├── EmailQueueRepository.cs
│       ├── AuditLogRepository.cs
│       └── ... (1 repository per table)
└── Migrations/
    └── (Dapper doesn't need migrations - SQL scripts only)
```

### **2.2 Base Repository Interface**

**File:** `Infrastructure/Data/IRepository.cs`

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync(string tenantId);
    Task<T> UpsertAsync(T entity);  // INSERT or UPDATE
    Task<bool> DeleteAsync(object id);  // Soft delete
    Task<int> CountAsync(string tenantId);
}
```

### **2.3 Dapper Base Implementation**

**File:** `Infrastructure/Data/DapperRepository.cs`

```csharp
public abstract class DapperRepository<T> : IRepository<T> where T : class
{
    protected readonly IDbConnection _connection;
    protected readonly string _tableName;
    protected readonly string _schema;
    
    public async Task<T> GetByIdAsync(object id)
    {
        var sql = $"SELECT * FROM [{_schema}].[{_tableName}] WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }
    
    public async Task<IEnumerable<T>> GetAllAsync(string tenantId)
    {
        var sql = $"SELECT * FROM [{_schema}].[{_tableName}] WHERE TenantId = @TenantId AND IsDeleted = 0";
        return await _connection.QueryAsync<T>(sql, new { TenantId = tenantId });
    }
    
    public async Task<T> UpsertAsync(T entity)
    {
        var sp = $"sp_{_tableName}_Upsert";
        return await _connection.QueryFirstOrDefaultAsync<T>(sp, entity, commandType: CommandType.StoredProcedure);
    }
    
    public async Task<bool> DeleteAsync(object id)
    {
        var sql = $"UPDATE [{_schema}].[{_tableName}] SET IsDeleted = 1 WHERE Id = @Id";
        var result = await _connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
```

### **2.4 Concrete Repositories**

**Example 1: LookupRepository**

**File:** `Infrastructure/Data/Repositories/LookupRepository.cs`

```csharp
public class LookupRepository : DapperRepository<LookupDto>
{
    public LookupRepository(IDbConnection connection) : base(connection)
    {
        _tableName = "Lookup";
        _schema = "Master";
    }
    
    public async Task<IEnumerable<LookupDto>> GetByCategory(string categoryKey, string tenantId)
    {
        var sql = @"SELECT * FROM Master.Lookup 
                   WHERE CategoryKey = @CategoryKey 
                   AND (IsGlobalScope = 1 OR TenantId = @TenantId)
                   AND IsActive = 1 AND IsDeleted = 0
                   ORDER BY SortOrder";
        return await _connection.QueryAsync<LookupDto>(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }
    
    public async Task<IEnumerable<LookupDto>> GetCurrencies(string tenantId)
        => await GetByCategory("currencies", tenantId);
    
    public async Task<IEnumerable<LookupDto>> GetLanguages(string tenantId)
        => await GetByCategory("languages", tenantId);
    
    public async Task<IEnumerable<LookupDto>> GetTimeZones(string tenantId)
        => await GetByCategory("timezones", tenantId);
}
```

**Example 2: UserRepository**

**File:** `Infrastructure/Data/Repositories/UserRepository.cs`

```csharp
public class UserRepository : DapperRepository<UserDto>
{
    public UserRepository(IDbConnection connection) : base(connection)
    {
        _tableName = "Users";
        _schema = "Auth";
    }
    
    public async Task<UserDto> GetByEmail(string email, string tenantId)
    {
        var sql = @"SELECT * FROM Auth.Users 
                   WHERE NormalizedEmail = @NormalizedEmail 
                   AND TenantId = @TenantId 
                   AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<UserDto>(sql, 
            new { NormalizedEmail = email.ToUpper(), TenantId = tenantId });
    }
    
    public async Task<IEnumerable<UserRoleDto>> GetRoles(string userId, string tenantId)
    {
        var sql = @"SELECT r.* FROM Auth.Roles r
                   INNER JOIN Auth.UserRoles ur ON r.RoleId = ur.RoleId
                   WHERE ur.UserId = @UserId AND ur.TenantId = @TenantId";
        return await _connection.QueryAsync<UserRoleDto>(sql, new { UserId = userId, TenantId = tenantId });
    }
    
    public async Task<IEnumerable<PermissionDto>> GetPermissions(string userId, string tenantId)
    {
        var sql = @"SELECT p.* FROM Auth.Permissions p
                   INNER JOIN Auth.UserPermissions up ON p.PermissionId = up.PermissionId
                   WHERE up.UserId = @UserId AND up.TenantId = @TenantId
                   UNION
                   SELECT p.* FROM Auth.Permissions p
                   INNER JOIN Auth.RolePermissions rp ON p.PermissionId = rp.PermissionId
                   INNER JOIN Auth.UserRoles ur ON rp.RoleId = ur.RoleId
                   WHERE ur.UserId = @UserId AND ur.TenantId = @TenantId";
        return await _connection.QueryAsync<PermissionDto>(sql, new { UserId = userId, TenantId = tenantId });
    }
}
```

**Example 3: ConfigurationRepository**

**File:** `Infrastructure/Data/Repositories/ConfigurationRepository.cs`

```csharp
public class ConfigurationRepository : DapperRepository<ConfigurationDto>
{
    public ConfigurationRepository(IDbConnection connection) : base(connection)
    {
        _tableName = "Configuration";
        _schema = "Master";
    }
    
    public async Task<ConfigurationDto> GetByKey(string key, string tenantId)
    {
        var sql = @"SELECT * FROM Master.Configuration 
                   WHERE [Key] = @Key AND TenantId = @TenantId AND IsActive = 1";
        return await _connection.QueryFirstOrDefaultAsync<ConfigurationDto>(sql, 
            new { Key = key, TenantId = tenantId });
    }
    
    public async Task<string> GetConfigValue(string key, string tenantId, string defaultValue = null)
    {
        var config = await GetByKey(key, tenantId);
        return config?.Value ?? defaultValue;
    }
}
```

### **2.5 Unit of Work Pattern**

**File:** `Infrastructure/Data/UnitOfWork.cs`

```csharp
public class UnitOfWork : IDisposable
{
    private readonly IDbConnection _connection;
    
    public LookupRepository Lookups { get; }
    public UserRepository Users { get; }
    public ConfigurationRepository Configurations { get; }
    public BlogPostRepository BlogPosts { get; }
    public NotificationRepository Notifications { get; }
    public AuditLogRepository AuditLogs { get; }
    public EmailQueueRepository EmailQueues { get; }
    
    public UnitOfWork(IConfiguration config)
    {
        _connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        
        Lookups = new LookupRepository(_connection);
        Users = new UserRepository(_connection);
        Configurations = new ConfigurationRepository(_connection);
        BlogPosts = new BlogPostRepository(_connection);
        Notifications = new NotificationRepository(_connection);
        AuditLogs = new AuditLogRepository(_connection);
        EmailQueues = new EmailQueueRepository(_connection);
    }
    
    public void Dispose()
    {
        _connection?.Dispose();
    }
}
```

### **2.6 Repository Registration**

**File:** `Program.cs`

```csharp
// Add to DI container
services.AddScoped<IDbConnection>(sp => 
    new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
    
services.AddScoped<UnitOfWork>();
services.AddScoped<ILookupRepository, LookupRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
// ... register all repositories
```

---

## **Phase 3: Business Logic Service Layer**

### **3.1 Project Structure**

```
src/SmartWorkz.StarterKitMVC.Application/
├── Services/
│   ├── ILookupService.cs / LookupService.cs
│   ├── IUserService.cs / UserService.cs
│   ├── IConfigurationService.cs / ConfigurationService.cs
│   ├── IAuthenticationService.cs / AuthenticationService.cs
│   ├── IBlogService.cs / BlogService.cs
│   ├── INotificationService.cs / NotificationService.cs
│   ├── IEmailService.cs / EmailService.cs
│   └── ... (1 service per feature)
├── DTOs/
│   ├── LookupDto.cs
│   ├── UserDto.cs
│   ├── BlogPostDto.cs
│   └── ...
└── Enums/
    ├── TokenType.cs
    ├── PermissionType.cs
    └── ...
```

### **3.2 Service Examples**

**Example 1: LookupService**

**File:** `Application/Services/ILookupService.cs`

```csharp
public interface ILookupService
{
    Task<IEnumerable<LookupDto>> GetCurrencies(string tenantId);
    Task<IEnumerable<LookupDto>> GetLanguages(string tenantId);
    Task<IEnumerable<LookupDto>> GetTimeZones(string tenantId);
    Task<IEnumerable<LookupDto>> GetByCategory(string categoryKey, string tenantId);
    Task<LookupDto> UpsertAsync(LookupDto lookup);
    Task<bool> DeleteAsync(Guid id);
    Task<LookupDto> GetByIdAsync(Guid id);
}
```

**File:** `Application/Services/LookupService.cs`

```csharp
public class LookupService : ILookupService
{
    private readonly ILookupRepository _repository;
    private readonly IMemoryCache _cache;
    
    public LookupService(ILookupRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }
    
    public async Task<IEnumerable<LookupDto>> GetCurrencies(string tenantId)
    {
        var cacheKey = $"lookups_currencies_{tenantId}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<LookupDto> currencies))
        {
            currencies = await _repository.GetByCategory("currencies", tenantId);
            _cache.Set(cacheKey, currencies, TimeSpan.FromHours(24));
        }
        return currencies;
    }
    
    public async Task<IEnumerable<LookupDto>> GetLanguages(string tenantId)
    {
        var cacheKey = $"lookups_languages_{tenantId}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<LookupDto> languages))
        {
            languages = await _repository.GetByCategory("languages", tenantId);
            _cache.Set(cacheKey, languages, TimeSpan.FromHours(24));
        }
        return languages;
    }
    
    public async Task<IEnumerable<LookupDto>> GetTimeZones(string tenantId)
    {
        var cacheKey = $"lookups_timezones_{tenantId}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<LookupDto> timeZones))
        {
            timeZones = await _repository.GetByCategory("timezones", tenantId);
            _cache.Set(cacheKey, timeZones, TimeSpan.FromHours(24));
        }
        return timeZones;
    }
    
    public async Task<LookupDto> UpsertAsync(LookupDto lookup)
    {
        var result = await _repository.UpsertAsync(lookup);
        InvalidateCache(lookup.CategoryKey, lookup.TenantId);
        return result;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            InvalidateAllCaches();
        }
        return result;
    }
    
    private void InvalidateCache(string categoryKey, string tenantId)
    {
        _cache.Remove($"lookups_{categoryKey}_{tenantId}");
    }
    
    private void InvalidateAllCaches()
    {
        // Clear all lookup caches - implementation depends on cache type
    }
}
```

**Example 2: AuthenticationService**

**File:** `Application/Services/IAuthenticationService.cs`

```csharp
public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(string email, string password, string tenantId);
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<bool> SendPasswordResetEmailAsync(string email, string tenantId);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<string> GenerateJwtToken(UserDto user);
    Task<string> GenerateRefreshToken(string userId, string tenantId);
}
```

**File:** `Application/Services/AuthenticationService.cs`

```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthenticationService> _logger;
    
    public AuthenticationService(
        IUserRepository userRepository,
        IAuthTokenRepository tokenRepository,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    
    public async Task<LoginResult> LoginAsync(string email, string password, string tenantId)
    {
        var user = await _userRepository.GetByEmail(email, tenantId);
        if (user == null)
        {
            await LogFailedLoginAttempt(email, "User not found", tenantId);
            return new LoginResult { Success = false, Message = "Invalid email or password" };
        }
        
        if (!VerifyPassword(password, user.PasswordHash))
        {
            await LogFailedLoginAttempt(email, "Invalid password", tenantId);
            return new LoginResult { Success = false, Message = "Invalid email or password" };
        }
        
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshToken(user.UserId, tenantId);
        
        await LogSuccessfulLogin(user.UserId, tenantId);
        
        return new LoginResult 
        { 
            Success = true, 
            JwtToken = jwtToken,
            RefreshToken = refreshToken,
            User = user
        };
    }
    
    public async Task<string> GenerateJwtToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", user.UserId),
                new Claim("email", user.Email),
                new Claim("name", user.DisplayName),
                new Claim("tenant", user.TenantId)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

**Example 3: NotificationService**

**File:** `Application/Services/INotificationService.cs`

```csharp
public interface INotificationService
{
    Task<bool> SendNotificationAsync(NotificationRequest request);
    Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, string tenantId);
    Task<bool> SendSmsAsync(string phoneNumber, string message, string tenantId);
}
```

**File:** `Application/Services/NotificationService.cs`

```csharp
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailQueueRepository _emailQueueRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    
    public async Task<bool> SendNotificationAsync(NotificationRequest request)
    {
        var notification = new NotificationDto
        {
            NotificationType = request.Type,
            RecipientType = "User",
            RecipientId = request.UserId,
            Subject = request.Subject,
            Message = request.Message,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _notificationRepository.UpsertAsync(notification);
        return result != null;
    }
    
    public async Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId)
    {
        return await _notificationRepository.GetUnread(userId, tenantId);
    }
    
    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        return await _notificationRepository.MarkAsRead(notificationId);
    }
    
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string tenantId)
    {
        var emailQueue = new EmailQueueDto
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            IsHtml = true,
            Status = "Pending",
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _emailQueueRepository.UpsertAsync(emailQueue);
        return result != null;
    }
}
```

### **3.3 Service Registration**

**File:** `Program.cs`

```csharp
// Register application services
services.AddScoped<ILookupService, LookupService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IConfigurationService, ConfigurationService>();
services.AddScoped<IAuthenticationService, AuthenticationService>();
services.AddScoped<IBlogService, BlogService>();
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IAuditService, AuditService>();

// Add caching
services.AddMemoryCache();

// Add JWT authentication
var jwtSettings = new JwtSettings();
configuration.GetSection("JwtSettings").Bind(jwtSettings);
services.AddSingleton(Options.Create(jwtSettings));
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtSettings.Secret))
        };
    });
```

---

## **Phase 4: API Controllers (RESTful)**

### **4.1 API Project Structure**

```
src/SmartWorkz.StarterKitMVC.Web/
├── Controllers/
│   ├── Api/
│   │   ├── LookupsController.cs
│   │   ├── UsersController.cs
│   │   ├── AuthenticationController.cs
│   │   ├── BlogsController.cs
│   │   ├── ConfigurationsController.cs
│   │   ├── NotificationsController.cs
│   │   └── ReportsController.cs
│   └── MVC/
│       └── (Traditional MVC controllers)
├── Filters/
│   ├── AuthorizeAttribute.cs
│   ├── ValidateModelAttribute.cs
│   └── TenantIdAttribute.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── TenantMiddleware.cs
```

### **4.2 API Controller Examples**

**Example 1: LookupsController**

**File:** `Controllers/Api/LookupsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<LookupsController> _logger;
    
    public LookupsController(ILookupService lookupService, ILogger<LookupsController> logger)
    {
        _lookupService = lookupService;
        _logger = logger;
    }
    
    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies()
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        try
        {
            var currencies = await _lookupService.GetCurrencies(tenantId);
            return Ok(new { success = true, data = currencies });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currencies");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
    
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages()
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        try
        {
            var languages = await _lookupService.GetLanguages(tenantId);
            return Ok(new { success = true, data = languages });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting languages");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
    
    [HttpGet("timezones")]
    public async Task<IActionResult> GetTimeZones()
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        try
        {
            var timeZones = await _lookupService.GetTimeZones(tenantId);
            return Ok(new { success = true, data = timeZones });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timezones");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOrUpdate([FromBody] LookupDto lookup)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var tenantId = User.FindFirst("tenant")?.Value;
        lookup.TenantId = tenantId;
        
        try
        {
            var result = await _lookupService.UpsertAsync(lookup);
            return Ok(new { success = true, data = result, message = "Lookup saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving lookup");
            return StatusCode(500, new { success = false, message = "Error saving lookup" });
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _lookupService.DeleteAsync(id);
            if (result)
                return Ok(new { success = true, message = "Lookup deleted successfully" });
            return NotFound(new { success = false, message = "Lookup not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup");
            return StatusCode(500, new { success = false, message = "Error deleting lookup" });
        }
    }
}
```

**Example 2: AuthenticationController**

**File:** `Controllers/Api/AuthenticationController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthenticationController> _logger;
    
    public AuthenticationController(
        IAuthenticationService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            var result = await _authService.LoginAsync(request.Email, request.Password, request.TenantId);
            if (!result.Success)
                return Unauthorized(new { success = false, message = result.Message });
            
            return Ok(new { 
                success = true, 
                data = result,
                message = "Login successful" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(new { 
                success = true, 
                data = user,
                message = "Registration successful" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        var tenantId = User.FindFirst("tenant")?.Value;
        
        try
        {
            var newToken = await _authService.RefreshAccessToken(request.RefreshToken, userId, tenantId);
            return Ok(new { 
                success = true,
                data = new { accessToken = newToken },
                message = "Token refreshed" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh error");
            return StatusCode(500, new { success = false, message = "Token refresh failed" });
        }
    }
}
```

**Example 3: ConfigurationController**

**File:** `Controllers/Api/ConfigurationController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configService;
    
    public ConfigurationsController(IConfigurationService configService)
    {
        _configService = configService;
    }
    
    [HttpGet("{key}")]
    public async Task<IActionResult> GetConfiguration(string key)
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        var config = await _configService.GetByKeyAsync(key, tenantId);
        if (config == null)
            return NotFound();
        return Ok(new { success = true, data = config });
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveConfiguration([FromBody] ConfigurationDto config)
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        config.TenantId = tenantId;
        
        var result = await _configService.SaveAsync(config);
        return Ok(new { success = true, data = result, message = "Configuration saved" });
    }
}
```

### **4.3 Global Error Handling**

**File:** `Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var response = new { success = false, message = ex.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

### **4.4 Startup Configuration**

**File:** `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add repositories, services, authentication (as above)

var app = builder.Build();

// Enable middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
```

---

## **Phase 5: Admin UI (MVC Razor Views)**

### **5.1 Admin View Structure**

```
src/SmartWorkz.StarterKitMVC.Admin/
├── Controllers/
│   ├── DashboardController.cs
│   ├── LookupsController.cs
│   ├── UsersController.cs
│   ├── ConfigurationsController.cs
│   ├── BlogsController.cs
│   └── ReportsController.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _NavBar.cshtml
│   ├── Dashboard/
│   │   └── Index.cshtml
│   ├── Lookups/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Users/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── Configurations/
│   │   ├── Index.cshtml
│   │   └── Edit.cshtml
│   ├── Blogs/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   └── Reports/
│       ├── Index.cshtml
│       └── View.cshtml
└── wwwroot/
    ├── css/
    ├── js/
    └── images/
```

### **5.2 Admin Controllers (MVC)**

**Example: LookupsController (MVC)**

**File:** `Admin/Controllers/LookupsController.cs`

```csharp
[Authorize(Roles = "Admin")]
public class LookupsController : Controller
{
    private readonly ILookupService _lookupService;
    
    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }
    
    public async Task<IActionResult> Index(string category = "currencies")
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        var lookups = await _lookupService.GetByCategory(category, tenantId);
        
        ViewData["Category"] = category;
        return View(lookups);
    }
    
    public IActionResult Create(string category)
    {
        var model = new LookupViewModel { CategoryKey = category };
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LookupViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var tenantId = User.FindFirst("tenant")?.Value;
        var lookup = new LookupDto
        {
            CategoryKey = model.CategoryKey,
            Key = model.Key,
            DisplayName = model.DisplayName,
            TenantId = tenantId,
            IsActive = model.IsActive
        };
        
        await _lookupService.UpsertAsync(lookup);
        return RedirectToAction("Index", new { category = model.CategoryKey });
    }
}
```

### **5.3 Admin Razor Views**

**Example: Lookups Index View**

**File:** `Admin/Views/Lookups/Index.cshtml`

```html
@model IEnumerable<LookupDto>

@{
    ViewData["Title"] = "Manage Lookups";
    var category = ViewData["Category"]?.ToString() ?? "currencies";
}

<div class="container-fluid">
    <div class="row mb-3">
        <div class="col-md-8">
            <h1>@ViewData["Title"]</h1>
        </div>
        <div class="col-md-4 text-end">
            <a href="@Url.Action("Create", new { category = category })" class="btn btn-primary">
                <i class="fas fa-plus"></i> Add Lookup
            </a>
        </div>
    </div>
    
    <!-- Category Tabs -->
    <ul class="nav nav-tabs mb-3" role="tablist">
        <li class="nav-item" role="presentation">
            <a class="nav-link @(category == "currencies" ? "active" : "")" 
               href="@Url.Action("Index", new { category = "currencies" })">
                Currencies
            </a>
        </li>
        <li class="nav-item" role="presentation">
            <a class="nav-link @(category == "languages" ? "active" : "")" 
               href="@Url.Action("Index", new { category = "languages" })">
                Languages
            </a>
        </li>
        <li class="nav-item" role="presentation">
            <a class="nav-link @(category == "timezones" ? "active" : "")" 
               href="@Url.Action("Index", new { category = "timezones" })">
                Time Zones
            </a>
        </li>
    </ul>
    
    <!-- Table -->
    <div class="card">
        <table class="table table-striped table-hover">
            <thead>
                <tr>
                    <th>Key</th>
                    <th>Display Name</th>
                    <th>Sort Order</th>
                    <th>Active</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var lookup in Model)
                {
                    <tr>
                        <td>@lookup.Key</td>
                        <td>@lookup.DisplayName</td>
                        <td>@lookup.SortOrder</td>
                        <td>
                            @if (lookup.IsActive)
                            {
                                <span class="badge bg-success">Active</span>
                            }
                            else
                            {
                                <span class="badge bg-danger">Inactive</span>
                            }
                        </td>
                        <td>
                            <a href="@Url.Action("Edit", new { id = lookup.Id })" class="btn btn-sm btn-primary">
                                Edit
                            </a>
                            <a href="@Url.Action("Delete", new { id = lookup.Id })" class="btn btn-sm btn-danger">
                                Delete
                            </a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</div>
```

---

## **Phase 6: Public UI (MVC Razor Views)**

### **6.1 Public View Structure**

```
src/SmartWorkz.StarterKitMVC.Public/
├── Controllers/
│   ├── HomeController.cs
│   ├── AuthenticationController.cs
│   ├── ProfileController.cs
│   ├── BlogController.cs
│   └── ContactController.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _Footer.cshtml
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Authentication/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── ForgotPassword.cshtml
│   ├── Profile/
│   │   ├── Index.cshtml
│   │   └── Edit.cshtml
│   ├── Blog/
│   │   ├── Index.cshtml
│   │   └── Post.cshtml
│   └── Contact/
│       └── Index.cshtml
└── wwwroot/
    ├── css/
    ├── js/
    └── images/
```

### **6.2 Public Controllers**

**Example: BlogController (Public)**

**File:** `Public/Controllers/BlogController.cs`

```csharp
public class BlogController : Controller
{
    private readonly IBlogService _blogService;
    
    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }
    
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        var blogs = await _blogService.GetPublishedAsync(page, pageSize);
        return View(blogs);
    }
    
    public async Task<IActionResult> Post(string slug)
    {
        var blog = await _blogService.GetBySlugAsync(slug);
        if (blog == null)
            return NotFound();
        return View(blog);
    }
}
```

---

## **Phase 7: Testing**

### **7.1 Unit Tests**

```
tests/SmartWorkz.StarterKitMVC.Tests.Unit/
├── Services/
│   ├── LookupServiceTests.cs
│   ├── UserServiceTests.cs
│   └── AuthenticationServiceTests.cs
└── Repositories/
    └── LookupRepositoryTests.cs
```

### **7.2 Integration Tests**

```
tests/SmartWorkz.StarterKitMVC.Tests.Integration/
├── Controllers/
│   ├── LookupsControllerTests.cs
│   ├── AuthenticationControllerTests.cs
│   └── ConfigurationsControllerTests.cs
└── Database/
    └── DatabaseTests.cs
```

---

## **Development Timeline**

| Phase | Task | Duration | Status |
|-------|------|----------|--------|
| 1 | Database Schema (V3) | 1 day | ✅ Done |
| 1 | Stored Procedures | 1 day | ⏳ TODO |
| 1 | Seed Data | 1 day | ⏳ TODO |
| 1 | Materialized Views | 1 day | ⏳ TODO |
| 2 | Dapper Repositories | 2 days | ⏳ TODO |
| 3 | Service Layer | 3 days | ⏳ TODO |
| 4 | API Controllers | 2 days | ⏳ TODO |
| 5 | Admin UI | 3 days | ⏳ TODO |
| 6 | Public UI | 3 days | ⏳ TODO |
| 7 | Testing | 2 days | ⏳ TODO |
| | **TOTAL** | **18 days** | **In Progress** |

---

## **Success Criteria**

- [x] V3 schema created with all 38+ tables
- [ ] All CRUD operations working via Dapper
- [ ] Service layer implements business logic
- [ ] API endpoints functional with Swagger docs
- [ ] Admin UI functional for full CRUD
- [ ] Public UI displays lookups, blogs, content
- [ ] Authentication working (JWT + OAuth)
- [ ] Unit and integration tests passing
- [ ] Code coverage > 80%
- [ ] Performance optimized (response time < 200ms)
- [ ] Security best practices implemented (auth, authorization, validation)

---

**Next: Execute Phase 1 remaining steps (stored procedures, seed data, views)**
