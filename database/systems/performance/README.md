# Performance & Diagnostics Systems

**Purpose**: Optimize database performance through index maintenance, query monitoring, bottleneck analysis, deadlock prevention, and performance trending.

## 📂 Files in This Group

| # | File | Purpose | Key Tables | Key Procedures |
|---|------|---------|-----------|-----------------|
| 1 | 01_INDEX_OPTIMIZATION.sql | Index fragmentation analysis and auto-remediation | IndexMaintenanceLog | spAutoRebuildFragmentedIndexes, spIdentifyMissingIndexes, spIdentifyUnusedIndexes |
| 2 | 02_QUERY_PERFORMANCE_MONITORING.sql | Query execution tracking and slow query detection | QueryPerformanceLog | spLogQueryPerformance, Report.spSlowQueryAnalysis, Report.spResourceUsageMonitoring |
| 3 | 03_BOTTLENECK_ANALYZER.sql | Comprehensive performance bottleneck detection | BottleneckAnalysisLog | Report.spFindSlowStoredProcedures, Report.spFindSlowViews, Report.spAnalyzeColumnUsageStrategy |
| 4 | 04_DEADLOCK_DETECTOR.sql | Real-time deadlock detection and prevention | DeadlockLog, BlockedProcessLog | spEnableDeadlockTracking, spDetectActiveDeadlocks, Report.spPreventDeadlocks |
| 5 | 05_PERFORMANCE_BASELINE.sql | Establish baselines and detect degradation | PerformanceBaseline, PerformanceTrends, PerformanceDegradationAlerts | spCapturePerformanceBaseline, spTrackPerformanceTrends, spAlertOnDegradation |

## 🎯 When to Deploy

**Phase**: After Phase 1 foundation and initial data load  
**Timing**: Before production go-live (establish baselines with production-like data)  
**Effort**: 2-3 hours to deploy + 1 hour to configure

## 🔧 Quick Setup

### 1. Deploy All Files (Prerequisites: Phase 1 foundation)
```sql
USE Boilerplate;
:r performance/01_INDEX_OPTIMIZATION.sql
:r performance/02_QUERY_PERFORMANCE_MONITORING.sql
:r performance/03_BOTTLENECK_ANALYZER.sql
:r performance/04_DEADLOCK_DETECTOR.sql
:r performance/05_PERFORMANCE_BASELINE.sql
```

### 2. Enable Deadlock Tracking (One-time)
```sql
EXEC dbo.spEnableDeadlockTracking;
-- Output: Extended Events session 'DeadlockMonitoring' created
```

### 3. Capture Initial Baselines
```sql
-- For query execution time (milliseconds)
EXEC dbo.spCapturePerformanceBaseline 
    @MetricType='QueryExecution',
    @MetricName='AvgResponseTime',
    @BaselineValue=50,
    @BaselineUnit='ms',
    @SampleSize=100;

-- For CPU percentage
EXEC dbo.spCapturePerformanceBaseline 
    @MetricType='System',
    @MetricName='CPUUtilization',
    @BaselineValue=45,
    @BaselineUnit='%',
    @SampleSize=50;
```

### 4. Schedule Automated Maintenance
```sql
-- Add to SQL Agent or use Phase 1C Job Scheduler
EXEC dbo.spRunIndexMaintenanceJob;  -- Run daily at 2 AM (off-peak)
```

## 📊 System Flows

### Index Optimization Flow
```
spAnalyzeIndexFragmentation (scan)
  ↓
spAutoRebuildFragmentedIndexes (>30% → rebuild, >10% → reorganize)
  ↓
spIdentifyMissingIndexes (suggest new indexes)
  ↓
Report.spIndexHealthReport (dashboard)
```

### Query Performance Flow
```
Application logs query execution → spLogQueryPerformance
  ↓
Report.spSlowQueryAnalysis (queries > 1000ms)
  ↓
Report.spResourceUsageMonitoring (CPU, memory, locks)
  ↓
Bottleneck detection & optimization
```

### Deadlock Detection Flow
```
spEnableDeadlockTracking (Extended Events setup)
  ↓
spDetectActiveDeadlocks (real-time monitoring)
  ↓
Report.spAnalyzeDeadlockHistory (forensic analysis)
  ↓
Report.spPreventDeadlocks (prevention strategies)
```

