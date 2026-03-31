-- ============================================
-- SmartWorkz v4 Phase 1: Report Schema
-- Date: 2026-03-31
-- 4 Tables: Reporting and Analytics
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. Reports (Report Definitions)
-- ============================================
CREATE TABLE Report.Reports (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    ReportType NVARCHAR(100) NOT NULL, -- 'Sales', 'Inventory', 'Customer', 'Financial'
    QueryDefinition NVARCHAR(MAX), -- SQL or stored procedure name
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Reports_ReportType ON Report.Reports(ReportType);
CREATE INDEX IX_Reports_TenantId ON Report.Reports(TenantId);

-- ============================================
-- 2. ReportSchedules (Scheduled Report Generation)
-- ============================================
CREATE TABLE Report.ReportSchedules (
    ReportScheduleId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    ScheduleName NVARCHAR(256) NOT NULL,
    Frequency NVARCHAR(50), -- 'Daily', 'Weekly', 'Monthly'
    NextRun DATETIME2,
    LastRun DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ReportId) REFERENCES Report.Reports(ReportId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_ReportSchedules_ReportId ON Report.ReportSchedules(ReportId);
CREATE INDEX IX_ReportSchedules_NextRun ON Report.ReportSchedules(NextRun);
CREATE INDEX IX_ReportSchedules_TenantId ON Report.ReportSchedules(TenantId);

-- ============================================
-- 3. ReportData (Generated Report Data)
-- ============================================
CREATE TABLE Report.ReportData (
    ReportDataId BIGINT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataJson NVARCHAR(MAX), -- JSON format report data
    Summary NVARCHAR(MAX), -- Summary statistics
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ReportId) REFERENCES Report.Reports(ReportId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_ReportData_ReportId ON Report.ReportData(ReportId);
CREATE INDEX IX_ReportData_GeneratedAt ON Report.ReportData(GeneratedAt);
CREATE INDEX IX_ReportData_TenantId ON Report.ReportData(TenantId);

-- ============================================
-- 4. Analytics (Event Analytics)
-- ============================================
CREATE TABLE Report.Analytics (
    AnalyticsId BIGINT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(256) NOT NULL, -- 'ProductViewed', 'AddedToCart', 'Purchased'
    EntityType NVARCHAR(100),
    EntityId INT,
    UserId NVARCHAR(256),
    EventData NVARCHAR(MAX), -- JSON event details
    EventDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Analytics_EventName ON Report.Analytics(EventName);
CREATE INDEX IX_Analytics_EntityType_EntityId ON Report.Analytics(EntityType, EntityId);
CREATE INDEX IX_Analytics_UserId ON Report.Analytics(UserId);
CREATE INDEX IX_Analytics_EventDate ON Report.Analytics(EventDate);
CREATE INDEX IX_Analytics_TenantId ON Report.Analytics(TenantId);

PRINT '✓ Report schema: 4 tables created successfully'

