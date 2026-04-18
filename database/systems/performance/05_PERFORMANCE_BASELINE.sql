-- ============================================
-- Phase 1C: Performance Baseline & Trending
-- Purpose: Baseline establishment, trend tracking, degradation alerts
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Performance Baseline
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerformanceBaseline' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PerformanceBaseline (
        BaselineId BIGINT PRIMARY KEY IDENTITY(1,1),
        MetricType NVARCHAR(100) NOT NULL,
        MetricName NVARCHAR(256) NOT NULL,
        BaselineValue DECIMAL(18, 4),
        BaselineUnit NVARCHAR(50),
        Percentile INT DEFAULT 50,
        SampleSize INT,
        MinObserved DECIMAL(18, 4),
        MaxObserved DECIMAL(18, 4),
        StandardDeviation DECIMAL(18, 4),
        BaselinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_PerformanceBaseline_Type ON dbo.PerformanceBaseline(MetricType);
    CREATE INDEX IX_PerformanceBaseline_Active ON dbo.PerformanceBaseline(IsActive);
    PRINT '✅ Created PerformanceBaseline table';
END

GO

-- ============================================
-- TABLE: Performance Trend Tracking
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerformanceTrends' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PerformanceTrends (
        TrendId BIGINT PRIMARY KEY IDENTITY(1,1),
        BaselineId BIGINT NOT NULL,
        MetricType NVARCHAR(100) NOT NULL,
        MetricName NVARCHAR(256) NOT NULL,
        CurrentValue DECIMAL(18, 4),
        BaselineValue DECIMAL(18, 4),
        VariancePercent DECIMAL(8, 2),
        TrendDirection NVARCHAR(20),
        IsDegraded BIT NOT NULL DEFAULT 0,
        DegradationThresholdPercent INT DEFAULT 10,
        AlertLevel NVARCHAR(20) NOT NULL DEFAULT 'NORMAL',
        ConsecutiveDegradations INT DEFAULT 0,
        MeasuredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (BaselineId) REFERENCES dbo.PerformanceBaseline(BaselineId)
    );

    CREATE INDEX IX_PerformanceTrends_Baseline ON dbo.PerformanceTrends(BaselineId);
    CREATE INDEX IX_PerformanceTrends_Status ON dbo.PerformanceTrends(AlertLevel);
    CREATE INDEX IX_PerformanceTrends_Measured ON dbo.PerformanceTrends(MeasuredAt);
    PRINT '✅ Created PerformanceTrends table';
END

GO

-- ============================================
-- TABLE: Performance Degradation Alerts
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerformanceDegradationAlerts' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PerformanceDegradationAlerts (
        AlertId BIGINT PRIMARY KEY IDENTITY(1,1),
        TrendId BIGINT NOT NULL,
        AlertType NVARCHAR(50) NOT NULL,
        MetricType NVARCHAR(100),
        MetricName NVARCHAR(256),
        BaselineValue DECIMAL(18, 4),
        CurrentValue DECIMAL(18, 4),
        DegradationPercent DECIMAL(8, 2),
        RootCauseAnalysis NVARCHAR(MAX),
        RecommendedAction NVARCHAR(MAX),
        Severity NVARCHAR(20) NOT NULL DEFAULT 'MEDIUM',
        IsAcknowledged BIT NOT NULL DEFAULT 0,
        AcknowledgedAt DATETIME2,
        AcknowledgedBy NVARCHAR(256),
        IsResolved BIT NOT NULL DEFAULT 0,
        ResolvedAt DATETIME2,
        AlertedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (TrendId) REFERENCES dbo.PerformanceTrends(TrendId)
    );

    CREATE INDEX IX_PerformanceDegradationAlerts_Trend ON dbo.PerformanceDegradationAlerts(TrendId);
    CREATE INDEX IX_PerformanceDegradationAlerts_Status ON dbo.PerformanceDegradationAlerts(IsResolved);
    CREATE INDEX IX_PerformanceDegradationAlerts_Severity ON dbo.PerformanceDegradationAlerts(Severity);
    PRINT '✅ Created PerformanceDegradationAlerts table';
END

