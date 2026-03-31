-- ============================================
-- SmartWorkz v4 Phase 1: Transaction Schema
-- Date: 2026-03-31
-- 1 Table: Transaction logging
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. TransactionLog (Financial Transaction Tracking)
-- ============================================
CREATE TABLE [Transaction].TransactionLog (
    TransactionLogId BIGINT PRIMARY KEY IDENTITY(1,1),
    TransactionType NVARCHAR(50) NOT NULL, -- 'Payment', 'Refund', 'Transfer'
    EntityType NVARCHAR(100), -- 'Order', 'Invoice', 'Account'
    EntityId INT,
    Amount DECIMAL(18, 2) NOT NULL,
    CurrencyId INT,
    Description NVARCHAR(500),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Failed, Cancelled
    PaymentMethod NVARCHAR(100),
    ReferenceNumber NVARCHAR(256),
    ProcessedAt DATETIME2,
    CompletedAt DATETIME2,
    FailureReason NVARCHAR(500),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CurrencyId) REFERENCES Master.Currencies(CurrencyId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_TransactionLog_TransactionType ON [Transaction].TransactionLog(TransactionType);
CREATE INDEX IX_TransactionLog_Status ON [Transaction].TransactionLog(Status);
CREATE INDEX IX_TransactionLog_EntityType_EntityId ON [Transaction].TransactionLog(EntityType, EntityId);
CREATE INDEX IX_TransactionLog_CreatedAt ON [Transaction].TransactionLog(CreatedAt);
CREATE INDEX IX_TransactionLog_TenantId ON [Transaction].TransactionLog(TenantId);

PRINT '✓ Transaction schema: 1 table created successfully'

