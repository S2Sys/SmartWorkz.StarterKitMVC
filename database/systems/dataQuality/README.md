# Data Quality & Validation Systems

**Purpose**: Validate data integrity, detect anomalies, identify orphaned records, and ensure compliance with constraints.

## 📂 Files in This Group

| # | File | Purpose | Key Tables | Key Procedures |
|---|------|---------|-----------|-----------------|
| 1 | 01_DATA_QUALITY_VALIDATOR.sql | Comprehensive data integrity and anomaly detection | DataQualityLog, OrphanedRecordsLog | spValidateDataIntegrity, spDetectOrphanedRecords, Report.spFindDataAnomalies, Report.spGenerateDataQualityReport |

## 🎯 When to Deploy

**Phase**: After Phase 1 foundation, before production data load  
**Timing**: Essential for data migration validation  
**Effort**: 30 minutes to deploy + 1 hour to configure

## 🔧 Quick Setup

### 1. Deploy
```sql
USE Boilerplate;
:r dataQuality/01_DATA_QUALITY_VALIDATOR.sql
```

### 2. Run Initial Validation (Dry-Run)
```sql
EXEC dbo.spValidateDataIntegrity @DryRun=1;
-- No changes made, just reports issues
```

### 3. Detect Orphaned Records
```sql
EXEC dbo.spDetectOrphanedRecords @LogResults=1;
-- Logs orphaned records to OrphanedRecordsLog table
```

### 4. Generate Quality Report
```sql
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=30;
-- Shows quality score, issues by type, critical issues
```

## 📊 System Flows

```
Data inserted/updated
  ↓
spValidateDataIntegrity (background check, optional)
  ↓
spDetectOrphanedRecords (find missing parents)
  ↓
Report.spFindDataAnomalies (find anomalies)
  ↓
Report.spGenerateDataQualityReport (quality dashboard)
```

## 💡 Common Tasks

### Task 1: Validate Data Before Migration
```sql
-- Dry-run: Check without making changes
EXEC dbo.spValidateDataIntegrity @SchemaName='Master', @DryRun=1;

-- If issues found, investigate:
SELECT * FROM dbo.DataQualityLog WHERE IsResolved=0 ORDER BY CheckedAt DESC;
```

### Task 2: Find Orphaned Records
```sql
-- Find records with missing foreign key parents
EXEC dbo.spDetectOrphanedRecords @LogResults=1;

-- View results
SELECT * FROM dbo.OrphanedRecordsLog WHERE IsDeleted=0;

-- Example: Users with missing Tenant references
-- SELECT * FROM Users WHERE TenantId NOT IN (SELECT TenantId FROM Tenants WHERE IsDeleted=0)
```

### Task 3: Detect Data Anomalies
```sql
-- Find unusual patterns, null violations, large columns
EXEC Report.spFindDataAnomalies @AnomalyType='NullViolation';

-- Examples of detected anomalies:
-- - NULL in NOT NULL columns
-- - Large string values (>1000 chars)
-- - Duplicate emails/unique values
-- - Future dates, past dates
```

### Task 4: Generate Weekly Quality Report
```sql
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=7;

-- Shows:
-- - Overall quality score (%)
-- - Issues by type
-- - Critical issues requiring attention
-- - Resolution status
```

## 📈 Quality Score Calculation

```
Quality Score = 100 - (Total Issues / Total Checks * 100)

Severity Levels:
- CRITICAL: Data integrity violation (block operations)
- HIGH: Constraint violation (fix before production)
- MEDIUM: Anomaly detected (investigate)
- LOW: Information (review for patterns)
```

## 🚨 Issue Categories

| Category | Severity | Example | Action |
|----------|----------|---------|--------|
| NULL Constraint | CRITICAL | NULL in NOT NULL column | DELETE or UPDATE |
| Foreign Key | CRITICAL | Orphaned record | DELETE or UPDATE |
| Unique Constraint | HIGH | Duplicate email | MERGE or DELETE |
| Data Type | MEDIUM | Invalid format | CONVERT or UPDATE |
| Anomaly | LOW | Unusual value | Investigate pattern |

## 🔄 Maintenance Schedule

| Frequency | Task | Procedure |
|-----------|------|-----------|
| Before Migration | Full validation | spValidateDataIntegrity @DryRun=1 |
| After Import | Orphan detection | spDetectOrphanedRecords |
| Daily | Anomaly detection | Report.spFindDataAnomalies (scheduled) |
| Weekly | Quality report | Report.spGenerateDataQualityReport |
| Monthly | Trend analysis | Compare quality scores over time |

## 📋 Procedures

### spValidateDataIntegrity
Validates:
- NOT NULL constraint compliance
- UNIQUE constraint compliance
- Data type consistency
- Soft-delete pattern compliance

**Parameters**:
- @SchemaName: Validate specific schema (NULL = all)
- @TableName: Validate specific table (NULL = all)
- @DryRun: 1=report only, 0=make corrections

**Output**:
- Console messages with findings
- Records logged to DataQualityLog

### spDetectOrphanedRecords
Scans all foreign key relationships and finds:
- Child records without valid parents
- Reference integrity violations
- Soft-delete orphans

