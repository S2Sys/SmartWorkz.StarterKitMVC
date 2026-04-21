#if !WINDOWS
using SQLite;
#endif

namespace SmartWorkz.Core.Mobile;

#if !WINDOWS
[Table("sync_operations")]
#endif
public class SyncOperation
{
#if !WINDOWS
    [PrimaryKey, Column("id")]
#endif
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OperationType { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? PayloadJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; set; }
    public int AttemptCount { get; set; }
}