### Performance Baseline Flow
```
spCapturePerformanceBaseline (establish baseline)
  ↓
spTrackPerformanceTrends (log measurements)
  ↓
spAlertOnDegradation (detect variance > threshold)
  ↓
Report.spGeneratePerformanceReport (trending & alerts)
```

## 💡 Common Tasks

### Task 1: Find Slow Queries
```sql
-- Get slowest queries (>1000ms)
EXEC Report.spSlowQueryAnalysis @ThresholdMs=1000, @TopCount=20;

-- Result columns:
-- QueryHash, QueryPreview, ExecutionCount, AvgElapsedTimeMs, MaxElapsedTimeMs
```

### Task 2: Identify Missing Indexes
```sql
EXEC Report.spIndexHealthReport;
-- Shows fragmented indexes and missing index recommendations
```

### Task 3: Analyze Index Strategy
```sql
EXEC Report.spAnalyzeIndexStrategy;
-- Recommend when to add, remove, or modify indexes
```

### Task 4: Detect Deadlocks in Real-Time
```sql
EXEC dbo.spDetectActiveDeadlocks;
-- Shows active blocking chains and potential deadlock victims
```

### Task 5: Get Prevention Strategies
```sql
EXEC Report.spPreventDeadlocks;
-- Lists specific prevention recommendations:
-- - Add missing indexes
-- - Ensure consistent access order
-- - Reduce transaction scope
-- - Lower isolation levels where possible
```

### Task 6: Check Performance Degradation
```sql
EXEC dbo.spAlertOnDegradation @MetricType='QueryExecution';
-- Shows metrics that exceeded baseline by >10%
```

## 📈 Metrics Tracked

### Index Metrics
- Fragmentation level (%)
- Page count
- Rebuild/reorganize frequency
- Maintenance duration

### Query Metrics
- Execution count
- Average elapsed time (ms)
- Logical reads
- Physical reads
- Slow query flags

### System Metrics
- Active connections
- Memory grants (MB)
- Lock waits (ms)
- Wait types

### Performance Metrics
- Query execution time
- CPU utilization (%)
- Memory usage (MB)
- Deadlock count
- Blocking duration

## 🚨 Alert Thresholds

| Metric | Warning | Critical |
|--------|---------|----------|
| Index Fragmentation | >10% | >30% |
| Query Execution | +10% vs baseline | +20% vs baseline |
| CPU Utilization | >70% | >85% |
| Memory Usage | >80% | >95% |
| Deadlock Count | >1/hour | >5/hour |
| Blocking Duration | >5 seconds | >30 seconds |

## 🔄 Maintenance Schedule

| Frequency | Task | Procedure |
|-----------|------|-----------|
| Real-time | Deadlock detection | spDetectActiveDeadlocks |
| Hourly | Log query metrics | spLogQueryPerformance (from app) |
| Daily | Index maintenance | dbo.spRunIndexMaintenanceJob |
| Daily | Performance trending | spTrackPerformanceTrends (for current metrics) |
| Daily | Degradation alerts | spAlertOnDegradation |
| Weekly | Slow query analysis | Report.spSlowQueryAnalysis |
| Weekly | Bottleneck analysis | Report.spComprehensiveBottleneckAnalysis |
| Monthly | Performance report | Report.spGeneratePerformanceReport |

## 🎯 Performance Optimization Workflow

### Phase 1: Baseline Establishment
```sql
-- Capture metrics with production-like data
EXEC dbo.spCapturePerformanceBaseline @MetricType='QueryExecution', @MetricName='AvgResponseTime', @BaselineValue=100, @SampleSize=100;

-- Run initial diagnostics
EXEC Report.spComprehensiveBottleneckAnalysis;
EXEC Report.spIndexHealthReport;
```

### Phase 2: Identify Issues
```sql
-- Find slow queries
EXEC Report.spSlowQueryAnalysis @ThresholdMs=1000, @TopCount=20;

-- Find missing indexes
EXEC Report.spIdentifyMissingIndexes;

-- Find slow procedures
EXEC Report.spFindSlowStoredProcedures @ThresholdMs=500;
```

### Phase 3: Apply Fixes
```sql
-- Auto-remediate fragmented indexes
EXEC dbo.spAutoRebuildFragmentedIndexes;

-- Update statistics
EXEC dbo.spUpdateIndexStatistics;

-- Implement missing indexes (manual review required)
-- CREATE INDEX ...
```