**Parameters**:
- @SchemaName: Check specific schema (NULL = all)
- @TableName: Check specific table (NULL = all)
- @LogResults: 1=log findings, 0=report only

**Output**:
- Records logged to OrphanedRecordsLog
- Console messages with count

### Report.spFindDataAnomalies
Detects patterns like:
- NULL values in important columns
- Very large string columns
- Duplicate email addresses
- Date anomalies (future, pre-2020)

**Parameters**:
- @AnomalyType: Filter by type (NULL = all)

**Output**:
- Result sets with anomaly details
- Console report format

### Report.spGenerateDataQualityReport
Comprehensive quality dashboard showing:
- Overall quality score
- Issues by type
- Critical issues requiring attention
- Resolution status and trends

**Parameters**:
- @DaysToAnalyze: Days of history to analyze (default=30)

**Output**:
- Quality score summary
- Issues by type
- Critical issues requiring attention
- Resolution status

## 🔗 Data Quality Workflow

### 1. Pre-Production Validation
```sql
-- Validate before go-live
EXEC dbo.spValidateDataIntegrity @DryRun=1;
EXEC dbo.spDetectOrphanedRecords @LogResults=1;
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=30;
```

### 2. Post-Migration Audit
```sql
-- After bulk data import
EXEC dbo.spValidateDataIntegrity @DryRun=0;  -- Fix issues
EXEC dbo.spDetectOrphanedRecords @LogResults=1;
SELECT * FROM dbo.OrphanedRecordsLog WHERE IsDeleted=0;
```

### 3. Ongoing Monitoring
```sql
-- Scheduled nightly/weekly
EXEC Report.spFindDataAnomalies;
EXEC Report.spGenerateDataQualityReport @DaysToAnalyze=7;
```

### 4. Issue Resolution
```sql
-- Fix identified issues
DELETE FROM [Table] WHERE [PrimaryKey] IN (
    SELECT [FK] FROM OrphanedRecordsLog WHERE IsDeleted=0
);

-- Mark as resolved
UPDATE dbo.DataQualityLog 
SET IsResolved=1, ResolutionNotes='Deleted orphaned records'
WHERE DataQualityLogId=@LogId;
```

## 🎯 Quality Score Targets

| Environment | Target Score | Acceptable Threshold |
|-------------|--------------|----------------------|
| Development | 100% | >95% |
| Staging | >99% | >98% |
| Production | 100% | >99.5% |

## 📞 Troubleshooting

### "Finding many orphaned records"
```sql
-- Check foreign key relationships
SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS;

-- Review orphaned records
SELECT * FROM dbo.OrphanedRecordsLog 
WHERE IsDeleted=0 
ORDER BY ChildTableName, DetectedAt DESC;

-- Investigate root cause (soft-deleted parents?)
-- Bulk delete orphaned child records if appropriate
```

### "Data anomalies detected"
```sql
-- Review specific anomalies
SELECT * FROM dbo.DataQualityLog 
WHERE IsResolved=0 
ORDER BY Severity DESC, CheckedAt DESC;

-- Examples of corrective action:
-- - UPDATE [Table] SET [Column] = NULL WHERE [Condition]
-- - DELETE FROM [Table] WHERE [InvalidCondition]
-- - MERGE INTO [Table] USING staging...
```

### "Quality score declining"
```sql
-- Check trends
SELECT 
    CAST(100 - (COUNT(*) * 100.0 / MAX(COUNT(*)) OVER()) AS INT) AS QualityScore,
    CheckedAt
FROM dbo.DataQualityLog
GROUP BY CAST(CheckedAt AS DATE)
ORDER BY CheckedAt DESC;

-- Investigate recent issues
SELECT TOP 20 * FROM dbo.DataQualityLog
WHERE IsResolved=0
ORDER BY CheckedAt DESC;
```

## 🔐 Integration Points

- **Performance**: Validate index integrity
- **Operations**: Run as scheduled job
- **Observability**: Display quality metrics on dashboard
- **Phase 1**: Audit trail via AuditLogs table

## 📚 Common Patterns

### Validate Email Uniqueness
```sql
-- Find duplicate emails
SELECT Email, COUNT(*) as Count
FROM Master.Users
WHERE IsDeleted=0
GROUP BY Email
HAVING COUNT(*) > 1;
```

### Find Records with Missing References
```sql
-- Find Users with missing Tenants
SELECT u.UserId, u.TenantId
FROM Master.Users u
WHERE NOT EXISTS (
    SELECT 1 FROM Master.Tenants t 
    WHERE t.TenantId = u.TenantId 
    AND t.IsDeleted=0
)
AND u.IsDeleted=0;
```

### Check Date Validity
```sql
-- Find impossible dates
SELECT * FROM [Table]
WHERE CreatedAt > GETUTCDATE()
   OR CreatedAt < '2020-01-01'
   AND IsDeleted=0;
```

---

**Group**: Data Quality & Validation  
**Files**: 1 SQL script  
**Tables**: 2 audit/tracking tables  
**Procedures**: 4 validation and reporting procedures  
**Status**: Production-ready
