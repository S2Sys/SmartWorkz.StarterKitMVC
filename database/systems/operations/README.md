# Operations & Scheduling Systems

**Purpose**: Automate recurring maintenance tasks, reporting, and health checks through intelligent job scheduling.

## 📂 Files in This Group

| # | File | Purpose | Key Tables | Key Procedures |
|---|------|---------|-----------|-----------------|
| 1 | 01_JOB_SCHEDULING.sql | Centralized job scheduling framework | JobSchedules, JobExecutionLogs | spScheduleMaintenanceJob, spScheduleAnalyticsJob, spScheduleHealthCheckJob, spRunScheduledJobs, spLogJobExecution |

## 🎯 When to Deploy

**Phase**: After Phase 1 foundation, before production  
**Timing**: Early in deployment to automate maintenance  
**Effort**: 30 minutes to deploy + 1 hour to configure jobs

## 🔧 Quick Setup

### 1. Deploy
```sql
USE Boilerplate;
:r operations/01_JOB_SCHEDULING.sql
```

### 2. Create Scheduled Jobs
```sql
-- Nightly maintenance (2 AM)
EXEC dbo.spScheduleMaintenanceJob 
    @JobName='NightlyMaintenance',
    @ScheduleTime='02:00',
    @Frequency='DAILY';

-- Weekly analytics (Sunday 3 AM)
EXEC dbo.spScheduleAnalyticsJob 
    @JobName='WeeklyAnalytics',
    @ScheduleTime='03:00',
    @Frequency='WEEKLY',
    @DayOfWeek='SUNDAY';

-- Daily health check (6 AM)
EXEC dbo.spScheduleHealthCheckJob 
    @JobName='DailyHealthCheck',
    @ScheduleTime='06:00',
    @Frequency='DAILY';
```

### 3. Verify Scheduling
```sql
-- View all scheduled jobs
SELECT * FROM dbo.JobSchedules WHERE IsDeleted=0;

-- View execution history
SELECT * FROM dbo.JobExecutionLogs 
WHERE CreatedAt >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY CreatedAt DESC;
```

### 4. Run Jobs Manually (Testing)
```sql
-- Execute scheduled jobs based on current time
EXEC dbo.spRunScheduledJobs;

-- View results
SELECT TOP 20 * FROM dbo.JobExecutionLogs 
ORDER BY CreatedAt DESC;
```

## 📊 System Flows

```
SQL Agent Job or External Scheduler
  ↓
EXEC dbo.spRunScheduledJobs
  ↓
Check JobSchedules for matching time
  ↓
Execute scheduled procedures:
  - spRunMaintenanceCycle (cleanup, archival)
  - Report.spAnalyticsDashboard (metrics)
  - Report.spResourceUsageMonitoring (health)
  ↓
spLogJobExecution (record result)
  ↓
JobExecutionLogs table
```

## 💡 Common Tasks

### Task 1: Schedule Nightly Maintenance
```sql
EXEC dbo.spScheduleMaintenanceJob 
    @JobName='NightlyMaintenance',
    @ScheduleTime='02:00',
    @Frequency='DAILY';

-- This job runs:
-- - spCleanExpiredTokens
-- - spArchiveOldAuditLogs
-- - spCleanupInactiveLogins
-- - spRunIndexMaintenanceJob
```

### Task 2: Schedule Weekly Analytics
```sql
EXEC dbo.spScheduleAnalyticsJob 
    @JobName='WeeklyAnalytics',
    @ScheduleTime='03:00',
    @Frequency='WEEKLY',
    @DayOfWeek='SUNDAY';

-- This job runs:
-- - Report.spUserActivityReport
-- - Report.spTenantUsageStatistics
-- - Report.spFeatureUsageAnalytics
-- - Report.spPerformanceMetrics
```

### Task 3: Monitor Job Execution
```sql
-- Check recent job executions
SELECT TOP 20
    JobName,
    Status,
    StartTime,
    EndTime,
    DATEDIFF(SECOND, StartTime, EndTime) AS DurationSeconds,
    ErrorMessage
FROM dbo.JobExecutionLogs
ORDER BY StartTime DESC;

-- Find failed jobs
SELECT * FROM dbo.JobExecutionLogs
WHERE Status = 'FAILED'
AND CreatedAt >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY StartTime DESC;
```

