namespace SmartWorkz.Mobile.Data.Database.Migrations;

/// <summary>Initial database schema migration with sync tables.</summary>
public class Migration_001_InitialSchema : DbMigration
{
    public override int Version => 1;
    public override string Description => "Create initial database schema with sync tables";

    public override async Task UpAsync(IDbConnection connection)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS SyncEntity (
    Id TEXT PRIMARY KEY,
    EntityType TEXT NOT NULL,
    Data TEXT NOT NULL,
    Version INTEGER DEFAULT 1,
    IsSynced BOOLEAN DEFAULT 0,
    SyncedAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS LocalChangeLog (
    Id TEXT PRIMARY KEY,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    ChangeType TEXT NOT NULL,
    Data TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsSynced BOOLEAN DEFAULT 0,
    SyncedAt DATETIME
);

CREATE TABLE IF NOT EXISTS SyncState (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_SyncEntity_EntityType ON SyncEntity(EntityType);
CREATE INDEX IF NOT EXISTS idx_SyncEntity_IsSynced ON SyncEntity(IsSynced);
CREATE INDEX IF NOT EXISTS idx_LocalChangeLog_EntityId ON LocalChangeLog(EntityId);
CREATE INDEX IF NOT EXISTS idx_LocalChangeLog_IsSynced ON LocalChangeLog(IsSynced);
        ";

        await connection.ExecuteAsync(sql);
    }
}
