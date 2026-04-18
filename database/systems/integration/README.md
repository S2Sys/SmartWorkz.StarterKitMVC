# API & Integration Systems

**Purpose**: Manage API rate limiting, external service integrations, health monitoring, and service event tracking.

## 📂 Files in This Group

| # | File | Purpose | Key Tables | Key Procedures |
|---|------|---------|-----------|-----------------|
| 1 | 01_RATE_LIMITING.sql | API rate limiting by key/IP | RateLimitPolicies, RateLimitTracking | spCheckRateLimit, spCreateRateLimitPolicy, spUpdateRateLimitPolicy, spGetRateLimitStatus, Report.spRateLimitingReport |
| 2 | 02_EXTERNAL_SERVICES.sql | External service registry & integration | ExternalServices, ServiceIntegrationEvents | spRegisterExternalService, spLogIntegrationEvent, spHealthCheckExternalService, spGetExternalService, spDisableExternalService, Report.spIntegrationEventsReport |

## 🎯 When to Deploy

**Phase**: Before API goes public, after Phase 1 foundation  
**Timing**: Essential for production API protection  
**Effort**: 1 hour to deploy + 2 hours to configure policies

## 🔧 Quick Setup

### 1. Deploy
```sql
USE Boilerplate;
:r integration/01_RATE_LIMITING.sql
:r integration/02_EXTERNAL_SERVICES.sql
```

### 2. Create Rate Limiting Policies
```sql
-- Public API - 100 requests per minute per key
EXEC dbo.spCreateRateLimitPolicy
    @PolicyName='PublicAPI',
    @ApiKeyPattern='pub_*',
    @MaxRequestsPerMinute=100,
    @MaxRequestsPerHour=5000,
    @IsActive=1;

-- Premium API - 1000 requests per minute
EXEC dbo.spCreateRateLimitPolicy
    @PolicyName='PremiumAPI',
    @ApiKeyPattern='premium_*',
    @MaxRequestsPerMinute=1000,
    @MaxRequestsPerHour=50000,
    @IsActive=1;

-- Internal API - Unlimited
EXEC dbo.spCreateRateLimitPolicy
    @PolicyName='Internal',
    @ApiKeyPattern='internal_*',
    @MaxRequestsPerMinute=10000,
    @MaxRequestsPerHour=999999,
    @IsActive=1;
```

### 3. Register External Services
```sql
-- Register Stripe payment gateway
DECLARE @ServiceId UNIQUEIDENTIFIER;
EXEC Shared.spRegisterExternalService
    @ServiceName='Stripe',
    @ServiceType='PaymentGateway',
    @BaseUrl='https://api.stripe.com/v1',
    @ApiKey='sk_live_...',
    @Timeout=30,
    @MaxRetries=3,
    @ServiceId=@ServiceId OUTPUT;

-- Register SendGrid email service
EXEC Shared.spRegisterExternalService
    @ServiceName='SendGrid',
    @ServiceType='EmailService',
    @BaseUrl='https://api.sendgrid.com/v3',
    @ApiKey='SG.xxx_yyy...',
    @Timeout=20,
    @MaxRetries=2;
```

### 4. Check Rate Limits
```sql
-- Check if request allowed
DECLARE @Remaining INT;
EXEC dbo.spCheckRateLimit
    @ApiKey='pub_abc123',
    @IpAddress='192.168.1.1',
    @Endpoint='/api/users',
    @RemainingRequests=@Remaining OUTPUT;

IF @Remaining >= 0
    PRINT 'Request allowed. Remaining: ' + CAST(@Remaining AS NVARCHAR(10));
ELSE
    PRINT 'Rate limit exceeded!';
```

## 📊 System Flows

### Rate Limiting Flow
```
API Request received
  ↓
Extract API Key + IP Address
  ↓
EXEC spCheckRateLimit
  ↓
Check against RateLimitPolicies
  ↓
If under limit:
  - Increment RateLimitTracking
  - Return remaining count
  ↓
If over limit:
  - Return 429 Too Many Requests
  - Log violation
```

### External Service Integration Flow
```
Application calls external service
  ↓
EXEC spLogIntegrationEvent (request)
  ↓
Execute external API call
  ↓
Capture response + timing
  ↓
EXEC spLogIntegrationEvent (response)
  ↓
If successful: Mark IsSuccessful=1
If failed: Log error, retry with backoff
  ↓
EXEC spHealthCheckExternalService
  ↓
If failure rate >20%: Auto-disable service
```

## 💡 Common Tasks

### Task 1: Enforce Rate Limiting
```sql
-- Check before processing request
DECLARE @Remaining INT, @Allowed BIT;
EXEC dbo.spCheckRateLimit
    @ApiKey='pub_abc123',
    @IpAddress=@UserIp,
    @Endpoint=@ApiEndpoint,
    @RemainingRequests=@Remaining OUTPUT,
    @IsAllowed=@Allowed OUTPUT;

IF @Allowed = 0
BEGIN
    -- Return HTTP 429 Too Many Requests
    RAISERROR('Rate limit exceeded', 16, 1);
END
-- Otherwise, continue processing request
```

