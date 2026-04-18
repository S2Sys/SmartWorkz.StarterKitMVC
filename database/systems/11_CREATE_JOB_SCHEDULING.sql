-- ============================================
-- Phase 1C: Job Scheduling & Automation
-- Purpose: SQL Agent jobs for automated maintenance and reporting
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Job Schedule Configuration
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobSchedules' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.JobSchedules (
        JobScheduleId INT PRIMARY KEY IDENTITY(1,1),
        JobName NVARCHAR(256) NOT NULL UNIQUE,
        JobDescription NVARCHAR(500),
        JobType NVARCHAR(50) NOT NULL,
        ScheduleFrequency NVARCHAR(50) NOT NULL,
        ScheduleTime TIME,
        ScheduleDayOfWeek INT = NULL,
        IsEnabled BIT NOT NULL DEFAULT 1,
        LastRunAt DATETIME2,
        NextRunAt DATETIME2,
        LastRunStatus NVARCHAR(50),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_JobSchedules_Enabled ON dbo.JobSchedules(IsEnabled);
    PRINT '✅ Created JobSchedules table';
END

-- ============================================
-- TABLE: Job Execution Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobExecutionLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.JobExecutionLogs (
        JobExecutionLogId BIGINT PRIMARY KEY IDENTITY(1,1),
        JobScheduleId INT NOT NULL,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2,
        Status NVARCHAR(50) NOT NULL,
        ErrorMessage NVARCHAR(MAX),
        RecordsProcessed INT,
        ExecutionTimeMs INT,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (JobScheduleId) REFERENCES dbo.JobSchedules(JobScheduleId)
    );

    CREATE INDEX IX_JobExecutionLogs_JobId ON dbo.JobExecutionLogs(JobScheduleId);
    CREATE INDEX IX_JobExecutionLogs_Status ON dbo.JobExecutionLogs(Status);
    PRINT '✅ Created JobExecutionLogs table';
END

GO

-- ============================================
-- PROCEDURE: Log Job Execution
-- ============================================