GO

-- ============================================
-- PROCEDURE: Capture Performance Baseline
-- ============================================

IF OBJECT_ID('dbo.spCapturePerformanceBaseline', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spCapturePerformanceBaseline;

GO

CREATE PROCEDURE dbo.spCapturePerformanceBaseline
    @MetricType NVARCHAR(100),
    @MetricName NVARCHAR(256),
    @BaselineValue DECIMAL(18, 4),
    @BaselineUnit NVARCHAR(50) = NULL,
    @SampleSize INT = 100,
    @Percentile INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BaselineId BIGINT;
    DECLARE @MinValue DECIMAL(18, 4) = @BaselineValue * 0.9;
    DECLARE @MaxValue DECIMAL(18, 4) = @BaselineValue * 1.1;
    DECLARE @StdDev DECIMAL(18, 4) = @BaselineValue * 0.05;

    -- Check if baseline already exists
    IF EXISTS (SELECT 1 FROM dbo.PerformanceBaseline
              WHERE MetricType = @MetricType
              AND MetricName = @MetricName
              AND IsDeleted = 0)
    BEGIN
        UPDATE dbo.PerformanceBaseline
        SET BaselineValue = @BaselineValue,
            SampleSize = @SampleSize,
            Percentile = @Percentile,
            MinObserved = @MinValue,
            MaxObserved = @MaxValue,
            StandardDeviation = @StdDev,
            UpdatedAt = GETUTCDATE()
        WHERE MetricType = @MetricType
        AND MetricName = @MetricName
        AND IsDeleted = 0;

        PRINT '✅ Updated baseline: ' + @MetricName + ' (' + @MetricType + ')';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.PerformanceBaseline (
            MetricType,
            MetricName,
            BaselineValue,
            BaselineUnit,
            Percentile,
            SampleSize,
            MinObserved,
            MaxObserved,
            StandardDeviation,
            IsActive,
            IsDeleted
        ) VALUES (
            @MetricType,
            @MetricName,
            @BaselineValue,
            @BaselineUnit,
            @Percentile,
            @SampleSize,
            @MinValue,
            @MaxValue,
            @StdDev,
            1,
            0
        );

        PRINT '✅ Captured baseline: ' + @MetricName + ' (' + @MetricType + ') = ' + CAST(@BaselineValue AS NVARCHAR(20));
    END
END;

GO

-- ============================================
-- PROCEDURE: Track Performance Trends
-- ============================================

IF OBJECT_ID('dbo.spTrackPerformanceTrends', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spTrackPerformanceTrends;

GO

CREATE PROCEDURE dbo.spTrackPerformanceTrends
    @MetricType NVARCHAR(100),
    @MetricName NVARCHAR(256),
    @CurrentValue DECIMAL(18, 4),
    @DegradationThresholdPercent INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BaselineId BIGINT;
    DECLARE @BaselineValue DECIMAL(18, 4);
    DECLARE @VariancePercent DECIMAL(8, 2);
    DECLARE @TrendDirection NVARCHAR(20);
    DECLARE @IsDegraded BIT = 0;
    DECLARE @AlertLevel NVARCHAR(20) = 'NORMAL';

    -- Get baseline
    SELECT @BaselineId = BaselineId,
           @BaselineValue = BaselineValue
    FROM dbo.PerformanceBaseline
    WHERE MetricType = @MetricType
    AND MetricName = @MetricName
    AND IsActive = 1
    AND IsDeleted = 0;

    IF @BaselineId IS NULL
    BEGIN
        PRINT '⚠️ No baseline found for: ' + @MetricName + '. Run spCapturePerformanceBaseline first.';
        RETURN;
    END

    -- Calculate variance
    SET @VariancePercent = ((@CurrentValue - @BaselineValue) / NULLIF(@BaselineValue, 0)) * 100;

    -- Determine trend
    IF @VariancePercent > 0
        SET @TrendDirection = 'DEGRADED';
    ELSE IF @VariancePercent < 0
        SET @TrendDirection = 'IMPROVED';
    ELSE
        SET @TrendDirection = 'STABLE';

    -- Check if degraded
    IF @VariancePercent > @DegradationThresholdPercent
    BEGIN
        SET @IsDegraded = 1;
        IF @VariancePercent > 20
            SET @AlertLevel = 'CRITICAL';
        ELSE IF @VariancePercent > 15
            SET @AlertLevel = 'HIGH';
        ELSE
            SET @AlertLevel = 'MEDIUM';
    END

    -- Record trend
    INSERT INTO dbo.PerformanceTrends (
        BaselineId,
        MetricType,
        MetricName,
        CurrentValue,
        BaselineValue,
        VariancePercent,
        TrendDirection,
        IsDegraded,
        DegradationThresholdPercent,
        AlertLevel,
        MeasuredAt,
        IsDeleted
    ) VALUES (
        @BaselineId,
        @MetricType,
        @MetricName,
        @CurrentValue,
        @BaselineValue,
        @VariancePercent,
        @TrendDirection,
        @IsDegraded,
        @DegradationThresholdPercent,
        @AlertLevel,
        GETUTCDATE(),
        0
    );

    IF @IsDegraded = 1
        PRINT '⚠️ Performance degradation detected: ' + @MetricName + ' (' + CAST(@VariancePercent AS NVARCHAR(10)) + '%)';
    ELSE
        PRINT '✅ Performance stable: ' + @MetricName;
END;

GO

-- ============================================
-- PROCEDURE: Alert on Performance Degradation
-- ============================================

IF OBJECT_ID('dbo.spAlertOnDegradation', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spAlertOnDegradation;

GO

CREATE PROCEDURE dbo.spAlertOnDegradation
    @MetricType NVARCHAR(100) = NULL,
    @SeverityThreshold NVARCHAR(20) = 'MEDIUM'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DegradedCount INT = 0;
    DECLARE @CriticalCount INT = 0;
    DECLARE @HighCount INT = 0;

    PRINT '═══════════════════════════════════════════';
    PRINT 'PERFORMANCE DEGRADATION ALERT CHECK';
    PRINT '═══════════════════════════════════════════';

    -- Find degraded trends
    DECLARE DEGRADATION_CURSOR CURSOR FOR
    SELECT
        TrendId,
        MetricType,
        MetricName,
        CurrentValue,
        BaselineValue,
        VariancePercent,
        AlertLevel
    FROM dbo.PerformanceTrends
    WHERE IsDegraded = 1
    AND MeasuredAt >= DATEADD(HOUR, -1, GETUTCDATE())
    AND MetricType = COALESCE(@MetricType, MetricType)
    AND IsDeleted = 0;

    DECLARE @TrendId BIGINT, @MType NVARCHAR(100), @MName NVARCHAR(256);
    DECLARE @CVal DECIMAL(18, 4), @BVal DECIMAL(18, 4), @VPercent DECIMAL(8, 2), @ALevel NVARCHAR(20);

    OPEN DEGRADATION_CURSOR;
    FETCH NEXT FROM DEGRADATION_CURSOR INTO @TrendId, @MType, @MName, @CVal, @BVal, @VPercent, @ALevel;

    PRINT '';
    PRINT '🚨 DEGRADATION ALERTS:';

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @DegradedCount = @DegradedCount + 1;

        IF @ALevel = 'CRITICAL'
            SET @CriticalCount = @CriticalCount + 1;
        ELSE IF @ALevel = 'HIGH'
            SET @HighCount = @HighCount + 1;

        -- Create alert record
        INSERT INTO dbo.PerformanceDegradationAlerts (
            TrendId,
            AlertType,
            MetricType,
            MetricName,
            BaselineValue,
            CurrentValue,
            DegradationPercent,
            Severity,
            RootCauseAnalysis,
            RecommendedAction,
            AlertedAt,
            IsDeleted
        ) VALUES (
            @TrendId,
            'DEGRADATION',
            @MType,
            @MName,
            @BVal,
            @CVal,
            @VPercent,
            @ALevel,
            'Performance metric exceeds baseline by ' + CAST(@VPercent AS NVARCHAR(10)) + '%',
            'Review slow queries, indexes, and resource utilization',
            GETUTCDATE(),
            0
        );

        PRINT @ALevel + ': ' + @MName + ' degraded by ' + CAST(@VPercent AS NVARCHAR(10)) + '% (Baseline: ' + CAST(@BVal AS NVARCHAR(20)) + ', Current: ' + CAST(@CVal AS NVARCHAR(20)) + ')';

        FETCH NEXT FROM DEGRADATION_CURSOR INTO @TrendId, @MType, @MName, @CVal, @BVal, @VPercent, @ALevel;
    END

    CLOSE DEGRADATION_CURSOR;
    DEALLOCATE DEGRADATION_CURSOR;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'ALERT SUMMARY:';
    PRINT 'Total Degraded Metrics: ' + CAST(@DegradedCount AS NVARCHAR(10));
    PRINT 'Critical Alerts: ' + CAST(@CriticalCount AS NVARCHAR(10));
    PRINT 'High Alerts: ' + CAST(@HighCount AS NVARCHAR(10));
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Generate Performance Report
-- ============================================

IF OBJECT_ID('Report.spGeneratePerformanceReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spGeneratePerformanceReport;

GO

CREATE PROCEDURE Report.spGeneratePerformanceReport
    @DaysToAnalyze INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT 'PERFORMANCE TREND REPORT';
    PRINT 'Period: Last ' + CAST(@DaysToAnalyze AS NVARCHAR(3)) + ' days';
    PRINT '═══════════════════════════════════════════';

    -- 1. Baseline Summary
    PRINT '';
    PRINT '📊 ACTIVE BASELINES:';
    SELECT TOP 20
        MetricType,
        MetricName,
        BaselineValue AS Baseline,
        BaselineUnit AS Unit,
        SampleSize,
        BaselinedAt
    FROM dbo.PerformanceBaseline
    WHERE IsActive = 1
    AND IsDeleted = 0
    ORDER BY BaselinedAt DESC;

    -- 2. Trend Summary
    PRINT '';
    PRINT '📈 RECENT TRENDS (Last 7 measurements per metric):';
    SELECT TOP 20
        MetricName,
        CurrentValue,
        BaselineValue,
        VariancePercent,
        TrendDirection,
        AlertLevel,
        MeasuredAt
    FROM dbo.PerformanceTrends
    WHERE MeasuredAt >= @StartDate
    AND IsDeleted = 0
    ORDER BY MeasuredAt DESC;

    -- 3. Active Alerts
    PRINT '';
    PRINT '🚨 ACTIVE DEGRADATION ALERTS:';
    SELECT TOP 25
        MetricName,
        BaselineValue,
        CurrentValue,
        DegradationPercent,
        Severity,
        RecommendedAction,
        AlertedAt,
        AcknowledgedAt
    FROM dbo.PerformanceDegradationAlerts
    WHERE IsResolved = 0
    AND AlertedAt >= @StartDate
    AND IsDeleted = 0
    ORDER BY AlertedAt DESC;

    -- 4. Degradation Trends
    PRINT '';
    PRINT '📉 METRICS WITH CONSISTENT DEGRADATION (3+ measurements):';
    SELECT
        MetricName,
        COUNT(*) AS DegradedMeasurements,
        AVG(VariancePercent) AS AvgDegradationPercent,
        MAX(VariancePercent) AS MaxDegradationPercent,
        MIN(MeasuredAt) AS FirstDegraded,
        MAX(MeasuredAt) AS LastDegraded
    FROM dbo.PerformanceTrends
    WHERE IsDegraded = 1
    AND MeasuredAt >= @StartDate
    AND IsDeleted = 0
    GROUP BY MetricName
    HAVING COUNT(*) >= 3
    ORDER BY AvgDegradationPercent DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: Performance Baseline & Trending successfully configured';
PRINT 'Total objects created:';
PRINT '  - 3 performance tracking tables (PerformanceBaseline, PerformanceTrends, PerformanceDegradationAlerts)';
PRINT '  - 4 performance procedures (capture baseline, track trends, alert on degradation, generate report)';
PRINT 'Status: Performance monitoring and trend analysis framework ready';

GO
