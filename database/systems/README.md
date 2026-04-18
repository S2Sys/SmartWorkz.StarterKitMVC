# SmartWorkz Database Systems - Reusable Procedures & Infrastructure

Comprehensive, production-ready SQL Server infrastructure for multi-tenant SaaS applications. These systems are shared across multiple applications and designed for zero breaking changes across database versions.

## 📋 Contents

This folder contains **5 logical groups** of system procedures covering performance, data quality, operations, observability, and integration:

| Group | Purpose | Files | Status |
|-------|---------|-------|--------|
| **Performance & Diagnostics** | Query optimization, bottleneck detection, deadlock prevention, performance baselines | 5 files | ✅ Complete |
| **Data Quality & Validation** | Data integrity checks, anomaly detection, orphaned record discovery | 1 file | ✅ Complete |
| **Operations & Scheduling** | Automated job execution, maintenance orchestration | 1 file | ✅ Complete |
| **Observability & Dashboards** | Real-time metrics, dashboard data, system monitoring | 1 file | ✅ Complete |
| **API & Integration** | Rate limiting, external service management, health monitoring | 2 files | ✅ Complete |

## 🎯 Quick Start

### Implementation Order (Recommended)

1. **Phase 1: Foundation** → Use main v3 Phase 1 tables & procedures
2. **Phase 2: Operations** → `operations/01_JOB_SCHEDULING.sql`
3. **Phase 3: Observability** → `observability/01_DASHBOARD_METRICS.sql` + `integration/` files
4. **Phase 4: API Management** → `integration/01_RATE_LIMITING.sql` + `integration/02_EXTERNAL_SERVICES.sql`
5. **Phase 5: Performance Tuning** → `performance/` files (5 procedures)
6. **Phase 6: Data Quality** → `dataQuality/01_DATA_QUALITY_VALIDATOR.sql`

### Typical Deployment

```sql
-- Example: Deploy all systems for production
USE Boilerplate;

-- Run in order (prerequisite: Phase 1 Foundation must be deployed)
:r operations/01_JOB_SCHEDULING.sql
:r observability/01_DASHBOARD_METRICS.sql
:r integration/01_RATE_LIMITING.sql
:r integration/02_EXTERNAL_SERVICES.sql
:r performance/01_INDEX_OPTIMIZATION.sql
:r performance/02_QUERY_PERFORMANCE_MONITORING.sql
:r performance/03_BOTTLENECK_ANALYZER.sql
:r performance/04_DEADLOCK_DETECTOR.sql
:r performance/05_PERFORMANCE_BASELINE.sql
:r dataQuality/01_DATA_QUALITY_VALIDATOR.sql
```

## 📊 System Architecture

### Tables Created (by category)
- **Performance**: IndexMaintenanceLog, BottleneckAnalysisLog, DeadlockLog, BlockedProcessLog, QueryPerformanceLog, PerformanceBaseline, PerformanceTrends, PerformanceDegradationAlerts
- **Data Quality**: DataQualityLog, OrphanedRecordsLog
- **Operations**: JobSchedules, JobExecutionLogs
- **Observability**: Dashboard views (5 materialized views)
- **Integration**: RateLimitPolicies, RateLimitTracking, ExternalServices, ServiceIntegrationEvents

### Procedures Created (by category)
- **Performance**: ~25 procedures for optimization, analysis, and prevention
- **Data Quality**: 4 procedures for validation and reporting
- **Operations**: 2 procedures for scheduling
- **Observability**: 2 procedures for dashboard support
- **Integration**: 8 procedures for rate limiting and service management

## 🔍 Group Details

### 1️⃣ Performance & Diagnostics (`performance/`)

**Purpose**: Identify and eliminate performance bottlenecks, prevent deadlocks, establish performance baselines

