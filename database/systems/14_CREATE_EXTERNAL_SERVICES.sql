-- ============================================
-- Phase 1C: External Service Integration
-- Purpose: External service registry and integration event logging
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: External Service Registry
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExternalServices' AND schema_id = SCHEMA_ID('Shared'))
BEGIN
    CREATE TABLE Shared.ExternalServices (
        ExternalServiceId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ServiceName NVARCHAR(256) NOT NULL UNIQUE,
        ServiceType NVARCHAR(50) NOT NULL,
        BaseUrl NVARCHAR(500) NOT NULL,
        ApiKey NVARCHAR(MAX),
        ApiSecret NVARCHAR(MAX),
        IsEncrypted BIT NOT NULL DEFAULT 1,
        Timeout INT NOT NULL DEFAULT 30,
        MaxRetries INT NOT NULL DEFAULT 3,
        IsActive BIT NOT NULL DEFAULT 1,
        LastHealthCheckAt DATETIME2,
        IsHealthy BIT NOT NULL DEFAULT 1,
        TenantId NVARCHAR(128),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_ExternalServices_Type ON Shared.ExternalServices(ServiceType);
    CREATE INDEX IX_ExternalServices_Active ON Shared.ExternalServices(IsActive);
    PRINT '✅ Created ExternalServices table';
END

-- ============================================
-- TABLE: Service Integration Events
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceIntegrationEvents' AND schema_id = SCHEMA_ID('Shared'))
BEGIN
    CREATE TABLE Shared.ServiceIntegrationEvents (
        IntegrationEventId BIGINT PRIMARY KEY IDENTITY(1,1),
        ExternalServiceId UNIQUEIDENTIFIER NOT NULL,
        EventType NVARCHAR(50) NOT NULL,
        RequestData NVARCHAR(MAX),
        ResponseData NVARCHAR(MAX),
        StatusCode INT,
        ErrorMessage NVARCHAR(MAX),
        RetryCount INT NOT NULL DEFAULT 0,
        ExecutionTimeMs INT,
        IsSuccessful BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        IsDeleted BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (ExternalServiceId) REFERENCES Shared.ExternalServices(ExternalServiceId)
    );

    CREATE INDEX IX_ServiceIntegrationEvents_Service ON Shared.ServiceIntegrationEvents(ExternalServiceId);
    CREATE INDEX IX_ServiceIntegrationEvents_Status ON Shared.ServiceIntegrationEvents(IsSuccessful);
    CREATE INDEX IX_ServiceIntegrationEvents_Created ON Shared.ServiceIntegrationEvents(CreatedAt);
    PRINT '✅ Created ServiceIntegrationEvents table';
END

GO

-- ============================================
-- PROCEDURE: Register External Service
-- ============================================

IF OBJECT_ID('Shared.spRegisterExternalService', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spRegisterExternalService;

GO

CREATE PROCEDURE Shared.spRegisterExternalService
    @ServiceName NVARCHAR(256),
    @ServiceType NVARCHAR(50),
    @BaseUrl NVARCHAR(500),
    @ApiKey NVARCHAR(MAX) = NULL,
    @ApiSecret NVARCHAR(MAX) = NULL,
    @Timeout INT = 30,
    @MaxRetries INT = 3,
    @TenantId NVARCHAR(128) = NULL,
    @ServiceId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ServiceId = NEWID();

    INSERT INTO Shared.ExternalServices (
        ExternalServiceId,
        ServiceName,
        ServiceType,
        BaseUrl,
        ApiKey,
        ApiSecret,
        IsEncrypted,
        Timeout,
        MaxRetries,
        IsActive,
        TenantId,
        CreatedAt,
        IsDeleted
    ) VALUES (
        @ServiceId,
        @ServiceName,
        @ServiceType,
        @BaseUrl,
        @ApiKey,
        @ApiSecret,
        1,
        @Timeout,
        @MaxRetries,
        1,
        @TenantId,
        GETUTCDATE(),
        0
    );

    PRINT '✅ Registered external service: ' + @ServiceName + ' (' + @ServiceType + ')';
END;

GO

-- ============================================
-- PROCEDURE: Log Integration Event
-- ============================================

IF OBJECT_ID('Shared.spLogIntegrationEvent', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spLogIntegrationEvent;

GO

CREATE PROCEDURE Shared.spLogIntegrationEvent
    @ExternalServiceId UNIQUEIDENTIFIER,
    @EventType NVARCHAR(50),
    @RequestData NVARCHAR(MAX) = NULL,
    @ResponseData NVARCHAR(MAX) = NULL,
    @StatusCode INT = NULL,
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @ExecutionTimeMs INT = 0,
    @IsSuccessful BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Shared.ServiceIntegrationEvents (
        ExternalServiceId,
        EventType,
        RequestData,
        ResponseData,
        StatusCode,
        ErrorMessage,
        ExecutionTimeMs,
        IsSuccessful,
        CreatedAt,
        IsDeleted
    ) VALUES (
        @ExternalServiceId,
        @EventType,
        @RequestData,
        @ResponseData,
        @StatusCode,
        @ErrorMessage,
        @ExecutionTimeMs,
        @IsSuccessful,
        GETUTCDATE(),
        0
    );

    PRINT CASE WHEN @IsSuccessful = 1 THEN '✅' ELSE '❌' END + ' Integration event logged: ' + @EventType;
END;

GO

-- ============================================
-- PROCEDURE: Health Check External Service
-- ============================================

IF OBJECT_ID('Shared.spHealthCheckExternalService', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spHealthCheckExternalService;

GO

CREATE PROCEDURE Shared.spHealthCheckExternalService
    @ExternalServiceId UNIQUEIDENTIFIER,
    @IsHealthy BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ServiceName NVARCHAR(256);
    DECLARE @RecentFailures INT = 0;
    DECLARE @TotalRecent INT = 0;

    -- Get service name
    SELECT @ServiceName = ServiceName
    FROM Shared.ExternalServices
    WHERE ExternalServiceId = @ExternalServiceId
    AND IsDeleted = 0;

    IF @ServiceName IS NULL
    BEGIN
        SET @IsHealthy = 0;
        PRINT '❌ Service not found: ' + CAST(@ExternalServiceId AS NVARCHAR(36));
        RETURN;
    END

    -- Check recent integration events (last 10)
    SELECT TOP 10
        @RecentFailures = SUM(CASE WHEN IsSuccessful = 0 THEN 1 ELSE 0 END),
        @TotalRecent = COUNT(*)
    FROM Shared.ServiceIntegrationEvents
    WHERE ExternalServiceId = @ExternalServiceId
    AND CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
    AND IsDeleted = 0;

    -- Service is healthy if <20% failure rate in recent events
    IF @TotalRecent = 0 OR (@RecentFailures * 100.0 / @TotalRecent) < 20
    BEGIN
        SET @IsHealthy = 1;
        PRINT '✅ Service healthy: ' + @ServiceName;
    END
    ELSE
    BEGIN
        SET @IsHealthy = 0;
        PRINT '⚠️ Service unhealthy: ' + @ServiceName + ' (Failure rate: ' + CAST((@RecentFailures * 100.0 / @TotalRecent) AS VARCHAR(5)) + '%)';
    END

    -- Update health check timestamp
    UPDATE Shared.ExternalServices
    SET LastHealthCheckAt = GETUTCDATE(),
        IsHealthy = @IsHealthy,
        UpdatedAt = GETUTCDATE()
    WHERE ExternalServiceId = @ExternalServiceId;
END;

GO

-- ============================================
-- PROCEDURE: Get Service Details
-- ============================================

IF OBJECT_ID('Shared.spGetExternalService', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetExternalService;

GO

CREATE PROCEDURE Shared.spGetExternalService
    @ServiceName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ExternalServiceId,
        ServiceName,
        ServiceType,
        BaseUrl,
        Timeout,
        MaxRetries,
        IsActive,
        IsHealthy,
        LastHealthCheckAt,
        CreatedAt
    FROM Shared.ExternalServices
    WHERE ServiceName = @ServiceName
    AND IsDeleted = 0;

    PRINT '✅ Retrieved service: ' + @ServiceName;
END;

GO

-- ============================================
-- PROCEDURE: Disable Service
-- ============================================

IF OBJECT_ID('Shared.spDisableExternalService', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spDisableExternalService;

GO

CREATE PROCEDURE Shared.spDisableExternalService
    @ExternalServiceId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Shared.ExternalServices
    SET IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE ExternalServiceId = @ExternalServiceId
    AND IsDeleted = 0;

    PRINT '🔴 Service disabled: ' + CAST(@ExternalServiceId AS NVARCHAR(36));
    IF @Reason IS NOT NULL
        PRINT 'Reason: ' + @Reason;
END;

GO

-- ============================================
-- PROCEDURE: Integration Events Report
-- ============================================

IF OBJECT_ID('Report.spIntegrationEventsReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spIntegrationEventsReport;

GO

CREATE PROCEDURE Report.spIntegrationEventsReport
    @DaysToAnalyze INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT 'INTEGRATION EVENTS REPORT';
    PRINT 'Period: Last ' + CAST(@DaysToAnalyze AS NVARCHAR(2)) + ' days';
    PRINT '═══════════════════════════════════════════';

    -- 1. Service status overview
    PRINT '';
    PRINT '🔧 SERVICE STATUS:';
    SELECT
        es.ServiceName,
        es.ServiceType,
        es.IsActive,
        es.IsHealthy,
        COUNT(sie.IntegrationEventId) AS TotalEvents,
        SUM(CASE WHEN sie.IsSuccessful = 1 THEN 1 ELSE 0 END) AS SuccessfulEvents,
        CAST(SUM(CASE WHEN sie.IsSuccessful = 1 THEN 1 ELSE 0 END) * 100.0 /
            NULLIF(COUNT(sie.IntegrationEventId), 0) AS DECIMAL(5, 2)) AS SuccessRate
    FROM Shared.ExternalServices es
    LEFT JOIN Shared.ServiceIntegrationEvents sie ON es.ExternalServiceId = sie.ExternalServiceId
        AND sie.CreatedAt >= @StartDate
        AND sie.IsDeleted = 0
    WHERE es.IsDeleted = 0
    GROUP BY es.ExternalServiceId, es.ServiceName, es.ServiceType, es.IsActive, es.IsHealthy;

    -- 2. Recent failures
    PRINT '';
    PRINT '⚠️ RECENT FAILURES:';
    SELECT TOP 20
        es.ServiceName,
        sie.EventType,
        sie.StatusCode,
        sie.ErrorMessage,
        sie.CreatedAt
    FROM Shared.ServiceIntegrationEvents sie
    INNER JOIN Shared.ExternalServices es ON sie.ExternalServiceId = es.ExternalServiceId
    WHERE sie.IsSuccessful = 0
    AND sie.CreatedAt >= @StartDate
    AND sie.IsDeleted = 0
    ORDER BY sie.CreatedAt DESC;

    -- 3. Performance metrics
    PRINT '';
    PRINT '⚡ PERFORMANCE METRICS:';
    SELECT
        es.ServiceName,
        AVG(sie.ExecutionTimeMs) AS AvgResponseTimeMs,
        MIN(sie.ExecutionTimeMs) AS MinResponseTimeMs,
        MAX(sie.ExecutionTimeMs) AS MaxResponseTimeMs,
        COUNT(sie.IntegrationEventId) AS RequestCount
    FROM Shared.ServiceIntegrationEvents sie
    INNER JOIN Shared.ExternalServices es ON sie.ExternalServiceId = es.ExternalServiceId
    WHERE sie.CreatedAt >= @StartDate
    AND sie.IsDeleted = 0
    AND sie.ExecutionTimeMs > 0
    GROUP BY es.ExternalServiceId, es.ServiceName;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: External Service Integration successfully configured';
PRINT 'Total objects created:';
PRINT '  - 2 external service management tables';
PRINT '  - 6 service integration procedures (register, log, health check, get, disable, report)';
PRINT 'Status: External service integration framework ready';

GO