### Task 2: Log Integration Events
```sql
-- Before calling external service
EXEC Shared.spLogIntegrationEvent
    @ExternalServiceId=@ServiceId,
    @EventType='PaymentAuthorization',
    @RequestData='{"amount": 99.99, "currency": "USD"}',
    @IsSuccessful=0;  -- Pre-log as pending

-- After success
EXEC Shared.spLogIntegrationEvent
    @ExternalServiceId=@ServiceId,
    @EventType='PaymentAuthorization',
    @RequestData='{"amount": 99.99, "currency": "USD"}',
    @ResponseData='{"transaction_id": "txn_123"}',
    @StatusCode=200,
    @ExecutionTimeMs=450,
    @IsSuccessful=1;

-- After failure
EXEC Shared.spLogIntegrationEvent
    @ExternalServiceId=@ServiceId,
    @EventType='PaymentAuthorization',
    @RequestData='{"amount": 99.99, "currency": "USD"}',
    @StatusCode=500,
    @ErrorMessage='Service temporarily unavailable',
    @ExecutionTimeMs=5000,
    @IsSuccessful=0;
```

### Task 3: Monitor Service Health
```sql
-- Check health status
DECLARE @IsHealthy BIT;
EXEC Shared.spHealthCheckExternalService
    @ExternalServiceId=@ServiceId,
    @IsHealthy=@IsHealthy OUTPUT;

IF @IsHealthy = 0
BEGIN
    -- Service is unhealthy
    EXEC Shared.spDisableExternalService
        @ExternalServiceId=@ServiceId,
        @Reason='Service failure rate exceeded threshold';
        
    -- Alert operations team
    PRINT 'Service disabled due to health check failure!';
END
```

### Task 4: View Rate Limiting Status
```sql
EXEC dbo.spGetRateLimitStatus @ApiKey='pub_abc123';

-- Result columns:
-- - ApiKey
-- - RequestsThisMinute
-- - RequestsThisHour
-- - MaxPerMinute
-- - MaxPerHour
-- - RemainingThis Minute
-- - RemainingThisHour
-- - LastRequest
```

### Task 5: Generate Integration Report
```sql
EXEC Report.spIntegrationEventsReport @DaysToAnalyze=7;

-- Shows:
-- - Service status overview (healthy/unhealthy)
-- - Recent integration failures
-- - Performance metrics (response times)
-- - Success rates per service
```

### Task 6: Generate Rate Limiting Report
```sql
EXEC Report.spRateLimitingReport;

-- Shows:
-- - API key usage statistics
-- - Limit violation frequency
-- - Top consumers by requests
-- - Rate limit effectiveness
```

## 🎯 Rate Limiting Strategy

### Tier-Based Approach
```
Free Tier:    100 req/min,  5K req/day
Standard:     500 req/min, 25K req/day
Premium:    1000 req/min, 50K req/day
Enterprise:  Custom (10K+ req/min)
```

### Implementation in API
```csharp
// ASP.NET Middleware
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDbConnection _db;

    public RateLimitMiddleware(RequestDelegate next, IDbConnection db)
    {
        _next = next;
        _db = db;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress.ToString();
        var endpoint = context.Request.Path.ToString();

        // Check rate limit
        using (var cmd = _db.CreateCommand())
        {
            cmd.CommandText = "dbo.spCheckRateLimit";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ApiKey", apiKey);
            cmd.Parameters.AddWithValue("@IpAddress", ipAddress);
            cmd.Parameters.AddWithValue("@Endpoint", endpoint);

            var remainingParam = cmd.Parameters.Add("@RemainingRequests", SqlDbType.Int);
            remainingParam.Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            int remaining = (int)remainingParam.Value;
            if (remaining < 0)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        }

        await _next(context);
    }
}
```

## 📈 Rate Limit Monitoring

### View Rate Limit Violations
```sql
SELECT TOP 20
    ApiKey,
    Endpoint,
    IpAddress,
    COUNT(*) AS ViolationCount,
    MAX(CheckedAt) AS LastViolation
FROM dbo.RateLimitTracking
WHERE IsViolation = 1
AND CheckedAt >= DATEADD(DAY, -1, GETUTCDATE())
GROUP BY ApiKey, Endpoint, IpAddress
ORDER BY ViolationCount DESC;
```

### Identify Abusive Clients
```sql
SELECT TOP 10
    ApiKey,
    COUNT(*) AS RequestCount,
    SUM(CASE WHEN IsViolation=1 THEN 1 ELSE 0 END) AS Violations,
    CAST(SUM(CASE WHEN IsViolation=1 THEN 1 ELSE 0 END) * 100.0 /
        NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS ViolationRate
FROM dbo.RateLimitTracking
WHERE CheckedAt >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY ApiKey
HAVING SUM(CASE WHEN IsViolation=1 THEN 1 ELSE 0 END) > 10
ORDER BY Violations DESC;
```