### Phase 4: Monitor Improvement
```sql
-- Track new performance metrics
EXEC dbo.spTrackPerformanceTrends @MetricType='QueryExecution', @MetricName='AvgResponseTime', @CurrentValue=75;

-- Alert on degradation
EXEC dbo.spAlertOnDegradation;

-- Generate report
EXEC Report.spGeneratePerformanceReport @DaysToAnalyze=7;
```

## 📋 SQL Capabilities

### 01_INDEX_OPTIMIZATION.sql
- **spAnalyzeIndexFragmentation**: Scan all indexes, calculate fragmentation, recommend actions
- **spAutoRebuildFragmentedIndexes**: Auto-rebuild >30%, auto-reorganize >10%
- **spIdentifyMissingIndexes**: Find missing indexes with impact scores
- **spIdentifyUnusedIndexes**: Find indexes that waste space
- **spUpdateIndexStatistics**: Full FULLSCAN statistics update
- **Report.spIndexHealthReport**: Dashboard showing fragmentation + usage

### 02_QUERY_PERFORMANCE_MONITORING.sql
- **spLogQueryPerformance**: Log query metrics (execution time, reads)
- **Report.spSlowQueryAnalysis**: Find slowest queries, most frequent slow queries, highest I/O
- **Report.spResourceUsageMonitoring**: Monitor connections, memory, locks, wait stats

### 03_BOTTLENECK_ANALYZER.sql
- **Report.spFindSlowStoredProcedures**: Procedures exceeding threshold
- **Report.spFindSlowViews**: Views with missing indexes
- **Report.spFindUnusedTables**: Tables without recent data changes
- **Report.spAnalyzeColumnUsageStrategy**: Data type recommendations
- **Report.spAnalyzeIndexStrategy**: Index recommendations
- **Report.spComprehensiveBottleneckAnalysis**: All diagnostics
- **Report.spOptimizationStrategyRecommendations**: Phased implementation roadmap

### 04_DEADLOCK_DETECTOR.sql
- **spEnableDeadlockTracking**: Set up Extended Events for automatic logging
- **spDetectActiveDeadlocks**: Real-time blocking chain detection
- **Report.spAnalyzeDeadlockHistory**: Forensic analysis by tables, procedures, timeline
- **Report.spPreventDeadlocks**: Specific prevention strategies
- **Report.spDeadlockReport**: Dashboard with health scoring and trends

### 05_PERFORMANCE_BASELINE.sql
- **spCapturePerformanceBaseline**: Establish baseline for any metric
- **spTrackPerformanceTrends**: Log current measurements against baseline
- **spAlertOnDegradation**: Detect performance drops >threshold
- **Report.spGeneratePerformanceReport**: Trends, active alerts, degradation summary

## 🔗 Integration Points

- **Phase 1C Job Scheduler**: Run index maintenance jobs automatically
- **Dashboard System**: Display performance metrics on dashboards
- **Rate Limiting**: Monitor query rate limits and API usage
- **Monitoring & Backup**: Access QueryPerformanceLog data
- **Data Quality**: Validate index integrity

## 📞 Troubleshooting

### "Deadlock detection not working"
```sql
-- Check Extended Events status
SELECT * FROM sys.server_event_sessions WHERE name = 'DeadlockMonitoring';

-- Re-enable if needed
EXEC dbo.spEnableDeadlockTracking;
```

### "Index fragmentation too high"
```sql
-- Force immediate rebuild
ALTER INDEX ALL ON [Table] REBUILD;

-- Then track
EXEC Report.spIndexHealthReport;
```

### "Performance trending shows null baselines"
```sql
-- Capture baselines first
EXEC dbo.spCapturePerformanceBaseline @MetricType='QueryExecution', @MetricName='AvgResponseTime', @BaselineValue=100, @SampleSize=100;

-- Then track
EXEC dbo.spTrackPerformanceTrends @MetricType='QueryExecution', @MetricName='AvgResponseTime', @CurrentValue=110;
```

## 📚 Related Reading

- SQL Server Index Tuning & Design
- Query Execution Plans Analysis
- Deadlock Graphs Interpretation
- Extended Events Configuration
- Performance Baseline & Trending Best Practices

---

**Group**: Performance & Diagnostics  
**Files**: 5 SQL scripts  
**Tables**: 8 audit/tracking tables  
**Procedures**: 25+ optimization and reporting procedures  
**Status**: Production-ready