| File | Purpose | Key Procedures |
|------|---------|-----------------|
| 01_INDEX_OPTIMIZATION.sql | Index health and maintenance | spAutoRebuildFragmentedIndexes, spIdentifyMissingIndexes, Report.spIndexHealthReport |
| 02_QUERY_PERFORMANCE_MONITORING.sql | Query execution tracking and analysis | spLogQueryPerformance, Report.spSlowQueryAnalysis, Report.spResourceUsageMonitoring |
| 03_BOTTLENECK_ANALYZER.sql | Comprehensive bottleneck detection | Report.spFindSlowStoredProcedures, Report.spAnalyzeColumnUsageStrategy, Report.spComprehensiveBottleneckAnalysis |
| 04_DEADLOCK_DETECTOR.sql | Deadlock detection and prevention | spEnableDeadlockTracking, spDetectActiveDeadlocks, Report.spAnalyzeDeadlockHistory, Report.spPreventDeadlocks |
| 05_PERFORMANCE_BASELINE.sql | Performance trends and degradation alerts | spCapturePerformanceBaseline, spTrackPerformanceTrends, spAlertOnDegradation, Report.spGeneratePerformanceReport |

**When to Use**: After Phase 1 deployment to identify optimization opportunities. Run continuously in production for real-time monitoring.

---

### 2️⃣ Data Quality & Validation (`dataQuality/`)

**Purpose**: Validate data integrity, detect anomalies, ensure constraint compliance

| File | Purpose | Key Procedures |
|------|---------|-----------------|
| 01_DATA_QUALITY_VALIDATOR.sql | Data integrity & anomaly detection | spValidateDataIntegrity, spDetectOrphanedRecords, Report.spFindDataAnomalies, Report.spGenerateDataQualityReport |

**When to Use**: Nightly data quality checks, before critical operations, after bulk imports/migrations

**Example**:
```sql
-- Weekly data quality audit
EXEC dbo.spValidateDataIntegrity @SchemaName='Master', @DryRun=1
EXEC dbo.spDetectOrphanedRecords @LogResults=1
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=30
```

---

### 3️⃣ Operations & Scheduling (`operations/`)

**Purpose**: Automate recurring maintenance, reporting, and operational tasks

| File | Purpose | Key Procedures |
|------|---------|-----------------|
| 01_JOB_SCHEDULING.sql | Job scheduling framework | spScheduleMaintenanceJob, spScheduleAnalyticsJob, spScheduleHealthCheckJob, spRunScheduledJobs |

**When to Use**: Configure once during setup, then runs automatically based on schedule

**Scheduled Jobs**:
- Nightly maintenance (cleanup, archival)
- Weekly analytics generation
- Daily system health checks

---

### 4️⃣ Observability & Dashboards (`observability/`)

**Purpose**: Real-time dashboards and monitoring views for system health and metrics

| File | Purpose | Key Procedures |
|------|---------|-----------------|
| 01_DASHBOARD_METRICS.sql | Dashboard views and summary queries | spGetDashboardSummary, spGetDashboardTile, 5 materialized views (User, Auth, Content, System, Tenant metrics) |

**When to Use**: Power dashboard UI, real-time monitoring, executive reporting

**Example Views**:
- vw_DashboardUserMetrics (active users, locked accounts, activity windows)
- vw_DashboardAuthMetrics (login success rate, failed attempts)
- vw_DashboardContentMetrics (published posts, view counts)
- vw_DashboardSystemMetrics (audit logs, notifications, feature flags)
- vw_DashboardTenantMetrics (tenant count, subscription status)

---

### 5️⃣ API & Integration (`integration/`)

**Purpose**: Manage external service integrations, enforce API rate limits, track service health

| File | Purpose | Key Procedures |
|------|---------|-----------------|
| 01_RATE_LIMITING.sql | API rate limiting by key/IP | spCheckRateLimit, spCreateRateLimitPolicy, spGetRateLimitStatus, Report.spRateLimitingReport |
| 02_EXTERNAL_SERVICES.sql | External service registry & integration tracking | spRegisterExternalService, spLogIntegrationEvent, spHealthCheckExternalService, Report.spIntegrationEventsReport |

**When to Use**: Before exposing APIs, when integrating third-party services

**Example**:
```sql
-- Register a Stripe integration
EXEC Shared.spRegisterExternalService 
    @ServiceName='Stripe', 
    @ServiceType='PaymentGateway',
    @BaseUrl='https://api.stripe.com/v1',
    @ApiKey='sk_live_...',
    @Timeout=30

-- Check health
EXEC Shared.spHealthCheckExternalService @ExternalServiceId=...
```

---

## 🔐 Prerequisites & Dependencies

### Required
- SQL Server 2016+ (Extended Events for deadlock detection)
- Phase 1 Foundation deployment (38 tables, 156 procedures, 3 views)
- Schemas: dbo, Report, Master, Shared, Auth (from Phase 1)