### Task 4: Disable/Enable Jobs
```sql
-- Disable a job
UPDATE dbo.JobSchedules
SET IsActive = 0
WHERE JobName = 'NightlyMaintenance';

-- Enable a job
UPDATE dbo.JobSchedules
SET IsActive = 1
WHERE JobName = 'NightlyMaintenance';
```

## 📋 Built-in Job Templates

### 1. Maintenance Job
**What it does**:
- Cleans expired auth tokens
- Archives old audit logs (>90 days)
- Detects orphaned records
- Validates data consistency
- Cleans up inactive logins
- Cleans up failed login attempts
- Maintains indexes

**Schedule**: Daily, off-peak (2-4 AM)  
**Duration**: 15-60 minutes (depending on data size)  
**Benefit**: Keeps database clean and performant

### 2. Analytics Job
**What it does**:
- Generate user activity reports
- Compute tenant usage statistics
- Analyze feature usage
- Collect performance metrics

**Schedule**: Weekly (Sunday 3 AM)  
**Duration**: 5-30 minutes  
**Benefit**: Ready-made dashboards and reports

### 3. Health Check Job
**What it does**:
- Check database integrity
- Monitor resource usage
- Detect deadlocks
- Check slow queries
- Verify backup status

**Schedule**: Daily (6 AM)  
**Duration**: 10-20 minutes  
**Benefit**: Proactive issue detection

## 🔄 Maintenance Schedule

| Job | Frequency | Time | Duration | Impact |
|-----|-----------|------|----------|--------|
| Nightly Maintenance | Daily | 2:00 AM | 15-60 min | Low (off-peak) |
| Weekly Analytics | Weekly (Sun) | 3:00 AM | 5-30 min | Low (off-peak) |
| Daily Health Check | Daily | 6:00 AM | 10-20 min | Low (off-peak) |

## 🎯 Integration with SQL Agent

### Option 1: Create SQL Agent Job

```sql
-- Create job that runs spRunScheduledJobs every hour
EXEC msdb.dbo.sp_add_job
    @job_name = 'SmartWorkz_ScheduledJobs',
    @enabled = 1;

EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'SmartWorkz_ScheduledJobs',
    @step_name = 'Run Scheduled Jobs',
    @command = 'EXEC Boilerplate.dbo.spRunScheduledJobs',
    @database_name = 'Boilerplate';

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'HourlySchedule',
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_recurrence_factor = 1,
    @active_start_time = 0,
    @active_end_time = 235959;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'SmartWorkz_ScheduledJobs',
    @schedule_name = 'HourlySchedule';
```

### Option 2: Windows Task Scheduler

```batch
:: Schedule as Windows Task
:: Task: Run every hour
:: Action: sqlcmd -S ServerName -d Boilerplate -Q "EXEC dbo.spRunScheduledJobs"
```

### Option 3: External Application

```csharp
// Run via application job scheduler
using (SqlConnection conn = new SqlConnection(connectionString))
{
    SqlCommand cmd = new SqlCommand("dbo.spRunScheduledJobs", conn);
    cmd.CommandType = CommandType.StoredProcedure;
    conn.Open();
    cmd.ExecuteNonQuery();
}
```

## 📊 Job Execution Monitoring

### View All Jobs
```sql
SELECT * FROM dbo.JobSchedules 
WHERE IsDeleted=0
ORDER BY JobName;
```

### View Execution History
```sql
SELECT 
    JobName,
    Status,
    StartTime,
    EndTime,
    DATEDIFF(SECOND, StartTime, EndTime) AS DurationSeconds,
    ExecutionCount,
    ErrorMessage
FROM dbo.JobExecutionLogs
WHERE CreatedAt >= DATEADD(DAY, -30, GETUTCDATE())
ORDER BY StartTime DESC;
```

### Find Failed Jobs
```sql
SELECT 
    JobName,
    COUNT(*) AS FailureCount,
    MAX(StartTime) AS LastFailure,
    MAX(ErrorMessage) AS ErrorMessage
FROM dbo.JobExecutionLogs
WHERE Status = 'FAILED'
AND CreatedAt >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY JobName
ORDER BY FailureCount DESC;
```