## 📊 Integration Monitoring

### Service Health Overview
```sql
SELECT
    ServiceName,
    ServiceType,
    IsActive,
    IsHealthy,
    LastHealthCheckAt,
    (SELECT COUNT(*) FROM Shared.ServiceIntegrationEvents 
     WHERE ExternalServiceId=es.ExternalServiceId
     AND CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
     AND IsSuccessful=0) AS RecentFailures
FROM Shared.ExternalServices es
WHERE IsDeleted=0
ORDER BY IsHealthy DESC, ServiceName;
```

### Integration Performance
```sql
SELECT
    ServiceName,
    AVG(ExecutionTimeMs) AS AvgResponseTimeMs,
    MAX(ExecutionTimeMs) AS MaxResponseTimeMs,
    MIN(ExecutionTimeMs) AS MinResponseTimeMs,
    COUNT(*) AS TotalRequests,
    SUM(CASE WHEN IsSuccessful=1 THEN 1 ELSE 0 END) AS SuccessfulRequests
FROM Shared.ServiceIntegrationEvents sie
JOIN Shared.ExternalServices es ON sie.ExternalServiceId=es.ExternalServiceId
WHERE CreatedAt >= DATEADD(DAY, -7, GETUTCDATE())
AND IsDeleted=0
GROUP BY ServiceName
ORDER BY AvgResponseTimeMs DESC;
```

## 🔐 Security Best Practices

### API Key Management
```sql
-- Store keys encrypted
INSERT INTO Shared.ExternalServices
(ServiceName, ServiceType, BaseUrl, ApiKey, IsEncrypted)
VALUES ('Stripe', 'Payment', 'https://api.stripe.com', 'encrypted_key_here', 1);

-- Never log full keys
-- Use KEY encryption in database
-- Rotate keys periodically
-- Audit key access
```

### Rate Limit Bypass Prevention
```sql
-- Don't allow unlimited rates for external keys
UPDATE dbo.RateLimitPolicies
SET MaxRequestsPerMinute=100
WHERE ApiKeyPattern LIKE 'pub_%'
AND IsActive=1;

-- Monitor for bypass attempts
SELECT * FROM dbo.RateLimitTracking
WHERE IsViolation=1
AND Endpoint IN ('/admin', '/internal', '/debug');
```

## 🚨 Troubleshooting

### "Rate limiting not working"
```sql
-- Check policies exist
SELECT * FROM dbo.RateLimitPolicies WHERE IsActive=1;

-- Check tracking data
SELECT TOP 10 * FROM dbo.RateLimitTracking 
ORDER BY CheckedAt DESC;

-- Verify middleware is invoking spCheckRateLimit
```

### "External service always shows unhealthy"
```sql
-- Check recent events
SELECT TOP 20 * FROM Shared.ServiceIntegrationEvents
WHERE ExternalServiceId=@ServiceId
ORDER BY CreatedAt DESC;

-- Check failure rate
SELECT
    SUM(CASE WHEN IsSuccessful=1 THEN 1 ELSE 0 END) AS SuccessCount,
    SUM(CASE WHEN IsSuccessful=0 THEN 1 ELSE 0 END) AS FailureCount,
    CAST(SUM(CASE WHEN IsSuccessful=0 THEN 1 ELSE 0 END) * 100.0 /
        COUNT(*) AS DECIMAL(5,2)) AS FailureRate
FROM Shared.ServiceIntegrationEvents
WHERE ExternalServiceId=@ServiceId
AND CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
AND IsDeleted=0;
```

### "API key blocking legitimate traffic"
```sql
-- Review violations for specific key
SELECT * FROM dbo.RateLimitTracking
WHERE ApiKey='pub_xxx'
AND IsViolation=1
ORDER BY CheckedAt DESC;

-- Increase limit if legitimate
EXEC dbo.spUpdateRateLimitPolicy
    @PolicyName='PublicAPI',
    @MaxRequestsPerMinute=200;
```

## 📋 Procedures

### Rate Limiting Procedures
- **spCheckRateLimit**: Validate request against limit
- **spCreateRateLimitPolicy**: Create new policy
- **spUpdateRateLimitPolicy**: Modify existing policy
- **spGetRateLimitStatus**: Check current usage
- **Report.spRateLimitingReport**: Usage analytics

### External Service Procedures
- **spRegisterExternalService**: Register service
- **spLogIntegrationEvent**: Log API call
- **spHealthCheckExternalService**: Check service health
- **spGetExternalService**: Retrieve service config
- **spDisableExternalService**: Disable unhealthy service
- **Report.spIntegrationEventsReport**: Integration analytics

## 🔗 Integration Points

- **Operations**: Run health checks on schedule
- **Observability**: Display rate limit metrics on dashboard
- **Data Quality**: Validate integration data format
- **Performance**: Monitor integration response times

---

**Group**: API & Integration  
**Files**: 2 SQL scripts  
**Tables**: 4 policy and event tables  
**Procedures**: 11 rate limiting and integration procedures  
**Status**: Production-ready
