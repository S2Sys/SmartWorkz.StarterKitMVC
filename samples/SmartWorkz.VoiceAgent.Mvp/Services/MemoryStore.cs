using System.Collections.Concurrent;
using System.Text.Json;

namespace SmartWorkz.VoiceAgent.Mvp.Services;

public record ChatTurn(string Role, string Text, DateTime At);

public class UserMemory
{
    public List<ChatTurn> Turns { get; set; } = [];
    public List<string> Facts { get; set; } = [];
}

/// <summary>
/// MVP memory: one JSON file per user under App_Data/memory.
/// Swap for an EF Core / SQL Server implementation when moving beyond the MVP.
/// </summary>
public class MemoryStore
{
    private readonly string _dir;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public MemoryStore(IWebHostEnvironment env)
    {
        _dir = Path.Combine(env.ContentRootPath, "App_Data", "memory");
        Directory.CreateDirectory(_dir);
    }

    public async Task<UserMemory> GetAsync(string userId)
    {
        var gate = Lock(userId);
        await gate.WaitAsync();
        try { return await LoadAsync(userId); }
        finally { gate.Release(); }
    }

    public Task AddTurnsAsync(string userId, params ChatTurn[] turns) =>
        UpdateAsync(userId, m => m.Turns.AddRange(turns));

    public Task AddFactsAsync(string userId, IEnumerable<string> facts) =>
        UpdateAsync(userId, m =>
        {
            foreach (var f in facts.Select(f => f.Trim().TrimStart('-', '*', ' ')).Where(f => f.Length > 0))
                if (!m.Facts.Contains(f, StringComparer.OrdinalIgnoreCase))
                    m.Facts.Add(f);
        });

    public Task ClearAsync(string userId) => UpdateAsync(userId, m => { m.Turns.Clear(); m.Facts.Clear(); });

    private async Task UpdateAsync(string userId, Action<UserMemory> change)
    {
        var gate = Lock(userId);
        await gate.WaitAsync();
        try
        {
            var memory = await LoadAsync(userId);
            change(memory);
            await File.WriteAllTextAsync(PathFor(userId), JsonSerializer.Serialize(memory, Json));
        }
        finally { gate.Release(); }
    }

    private async Task<UserMemory> LoadAsync(string userId)
    {
        var path = PathFor(userId);
        if (!File.Exists(path)) return new UserMemory();
        return JsonSerializer.Deserialize<UserMemory>(await File.ReadAllTextAsync(path)) ?? new UserMemory();
    }

    private SemaphoreSlim Lock(string userId) => _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

    // Keep only safe characters so a userId can never escape the memory folder.
    private string PathFor(string userId) =>
        Path.Combine(_dir, new string(userId.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray()) + ".json");
}