### Optional
- Administrative credentials (some monitoring requires sysadmin permissions)
- Agent Jobs (for automated scheduling)

## 📈 Performance Characteristics

| System | Typical Overhead | Recommended Frequency |
|--------|------------------|----------------------|
| Index Maintenance | <2% CPU | Daily (off-peak) |
| Deadlock Detection | <1% CPU | Continuous (via Extended Events) |
| Performance Baseline | <0.5% CPU | Hourly/Daily |
| Data Quality Checks | 2-5% CPU | Daily/Weekly |
| Dashboard Queries | <1% CPU | Real-time (cached 5min) |
| Rate Limiting | <1% CPU | Per-request |

## 🛡️ Security & Compliance

### Role-Based Access
- **Admin**: All procedures, full access
- **Operator**: Can run maintenance, scheduling, reports
- **Analyst**: Read-only access to reports and dashboards
- **API Consumer**: Rate limiting, external service logs only

### Audit Trail
- All operations logged in respective audit tables
- Change tracking via JSON-based before/after values
- Compliance-ready reporting procedures

## 📚 Common Use Cases

### "Database is slow" → Use Performance Group
```sql
-- 1. Find slow queries
EXEC Report.spSlowQueryAnalysis @ThresholdMs=1000, @TopCount=20

-- 2. Check index fragmentation
EXEC Report.spIndexHealthReport

-- 3. Find bottlenecks
EXEC Report.spComprehensiveBottleneckAnalysis

-- 4. Detect deadlocks
EXEC Report.spDeadlockReport
```

### "Need real-time dashboards" → Use Observability + Integration
```sql
-- Get dashboard summary
EXEC spGetDashboardSummary

-- Get specific tile
EXEC spGetDashboardTile @TileName='UserMetrics'

-- Monitor API usage
EXEC Report.spRateLimitingReport
```

### "Data might be corrupted" → Use Data Quality
```sql
-- Validate integrity
EXEC dbo.spValidateDataIntegrity @DryRun=1

-- Find orphaned records
EXEC dbo.spDetectOrphanedRecords

-- Generate quality report
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=7
```

### "External service failed" → Use Integration
```sql
-- Check health status
EXEC Shared.spHealthCheckExternalService @ExternalServiceId=...

-- Review recent events
EXEC Report.spIntegrationEventsReport @DaysToAnalyze=1

-- Disable unhealthy service
EXEC Shared.spDisableExternalService @ExternalServiceId=...
```

## 🔄 Maintenance Schedule

**Daily**:
- Run job scheduler: `EXEC dbo.spRunScheduledJobs`
- Check performance trends: `EXEC dbo.spAlertOnDegradation`
- Monitor deadlocks: `EXEC dbo.spDetectActiveDeadlocks`

**Weekly**:
- Data quality audit: `EXEC dbo.spValidateDataIntegrity`
- Index maintenance: `EXEC dbo.spAutoRebuildFragmentedIndexes`
- Performance report: `EXEC Report.spGeneratePerformanceReport`

**Monthly**:
- Comprehensive bottleneck analysis: `EXEC Report.spComprehensiveBottleneckAnalysis`
- Capacity planning: Review dashboard metrics and trend data

## 📖 Additional Documentation

Each subfolder contains:
- Detailed README.md with group-specific information
- SQL files with comprehensive inline documentation
- Procedure signatures and parameter descriptions
- Example usage and common patterns

## ✅ Deployment Checklist

- [ ] Phase 1 Foundation deployed
- [ ] All SQL scripts executed in order
- [ ] Job scheduler configured and running
- [ ] Dashboard views created and indexed
- [ ] Rate limiting policies defined
- [ ] External services registered (if applicable)
- [ ] Performance baselines captured
- [ ] Data quality baseline established
- [ ] Monitoring alerts configured
- [ ] Documentation reviewed with team

## 🚀 Next Steps

1. Review specific group READMEs for implementation details
2. Deploy systems in recommended order
3. Configure automated jobs and alerts
4. Establish performance baselines
5. Monitor dashboards daily for anomalies

---

**Version**: 1.0  
**Last Updated**: 2026-04-18  
**Database**: SQL Server 2016+  
**Schemas**: dbo, Report, Master, Shared, Auth
