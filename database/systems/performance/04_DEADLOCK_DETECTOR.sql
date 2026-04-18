-- ============================================
-- System Procedure: Deadlock Detection & Prevention
-- Purpose: Identify, analyze, and prevent deadlocks
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Deadlock Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeadlockLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.DeadlockLog (
        DeadlockId BIGINT PRIMARY KEY IDENTITY(1,1),
        DeadlockDetectedAt DATETIME2 NOT NULL,
        VictimProcedure NVARCHAR(256),
        WinnerProcedure NVARCHAR(256),
        VictimStatement NVARCHAR(MAX),
        WinnerStatement NVARCHAR(MAX),
        VictimTableName NVARCHAR(256),
        WinnerTableName NVARCHAR(256),
        VictimWaitTime INT,
        WinnerWaitTime INT,
        DeadlockChain NVARCHAR(MAX),
        IsResolved BIT NOT NULL DEFAULT 0,
        ResolutionStrategy NVARCHAR(MAX),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_DeadlockLog_Time ON dbo.DeadlockLog(DeadlockDetectedAt);
    CREATE INDEX IX_DeadlockLog_Tables ON dbo.DeadlockLog(VictimTableName, WinnerTableName);
    PRINT '✅ Created DeadlockLog table';
END

-- ============================================
-- TABLE: Blocked Process Events
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BlockedProcessLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.BlockedProcessLog (
        BlockedProcessId BIGINT PRIMARY KEY IDENTITY(1,1),
        BlockedSessionId INT NOT NULL,
        BlockingSessionId INT NOT NULL,
        BlockedProcedure NVARCHAR(256),
        BlockingProcedure NVARCHAR(256),
        BlockedTable NVARCHAR(256),
        BlockedResourcType NVARCHAR(50),
        BlockDurationSeconds INT,
        BlockedAt DATETIME2 NOT NULL,
        ResolvedAt DATETIME2,
        IsResolved BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_BlockedLog_Sessions ON dbo.BlockedProcessLog(BlockedSessionId, BlockingSessionId);
    CREATE INDEX IX_BlockedLog_Time ON dbo.BlockedProcessLog(BlockedAt);
    PRINT '✅ Created BlockedProcessLog table';
END

GO

-- ============================================
-- PROCEDURE: Enable Deadlock Tracking
-- ============================================

IF OBJECT_ID('dbo.spEnableDeadlockTracking', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spEnableDeadlockTracking;

GO

CREATE PROCEDURE dbo.spEnableDeadlockTracking
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'ENABLING DEADLOCK TRACKING';
    PRINT '═══════════════════════════════════════════';

    -- Enable trace flag 1222 for deadlock tracking
    DBCC TRACEON(1222, -1);
    PRINT '✅ Trace Flag 1222 enabled (deadlock graph to error log)';

    -- Create Extended Event Session for Deadlock Monitoring
    IF NOT EXISTS (
        SELECT 1 FROM sys.server_event_sessions
        WHERE name = 'DeadlockMonitoring'
    )
    BEGIN
        CREATE EVENT SESSION DeadlockMonitoring ON SERVER
        ADD EVENT sqlserver.xml_deadlock_report (
            ACTION (
                sqlserver.sql_text,
                sqlserver.session_id,
                sqlserver.database_id,
                sqlserver.client_app_name
            )
        )
        ADD TARGET package0.event_file (
            SET filename = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Log\DeadlockMonitoring.xel',
            max_file_size = 100
        );

        ALTER EVENT SESSION DeadlockMonitoring ON SERVER STATE = START;
        PRINT '✅ Extended Event Session "DeadlockMonitoring" created and started';
    END
    ELSE
        PRINT '⚠️ Extended Event Session "DeadlockMonitoring" already exists';

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'Deadlock tracking is now ACTIVE';
    PRINT 'Check SQL Server Error Log for deadlock details';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Detect Active Deadlocks
-- ============================================

IF OBJECT_ID('dbo.spDetectActiveDeadlocks', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spDetectActiveDeadlocks;

GO

CREATE PROCEDURE dbo.spDetectActiveDeadlocks
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeadlockFound INT = 0;

    PRINT '═══════════════════════════════════════════';
    PRINT '🔍 SEARCHING FOR ACTIVE DEADLOCKS';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    -- Check for blocking chains that indicate deadlock risk
    IF EXISTS (
        SELECT 1
        FROM sys.dm_exec_requests r1
        INNER JOIN sys.dm_exec_requests r2 ON r1.session_id = r2.blocking_session_id
        WHERE r1.blocking_session_id > 0
        OR r2.blocking_session_id > 0
    )
    BEGIN
        SET @DeadlockFound = 1;

        PRINT '⚠️ BLOCKING DETECTED - Potential Deadlock Risk:';
        PRINT '';

        SELECT
            r1.session_id AS BlockedSessionId,
            r2.session_id AS BlockingSessionId,
            r1.status AS BlockedStatus,
            r2.status AS BlockingStatus,
            r1.wait_type AS BlockedWaitType,
            r1.wait_time AS BlockedWaitTimeMs,
            OBJECT_NAME(r1.sql_handle) AS BlockedObjectName,
            OBJECT_NAME(r2.sql_handle) AS BlockingObjectName,
            r1.open_transaction_count AS BlockedOpenTrans,
            r2.open_transaction_count AS BlockingOpenTrans,
            DATEDIFF(SECOND, s1.login_time, GETUTCDATE()) AS BlockedSessionAgeSeconds,
            s1.program_name AS BlockedApp,
            s2.program_name AS BlockingApp
        FROM sys.dm_exec_requests r1
        INNER JOIN sys.dm_exec_requests r2 ON r1.blocking_session_id = r2.session_id
        INNER JOIN sys.dm_exec_sessions s1 ON r1.session_id = s1.session_id
        INNER JOIN sys.dm_exec_sessions s2 ON r2.session_id = s2.session_id
        WHERE r1.blocking_session_id > 0;

        PRINT '';
        PRINT '💡 REMEDIATION OPTIONS:';
        PRINT '   1. KILL <blocking_session_id> - Force terminate blocking session';
        PRINT '   2. Find blocking query - Review execution plan';
        PRINT '   3. Add indexes - Improve query performance';
        PRINT '   4. Shorten transactions - Reduce lock hold time';
    END
    ELSE
        PRINT '✅ No active deadlocks or blocking detected';

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Analyze Deadlock History
-- ============================================

IF OBJECT_ID('Report.spAnalyzeDeadlockHistory', 'P') IS NOT NULL
    DROP PROCEDURE Report.spAnalyzeDeadlockHistory;

GO

CREATE PROCEDURE Report.spAnalyzeDeadlockHistory
    @DaysToAnalyze INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT '📊 DEADLOCK HISTORY ANALYSIS';
    PRINT 'Period: Last ' + CAST(@DaysToAnalyze AS NVARCHAR(3)) + ' days';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    -- 1. Overall deadlock statistics
    PRINT '📈 DEADLOCK STATISTICS:';
    SELECT
        COUNT(*) AS TotalDeadlocks,
        COUNT(DISTINCT VictimProcedure) AS UniqueVictims,
        COUNT(DISTINCT WinnerProcedure) AS UniqueWinners,
        COUNT(DISTINCT VictimTableName) AS TablesInvolved,
        AVG(VictimWaitTime) AS AvgVictimWaitMs,
        MAX(VictimWaitTime) AS MaxVictimWaitMs,
        MAX(DeadlockDetectedAt) AS LastDeadlock
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate
    AND IsResolved = 0;

    -- 2. Most problematic tables
    PRINT '';
    PRINT '🔴 TABLES INVOLVED IN DEADLOCKS:';
    SELECT TOP 10
        COALESCE(VictimTableName, WinnerTableName) AS TableName,
        COUNT(*) AS DeadlockCount,
        COUNT(DISTINCT VictimProcedure) AS VictimProcs,
        COUNT(DISTINCT WinnerProcedure) AS WinnerProcs
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate
    AND (VictimTableName IS NOT NULL OR WinnerTableName IS NOT NULL)
    GROUP BY COALESCE(VictimTableName, WinnerTableName)
    ORDER BY DeadlockCount DESC;

    -- 3. Most frequent procedures
    PRINT '';
    PRINT '⚙️ PROCEDURES IN DEADLOCK CHAINS:';
    SELECT TOP 10
        ProcName,
        COUNT(*) AS DeadlockCount,
        'Victim' AS Role
    FROM (
        SELECT VictimProcedure AS ProcName FROM dbo.DeadlockLog
        WHERE VictimProcedure IS NOT NULL
        AND DeadlockDetectedAt >= @StartDate
    ) d
    GROUP BY ProcName
    UNION ALL
    SELECT TOP 10
        ProcName,
        COUNT(*) AS DeadlockCount,
        'Winner' AS Role
    FROM (
        SELECT WinnerProcedure AS ProcName FROM dbo.DeadlockLog
        WHERE WinnerProcedure IS NOT NULL
        AND DeadlockDetectedAt >= @StartDate
    ) d
    GROUP BY ProcName
    ORDER BY DeadlockCount DESC;

    -- 4. Deadlock timeline
    PRINT '';
    PRINT '📅 DEADLOCK TIMELINE (by hour):';
    SELECT
        DATEPART(DAY, DeadlockDetectedAt) AS Day,
        DATEPART(HOUR, DeadlockDetectedAt) AS Hour,
        COUNT(*) AS DeadlockCount,
        STRING_AGG(VictimProcedure, ', ') AS AffectedProcs
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate
    GROUP BY DATEPART(DAY, DeadlockDetectedAt), DATEPART(HOUR, DeadlockDetectedAt)
    ORDER BY Day DESC, Hour DESC;

    -- 5. Blocking patterns
    PRINT '';
    PRINT '🔗 BLOCKING CHAIN PATTERNS:';
    SELECT TOP 10
        BlockedProcedure,
        BlockingProcedure,
        COUNT(*) AS OccurrenceCount,
        AVG(BlockDurationSeconds) AS AvgBlockDurationSecs,
        MAX(BlockDurationSeconds) AS MaxBlockDurationSecs
    FROM dbo.BlockedProcessLog
    WHERE BlockedAt >= @StartDate
    AND IsResolved = 0
    GROUP BY BlockedProcedure, BlockingProcedure
    ORDER BY OccurrenceCount DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Prevent Deadlocks (Strategies)
-- ============================================

IF OBJECT_ID('Report.spPreventDeadlocks', 'P') IS NOT NULL
    DROP PROCEDURE Report.spPreventDeadlocks;

GO

CREATE PROCEDURE Report.spPreventDeadlocks
    @DaysToAnalyze INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '🛡️ DEADLOCK PREVENTION STRATEGIES';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '';

    -- Identify problem procedures
    DECLARE @ProblematicProc NVARCHAR(256);
    DECLARE @ProblematicTable NVARCHAR(256);
    DECLARE @DeadlockCount INT;

    PRINT '🎯 RECOMMENDED FIXES (Priority Order):';
    PRINT '';

    PRINT '1️⃣ ADD MISSING INDEXES:';
    PRINT '   Deadlocks often occur due to table scans';
    PRINT '   Run: EXEC dbo.spIdentifyMissingIndexes @MinImpact = 10';
    PRINT '';

    PRINT '2️⃣ ACCESS ORDER CONSISTENCY:';
    PRINT '   Ensure transactions access tables in same order';
    PRINT '   Problem: Proc A: Users → Orders → Products';
    PRINT '           Proc B: Products → Orders → Users';
    PRINT '   Fix: Both should access in same order';
    PRINT '';

    PRINT '3️⃣ REDUCE TRANSACTION SCOPE:';
    PRINT '   Keep transactions as short as possible';
    PRINT '   Move read operations outside transaction';
    PRINT '   Example: Read config BEFORE transaction starts';
    PRINT '';

    PRINT '4️⃣ USE APPROPRIATE ISOLATION LEVEL:';
    PRINT '   READ_COMMITTED_SNAPSHOT - Better for high concurrency';
    PRINT '   Example: SET TRANSACTION ISOLATION LEVEL READ_COMMITTED;';
    PRINT '';

    PRINT '5️⃣ ADD TABLE HINTS:';
    PRINT '   Use WITH (NOLOCK) for read-only operations';
    PRINT '   Use WITH (UPDLOCK) instead of XLOCK when possible';
    PRINT '   Example: SELECT * FROM Users WITH (NOLOCK) WHERE ...';
    PRINT '';

    PRINT '6️⃣ BATCH PROCESSING:';
    PRINT '   Break large operations into smaller batches';
    PRINT '   Release locks more frequently';
    PRINT '   Reduces hold time per transaction';
    PRINT '';

    -- Get specific recommendations based on deadlock history
    SELECT TOP 5
        COALESCE(VictimTableName, WinnerTableName) AS TableName,
        COUNT(*) AS DeadlockCount,
        'Add indexes on ' + COALESCE(VictimTableName, WinnerTableName) AS Recommendation
    INTO #ProblematicTables
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate
    AND (VictimTableName IS NOT NULL OR WinnerTableName IS NOT NULL)
    GROUP BY COALESCE(VictimTableName, WinnerTableName);

    PRINT '📋 SPECIFIC RECOMMENDATIONS FOR YOUR DATABASE:';
    PRINT '';

    IF EXISTS (SELECT 1 FROM #ProblematicTables)
    BEGIN
        SELECT
            TableName,
            DeadlockCount,
            'Priority: ' + CASE WHEN DeadlockCount > 5 THEN 'CRITICAL'
                               WHEN DeadlockCount > 2 THEN 'HIGH'
                               ELSE 'MEDIUM' END AS Priority
        FROM #ProblematicTables
        ORDER BY DeadlockCount DESC;
    END
    ELSE
        PRINT '✅ No recent deadlocks - System performing well!';

    DROP TABLE IF EXISTS #ProblematicTables;

    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '💡 IMPLEMENTATION PRIORITY:';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '';
    PRINT 'TODAY:      Review deadlock reports, identify problem procedures';
    PRINT 'THIS WEEK:  Add missing indexes, review query access order';
    PRINT 'THIS MONTH: Refactor to use consistent transaction order';
    PRINT 'ONGOING:    Monitor with spDetectActiveDeadlocks';
    PRINT '';

END;

GO

-- ============================================
-- PROCEDURE: Deadlock Report & Dashboard
-- ============================================

IF OBJECT_ID('Report.spDeadlockReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spDeadlockReport;

GO

CREATE PROCEDURE Report.spDeadlockReport
    @DaysToAnalyze INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '╔════════════════════════════════════════════════════════════════╗';
    PRINT '║            DEADLOCK ANALYSIS & TRENDING REPORT                 ║';
    PRINT '║                    Last ' + CAST(@DaysToAnalyze AS NVARCHAR(2)) + ' Days                            ║';
    PRINT '╚════════════════════════════════════════════════════════════════╝';
    PRINT '';

    -- Summary
    PRINT '📊 EXECUTIVE SUMMARY:';
    PRINT '';
    SELECT
        COUNT(*) AS TotalDeadlocks,
        COUNT(CASE WHEN IsResolved = 1 THEN 1 END) AS ResolvedDeadlocks,
        CAST(COUNT(CASE WHEN IsResolved = 1 THEN 1 END) * 100.0 /
             NULLIF(COUNT(*), 0) AS DECIMAL(5, 2)) AS ResolutionRate,
        MAX(DeadlockDetectedAt) AS MostRecentDeadlock,
        MIN(DeadlockDetectedAt) AS OldestDeadlock
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate;

    -- Trend analysis
    PRINT '';
    PRINT '📈 WEEKLY TREND:';
    PRINT '';
    SELECT
        DATEPART(WEEK, DeadlockDetectedAt) AS Week,
        DATEPART(YEAR, DeadlockDetectedAt) AS Year,
        COUNT(*) AS DeadlockCount,
        CASE
            WHEN COUNT(*) = 0 THEN '✅ HEALTHY'
            WHEN COUNT(*) <= 2 THEN '🟡 LOW'
            WHEN COUNT(*) <= 5 THEN '🟠 MEDIUM'
            ELSE '🔴 HIGH'
        END AS HealthStatus
    FROM dbo.DeadlockLog
    WHERE DeadlockDetectedAt >= @StartDate
    GROUP BY DATEPART(WEEK, DeadlockDetectedAt), DATEPART(YEAR, DeadlockDetectedAt)
    ORDER BY Year DESC, Week DESC;

    -- Health score
    PRINT '';
    PRINT '🏥 SYSTEM HEALTH SCORE:';
    DECLARE @DeadlockCount INT = (SELECT COUNT(*) FROM dbo.DeadlockLog WHERE DeadlockDetectedAt >= @StartDate);
    DECLARE @HealthScore INT = CASE
        WHEN @DeadlockCount = 0 THEN 100
        WHEN @DeadlockCount <= 2 THEN 90
        WHEN @DeadlockCount <= 5 THEN 75
        WHEN @DeadlockCount <= 10 THEN 50
        ELSE 25
    END;

    SELECT
        @HealthScore AS HealthScore,
        CASE
            WHEN @HealthScore >= 90 THEN 'EXCELLENT - No issues'
            WHEN @HealthScore >= 75 THEN 'GOOD - Minor issues'
            WHEN @HealthScore >= 50 THEN 'FAIR - Attention needed'
            ELSE 'POOR - Immediate action required'
        END AS Status;

    -- Recommendations
    PRINT '';
    PRINT '🎯 ACTION ITEMS:';
    IF @DeadlockCount > 10
        PRINT '🔴 CRITICAL: More than 10 deadlocks in ' + CAST(@DaysToAnalyze AS NVARCHAR(2)) + ' days - IMMEDIATE ACTION REQUIRED';
    ELSE IF @DeadlockCount > 5
        PRINT '🟠 HIGH: ' + CAST(@DeadlockCount AS NVARCHAR(3)) + ' deadlocks - Review problematic procedures';
    ELSE IF @DeadlockCount > 0
        PRINT '🟡 MEDIUM: ' + CAST(@DeadlockCount AS NVARCHAR(3)) + ' deadlocks - Monitor and optimize';
    ELSE
        PRINT '✅ GOOD: No deadlocks detected - Keep up the good work!';

    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════';

END;

GO

PRINT '✅ Deadlock Detection & Prevention System Created';
PRINT '';
PRINT 'Total Objects Created:';
PRINT '  - 2 logging tables (DeadlockLog, BlockedProcessLog)';
PRINT '  - 4 diagnostic procedures:';
PRINT '    • spEnableDeadlockTracking (setup XEvents)';
PRINT '    • spDetectActiveDeadlocks (real-time)';
PRINT '    • Report.spAnalyzeDeadlockHistory (forensics)';
PRINT '    • Report.spPreventDeadlocks (strategies)';
PRINT '    • Report.spDeadlockReport (dashboard)';
PRINT '';
PRINT 'Features:';
PRINT '  ✅ Real-time deadlock detection';
PRINT '  ✅ Extended events for automatic logging';
PRINT '  ✅ Historical analysis & trending';
PRINT '  ✅ Root cause identification';
PRINT '  ✅ Prevention strategies';
PRINT '  ✅ Health scoring';
PRINT '  ✅ Blocking chain analysis';
PRINT '';
PRINT 'Status: Production-ready deadlock management system';

GO
