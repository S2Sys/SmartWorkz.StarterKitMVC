---
name: Missing Stored Procedures Found During Verification
description: Critical: 10 SPs called by application but not in migration 010
type: project
---

# Missing Stored Procedures

**Date Found:** 2026-04-03
**Severity:** CRITICAL - Application will crash at runtime if these SPs are missing
**Root Cause:** SP verification skipped SPs from 010 that weren't in migration 009

## Missing SPs (10 total)

All are called by Dapper repositories but NOT created in migrations 009 or 010:

1. **sp_DeleteContentTemplate** - Called by DapperContentTemplateRepository
2. **sp_EnqueueEmail** - Called by DapperEmailQueueRepository
3. **sp_GetContentTemplateById** - Called by DapperContentTemplateRepository
4. **sp_GetContentTemplatePlaceholders** - Called by DapperContentTemplateRepository
5. **sp_GetContentTemplateSectionsByTenant** - Called by DapperContentTemplateRepository
6. **sp_GetContentTemplatesByTenant** - Called by DapperContentTemplateRepository
7. **sp_GetPendingEmails** - Called by DapperEmailQueueRepository
8. **sp_MarkEmailFailed** - Called by DapperEmailQueueRepository
9. **sp_MarkEmailSent** - Called by DapperEmailQueueRepository
10. **sp_ReplaceContentTemplatePlaceholders** - Called by DapperContentTemplateRepository
11. **sp_UpsertContentTemplate** - Called by DapperContentTemplateRepository
12. **sp_UpsertContentTemplateSection** - Called by DapperContentTemplateRepository

## How to Verify in Future

Run verification script:
```bash
grep -r "sp_" src/ --include="*.cs" | grep -oE "sp_[A-Za-z0-9]+" | sort -u > /tmp/app_sps.txt
for sp in $(cat /tmp/app_sps.txt); do
  grep -q "$sp" database/v1/009_CreateStoredProcedures.sql database/v1/010_CreateStoredProcedures_Complete.sql || echo "MISSING: $sp"
done
```

## Prevention Strategy

**Before releasing:** Always verify that ALL SPs called by application code exist in migration scripts.

**How to Apply:**
1. Extract all `sp_` references from C# code
2. Compare against migration 009 + 010 SP definitions
3. Add any missing SPs to 010 with proper patterns
4. Document schemas and patterns used
5. Test by running migrations end-to-end

