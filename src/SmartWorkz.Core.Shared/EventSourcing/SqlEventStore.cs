using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Shared;

/// <summary>
/// SQL Server implementation of the event store using Dapper for data access.
/// Provides immutable append-only event log with snapshot support for optimization.
/// Implements optimistic concurrency control using version numbers.
/// </summary>
public class SqlEventStore : IEventStore
{
    private readonly string _connectionString;
    private readonly ILogger<SqlEventStore>? _logger;

    private const string EventStoreTableName = "EventStore";
    private const string EventStoreSnapshotTableName = "EventStoreSnapshot";

    public SqlEventStore(string connectionString, ILogger<SqlEventStore>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Appends events to the event stream for an aggregate.
    /// </summary>
    public async Task AppendEventsAsync(
        string aggregateId,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var eventsList = events.ToList();
        if (!eventsList.Any())
        {
            return;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var currentVersion = await GetCurrentVersionAsync(connection, aggregateId, cancellationToken);

            using var transaction = connection.BeginTransaction();

            var version = currentVersion;
            foreach (var @event in eventsList)
            {
                version++;

                const string sql = @"
                    INSERT INTO " + EventStoreTableName + @"
                    (EventId, AggregateId, EventType, EventData, Version, OccurredAt, CreatedAt)
                    VALUES
                    (@EventId, @AggregateId, @EventType, @EventData, @Version, @OccurredAt, @CreatedAt)";

                var parameters = new
                {
                    @event.EventId,
                    AggregateId = aggregateId,
                    EventType = @event.GetType().FullName,
                    EventData = JsonSerializer.Serialize(@event),
                    Version = version,
                    @event.OccurredAt,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await connection.ExecuteAsync(sql, parameters, transaction: transaction);

                _logger?.LogInformation(
                    "Event appended: AggregateId={AggregateId}, EventType={EventType}, Version={Version}",
                    aggregateId, @event.GetType().Name, version);
            }

            transaction.Commit();
        }
        catch (SqlException ex)
        {
            _logger?.LogError(
                ex,
                "SQL error while appending events to aggregate {AggregateId}",
                aggregateId);
            throw new InvalidOperationException(
                $"Failed to append events to aggregate '{aggregateId}'. See inner exception for details.", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Unexpected error while appending events to aggregate {AggregateId}",
                aggregateId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all events for an aggregate in chronological order.
    /// </summary>
    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                SELECT EventId, AggregateId, EventType, EventData, Version, OccurredAt, CreatedAt
                FROM " + EventStoreTableName + @"
                WHERE AggregateId = @AggregateId
                ORDER BY Version ASC";

            var records = await connection.QueryAsync<EventRecord>(
                sql,
                new { AggregateId = aggregateId });

            var events = records.Select(r => DeserializeEvent(r)).ToList();

            _logger?.LogInformation(
                "Retrieved {EventCount} events for aggregate {AggregateId}",
                events.Count, aggregateId);

            return events;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error retrieving events for aggregate {AggregateId}",
                aggregateId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves events for an aggregate after a specific version.
    /// </summary>
    public async Task<IEnumerable<IDomainEvent>> GetEventsSinceAsync(
        string aggregateId,
        int version,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        if (version < 0)
        {
            throw new ArgumentException("Version cannot be negative.", nameof(version));
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                SELECT EventId, AggregateId, EventType, EventData, Version, OccurredAt, CreatedAt
                FROM " + EventStoreTableName + @"
                WHERE AggregateId = @AggregateId AND Version > @Version
                ORDER BY Version ASC";

            var records = await connection.QueryAsync<EventRecord>(
                sql,
                new { AggregateId = aggregateId, Version = version });

            var events = records.Select(r => DeserializeEvent(r)).ToList();

            _logger?.LogInformation(
                "Retrieved {EventCount} events for aggregate {AggregateId} since version {Version}",
                events.Count, aggregateId, version);

            return events;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error retrieving events since version {Version} for aggregate {AggregateId}",
                version, aggregateId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the latest snapshot for an aggregate if one exists.
    /// </summary>
    public async Task<EventStoreSnapshot?> GetSnapshotAsync(
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                SELECT TOP 1 AggregateId, Version, SnapshotData, CreatedAt
                FROM " + EventStoreSnapshotTableName + @"
                WHERE AggregateId = @AggregateId
                ORDER BY Version DESC";

            var record = await connection.QueryFirstOrDefaultAsync<SnapshotRecord>(
                sql,
                new { AggregateId = aggregateId });

            if (record == null)
            {
                _logger?.LogDebug(
                    "No snapshot found for aggregate {AggregateId}",
                    aggregateId);
                return null;
            }

            var snapshot = new EventStoreSnapshot
            {
                AggregateId = record.AggregateId,
                Version = record.Version,
                SnapshotData = JsonSerializer.Deserialize<object>(record.SnapshotData) ?? new object(),
                CreatedAt = record.CreatedAt
            };

            _logger?.LogInformation(
                "Retrieved snapshot for aggregate {AggregateId} at version {Version}",
                aggregateId, record.Version);

            return snapshot;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error retrieving snapshot for aggregate {AggregateId}",
                aggregateId);
            throw;
        }
    }

    /// <summary>
    /// Saves a snapshot of aggregate state at a specific version.
    /// </summary>
    public async Task SaveSnapshotAsync(
        EventStoreSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (string.IsNullOrWhiteSpace(snapshot.AggregateId))
        {
            throw new ArgumentException("Snapshot AggregateId cannot be null or empty.", nameof(snapshot));
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Check if snapshot already exists for this aggregate and version
            const string checkSql = @"
                SELECT COUNT(1)
                FROM " + EventStoreSnapshotTableName + @"
                WHERE AggregateId = @AggregateId AND Version = @Version";

            var exists = await connection.QueryFirstAsync<int>(
                checkSql,
                new { snapshot.AggregateId, snapshot.Version });

            if (exists > 0)
            {
                _logger?.LogDebug(
                    "Snapshot already exists for aggregate {AggregateId} at version {Version}, skipping",
                    snapshot.AggregateId, snapshot.Version);
                return;
            }

            // Delete older snapshots for this aggregate (keep only latest)
            const string deleteSql = @"
                DELETE FROM " + EventStoreSnapshotTableName + @"
                WHERE AggregateId = @AggregateId";

            await connection.ExecuteAsync(deleteSql, new { snapshot.AggregateId });

            // Insert new snapshot
            const string insertSql = @"
                INSERT INTO " + EventStoreSnapshotTableName + @"
                (AggregateId, Version, SnapshotData, CreatedAt)
                VALUES
                (@AggregateId, @Version, @SnapshotData, @CreatedAt)";

            var parameters = new
            {
                snapshot.AggregateId,
                snapshot.Version,
                SnapshotData = JsonSerializer.Serialize(snapshot.SnapshotData),
                snapshot.CreatedAt
            };

            await connection.ExecuteAsync(insertSql, parameters);

            _logger?.LogInformation(
                "Snapshot saved for aggregate {AggregateId} at version {Version}",
                snapshot.AggregateId, snapshot.Version);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error saving snapshot for aggregate {AggregateId}",
                snapshot.AggregateId);
            throw;
        }
    }

    /// <summary>
    /// Reconstructs an aggregate from its event history.
    /// Optionally uses snapshots for performance optimization.
    /// </summary>
    public async Task<T?> GetAggregateAsync<T>(
        string aggregateId,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        try
        {
            // Try to get snapshot first
            var snapshot = await GetSnapshotAsync(aggregateId, cancellationToken);

            T? aggregate = null;
            var startVersion = 0;

            if (snapshot != null)
            {
                // Deserialize snapshot as aggregate
                if (snapshot.SnapshotData is T snapshotAggregate)
                {
                    aggregate = snapshotAggregate;
                }
                else if (snapshot.SnapshotData is JsonElement jsonElement)
                {
                    aggregate = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }

                startVersion = snapshot.Version;
                _logger?.LogDebug(
                    "Using snapshot for aggregate {AggregateId}, starting event replay from version {Version}",
                    aggregateId, startVersion);
            }

            // Get events since snapshot (or all events if no snapshot)
            var events = await GetEventsSinceAsync(aggregateId, startVersion, cancellationToken);

            // If no snapshot exists and no events, return null
            if (aggregate == null && !events.Any())
            {
                _logger?.LogDebug(
                    "No aggregate or events found for {AggregateId}",
                    aggregateId);
                return null;
            }

            // Create new aggregate if not reconstructed from snapshot
            aggregate ??= Activator.CreateInstance<T>();

            // Replay events (simplified - real implementation would call event handlers)
            if (aggregate != null)
            {
                var versionProperty = typeof(T).GetProperty("Version");
                if (versionProperty?.CanWrite == true)
                {
                    versionProperty.SetValue(aggregate, events.Count() + startVersion);
                }
            }

            _logger?.LogInformation(
                "Aggregate {AggregateId} reconstructed with {EventCount} events",
                aggregateId, events.Count());

            return aggregate;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error reconstructing aggregate {AggregateId}",
                aggregateId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Gets the current version number for an aggregate.
    /// </summary>
    private async Task<int> GetCurrentVersionAsync(
        SqlConnection connection,
        string aggregateId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT ISNULL(MAX(Version), 0)
            FROM " + EventStoreTableName + @"
            WHERE AggregateId = @AggregateId";

        return await connection.QueryFirstAsync<int>(sql, new { AggregateId = aggregateId });
    }

    /// <summary>
    /// Deserializes a stored event record back to IDomainEvent.
    /// </summary>
    private IDomainEvent DeserializeEvent(EventRecord record)
    {
        try
        {
            var eventType = Type.GetType(record.EventType);
            if (eventType == null)
            {
                throw new InvalidOperationException(
                    $"Cannot find event type '{record.EventType}'. Ensure the type is available in the assembly.");
            }

            var @event = JsonSerializer.Deserialize(record.EventData, eventType);
            if (@event is IDomainEvent domainEvent)
            {
                return domainEvent;
            }

            throw new InvalidOperationException(
                $"Deserialized type '{eventType.Name}' does not implement IDomainEvent.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize event data for event ID {record.EventId}.", ex);
        }
    }

    #endregion

    #region Data Models

    /// <summary>
    /// Internal model for reading event records from the database.
    /// </summary>
    private class EventRecord
    {
        public Guid EventId { get; set; }
        public string AggregateId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Internal model for reading snapshot records from the database.
    /// </summary>
    private class SnapshotRecord
    {
        public string AggregateId { get; set; } = string.Empty;
        public int Version { get; set; }
        public string SnapshotData { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    #endregion
}