IF OBJECT_ID('dbo.spLogJobExecution', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spLogJobExecution;

GO

CREATE PROCEDURE dbo.spLogJobExecution
    @JobScheduleId INT,
    @Status NVARCHAR(50),
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @RecordsProcessed INT = 0,
    @ExecutionTimeMs INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartedAt DATETIME2 = GETUTCDATE();
    DECLARE @CompletedAt DATETIME2 = NULL;

    IF @Status = 'Success' OR @Status = 'Failed'
        SET @CompletedAt = GETUTCDATE();

    INSERT INTO dbo.JobExecutionLogs (
        JobScheduleId,
        StartedAt,
        CompletedAt,
        Status,
        ErrorMessage,
        RecordsProcessed,
        ExecutionTimeMs,
        CreatedAt,
        IsDeleted
    ) VALUES (
        @JobScheduleId,
        @StartedAt,
        @CompletedAt,
        @Status,
        @ErrorMessage,
        @RecordsProcessed,
        @ExecutionTimeMs,
        GETUTCDATE(),
        0
    );

    -- Update job schedule with last run info
    UPDATE dbo.JobSchedules
    SET LastRunAt = @StartedAt,
        LastRunStatus = @Status,
        UpdatedAt = GETUTCDATE()
    WHERE JobScheduleId = @JobScheduleId;

    PRINT '✅ Logged job execution: ' + CAST(@JobScheduleId AS NVARCHAR(10)) + ' - ' + @Status;
END;

GO

-- ============================================
-- PROCEDURE: Schedule Maintenance Job
-- ============================================

IF OBJECT_ID('dbo.spScheduleMaintenanceJob', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spScheduleMaintenanceJob;

GO

CREATE PROCEDURE dbo.spScheduleMaintenanceJob
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @JobScheduleId INT;
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    DECLARE @ExecutionTime INT;

    PRINT '═══════════════════════════════════════════';
    PRINT 'RUNNING MAINTENANCE JOB';
    PRINT '═══════════════════════════════════════════';

    -- 1. Clean expired tokens
    PRINT '';
    PRINT 'Step 1: Cleaning expired tokens...';
    DECLARE @DeletedCount INT;
    EXEC Auth.spCleanExpiredTokens @DaysToKeep = 90, @DryRun = 0, @DeletedCount = @DeletedCount OUTPUT;

    -- 2. Archive old audit logs
    PRINT '';
    PRINT 'Step 2: Archiving old audit logs...';
    DECLARE @ArchivedCount INT;
    EXEC Shared.spArchiveOldAuditLogs @MonthsToKeep = 12, @DryRun = 0, @ArchivedCount = @ArchivedCount OUTPUT;

    -- 3. Cleanup inactive logins
    PRINT '';
    PRINT 'Step 3: Cleaning up inactive logins...';
    DECLARE @UpdatedCount INT;
    EXEC Auth.spCleanupInactiveLogins @DaysInactive = 180, @DryRun = 0, @UpdatedCount = @UpdatedCount OUTPUT;

    -- 4. Clean expired cache
    PRINT '';
    PRINT 'Step 4: Cleaning expired cache...';
    EXEC dbo.spCleanExpiredCache @DryRun = 0;

    -- 5. Archive old notifications
    PRINT '';
    PRINT 'Step 5: Archiving old notifications...';
    EXEC Shared.spArchiveOldNotifications @DaysToKeep = 30, @DryRun = 0;

    -- 6. Data consistency check
    PRINT '';
    PRINT 'Step 6: Running data consistency checks...';
    EXEC dbo.spDataConsistencyCheck;

    SET @ExecutionTime = DATEDIFF(SECOND, @StartTime, GETUTCDATE());

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ MAINTENANCE JOB COMPLETED';
    PRINT 'Execution Time: ' + CAST(@ExecutionTime AS NVARCHAR(10)) + ' seconds';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Schedule Analytics Job
-- ============================================

IF OBJECT_ID('dbo.spScheduleAnalyticsJob', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spScheduleAnalyticsJob;

GO

CREATE PROCEDURE dbo.spScheduleAnalyticsJob
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'RUNNING ANALYTICS JOB';
    PRINT '═══════════════════════════════════════════';

    -- 1. Generate user activity report
    PRINT '';
    PRINT 'Generating user activity analytics...';
    EXEC Report.spUserActivityReport @DaysToAnalyze = 7;

    -- 2. Generate tenant usage statistics
    PRINT '';
    PRINT 'Generating tenant usage statistics...';
    EXEC Report.spTenantUsageStatistics;

    -- 3. Generate feature usage analytics
    PRINT '';
    PRINT 'Generating feature usage analytics...';
    EXEC Report.spFeatureUsageAnalytics;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ ANALYTICS JOB COMPLETED';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Schedule Health Check Job
-- ============================================

IF OBJECT_ID('dbo.spScheduleHealthCheckJob', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spScheduleHealthCheckJob;

GO

CREATE PROCEDURE dbo.spScheduleHealthCheckJob
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'RUNNING HEALTH CHECK JOB';
    PRINT '═══════════════════════════════════════════';

    -- 1. Detect orphaned records
    PRINT '';
    EXEC dbo.spDetectOrphanedRecords;

    -- 2. Data consistency
    PRINT '';
    EXEC dbo.spDataConsistencyCheck;

    -- 3. System health
    PRINT '';
    EXEC dbo.spSystemHealthCheck;

    -- 4. Security audit
    PRINT '';
    EXEC Auth.spSecurityAuditReport;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ HEALTH CHECK JOB COMPLETED';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Job Scheduler Master
-- ============================================

IF OBJECT_ID('dbo.spRunScheduledJobs', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spRunScheduledJobs;

GO

CREATE PROCEDURE dbo.spRunScheduledJobs
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentHour INT = DATEPART(HOUR, GETUTCDATE());
    DECLARE @CurrentDayOfWeek INT = DATEPART(WEEKDAY, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT 'JOB SCHEDULER MASTER';
    PRINT 'Current Time: ' + CONVERT(NVARCHAR(19), GETUTCDATE(), 121);
    PRINT '═══════════════════════════════════════════';

    -- Nightly maintenance (2 AM)
    IF @CurrentHour = 2
    BEGIN
        PRINT '';
        PRINT '⏰ Executing nightly maintenance...';
        EXEC dbo.spScheduleMaintenanceJob;
    END

    -- Weekly analytics (Sunday at 3 AM)
    IF @CurrentDayOfWeek = 1 AND @CurrentHour = 3
    BEGIN
        PRINT '';
        PRINT '📊 Executing weekly analytics...';
        EXEC dbo.spScheduleAnalyticsJob;
    END

    -- Daily health check (1 AM)
    IF @CurrentHour = 1
    BEGIN
        PRINT '';
        PRINT '🏥 Executing daily health check...';
        EXEC dbo.spScheduleHealthCheckJob;
    END

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ JOB SCHEDULER COMPLETE';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: Job Scheduling successfully configured';
PRINT 'Total objects created:';
PRINT '  - 2 job management tables (JobSchedules, JobExecutionLogs)';
PRINT '  - 5 job procedures (maintenance, analytics, health, master, log)';
PRINT 'Status: SQL Agent jobs ready for scheduling';
PRINT 'Note: Use SQL Server Agent to schedule spRunScheduledJobs execution';

GO
