namespace SmartWorkz.Mobile.Data.Database.Migrations;

/// <summary>Add push notification history table.</summary>
public class Migration_002_AddNotificationLog : DbMigration
{
    public override int Version => 2;
    public override string Description => "Add push notification history table";

    public override async Task UpAsync(IDbConnection connection)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS NotificationLog (
    Id TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Body TEXT NOT NULL,
    Data TEXT,
    ReceivedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsRead BOOLEAN DEFAULT 0,
    DeepLink TEXT
);

CREATE INDEX IF NOT EXISTS idx_NotificationLog_ReceivedAt ON NotificationLog(ReceivedAt);
CREATE INDEX IF NOT EXISTS idx_NotificationLog_IsRead ON NotificationLog(IsRead);
        ";

        await connection.ExecuteAsync(sql);
    }
}