### Calculate Job Durations
```sql
SELECT 
    JobName,
    AVG(DATEDIFF(SECOND, StartTime, EndTime)) AS AvgDurationSeconds,
    MAX(DATEDIFF(SECOND, StartTime, EndTime)) AS MaxDurationSeconds,
    MIN(DATEDIFF(SECOND, StartTime, EndTime)) AS MinDurationSeconds,
    COUNT(*) AS ExecutionCount
FROM dbo.JobExecutionLogs
WHERE Status = 'SUCCESS'
AND CreatedAt >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY JobName
ORDER BY AvgDurationSeconds DESC;
```

## 🚨 Troubleshooting

### "Jobs not running at scheduled time"
```sql
-- Check if jobs are enabled
SELECT * FROM dbo.JobSchedules WHERE JobName = 'YourJobName';

-- Check if SQL Agent is running (or your scheduler)
-- Verify EXEC dbo.spRunScheduledJobs is being called

-- Test manually
EXEC dbo.spRunScheduledJobs;

-- View results
SELECT TOP 10 * FROM dbo.JobExecutionLogs ORDER BY StartTime DESC;
```

### "Job taking too long"
```sql
-- Check job duration history
SELECT 
    JobName,
    StartTime,
    EndTime,
    DATEDIFF(SECOND, StartTime, EndTime) AS DurationSeconds
FROM dbo.JobExecutionLogs
WHERE JobName = 'YourJobName'
ORDER BY StartTime DESC
LIMIT 10;

-- If Nightly Maintenance is slow:
-- - Reduce data retention (archive more frequently)
-- - Disable orphan detection if not needed
-- - Move to different time window

-- If Analytics is slow:
-- - Reduce reporting scope
-- - Increase aggregation time periods
```

### "Job failed with error"
```sql
-- View error details
SELECT 
    JobName,
    Status,
    ErrorMessage,
    StartTime,
    EndTime
FROM dbo.JobExecutionLogs
WHERE Status = 'FAILED'
ORDER BY StartTime DESC;

-- Common causes:
-- - Insufficient permissions
-- - Missing dependencies
-- - Full transaction log
-- - Blocked tables/procedures
```

## 🔗 Integration Points

- **Performance**: Index maintenance scheduled automatically
- **Data Quality**: Data validation runs nightly
- **Observability**: Analytics job populates dashboards
- **Monitoring**: Health checks detect issues proactively

## 📋 Procedures

### spScheduleMaintenanceJob
Schedules nightly database maintenance

**Parameters**:
- @JobName: Unique job name
- @ScheduleTime: Time in HH:MM format
- @Frequency: DAILY, WEEKLY, MONTHLY
- @DayOfWeek: For weekly jobs (optional)
- @DayOfMonth: For monthly jobs (optional)

**Tasks Executed**:
- Clean expired tokens
- Archive old audit logs
- Detect orphaned records
- Validate data consistency
- Clean inactive logins
- Clean failed login attempts
- Run index maintenance

### spScheduleAnalyticsJob
Schedules analytics report generation

**Parameters**:
- @JobName: Unique job name
- @ScheduleTime: Time in HH:MM format
- @Frequency: DAILY, WEEKLY, MONTHLY
- @DayOfWeek: For weekly jobs (optional)

**Tasks Executed**:
- User activity reports
- Tenant usage statistics
- Feature usage analytics
- Performance metrics

### spScheduleHealthCheckJob
Schedules system health checks

**Parameters**:
- @JobName: Unique job name
- @ScheduleTime: Time in HH:MM format
- @Frequency: DAILY, WEEKLY, MONTHLY

**Tasks Executed**:
- Database integrity check
- Resource usage monitoring
- Deadlock detection
- Slow query analysis

### spRunScheduledJobs
Main job runner - call hourly

**Parameters**: None

**Logic**:
- Checks all enabled schedules
- Executes jobs matching current time
- Logs results to JobExecutionLogs

### spLogJobExecution
Records job execution result

**Parameters**:
- @JobName: Job being executed
- @Status: SUCCESS, FAILED, TIMEOUT
- @ExecutionCount: Number of items processed
- @StartTime: Execution start time
- @EndTime: Execution end time
- @ErrorMessage: Error details if failed

---

**Group**: Operations & Scheduling  
**Files**: 1 SQL script  
**Tables**: 2 scheduling and execution tables  
**Procedures**: 5 scheduling and execution procedures  
**Status**: Production-ready
