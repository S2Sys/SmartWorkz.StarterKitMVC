namespace SmartWorkz.Core.Tests.Fixtures;

using SmartWorkz.Shared.Guards;

/// <summary>
/// Fluent builder for creating test data with sensible defaults.
/// Supports building various domain entities for integration tests.
/// </summary>
public class TestDataBuilder
{
    private readonly DatabaseFixture _fixture;

    public TestDataBuilder(DatabaseFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>Build a test domain event.</summary>
    public DomainEventBuilder BuildDomainEvent()
        => new DomainEventBuilder(this);

    /// <summary>Build a test event store record.</summary>
    public EventStoreRecordBuilder BuildEventStoreRecord()
        => new EventStoreRecordBuilder(this);

    /// <summary>Build a test background job record.</summary>
    public BackgroundJobBuilder BuildBackgroundJob()
        => new BackgroundJobBuilder(this);

    /// <summary>Build test file metadata.</summary>
    public FileMetadataBuilder BuildFileMetadata()
        => new FileMetadataBuilder(this);

    /// <summary>Save domain event to database.</summary>
    public async Task<Guid> SaveDomainEventAsync(Guid? id = null, string? aggregateId = null,
        string? eventType = null, string? payload = null, DateTimeOffset? occurredAt = null)
    {
        id ??= Guid.NewGuid();
        aggregateId ??= Guid.NewGuid().ToString();
        eventType ??= "TestEvent";
        payload ??= "{}";
        occurredAt ??= DateTimeOffset.UtcNow;

        const string sql = @"
            INSERT INTO [DomainEvents] (Id, AggregateId, EventType, Payload, OccurredAt)
            VALUES (@Id, @AggregateId, @EventType, @Payload, @OccurredAt)
        ";

        await _fixture.ExecuteAsync(sql, new { Id = id, AggregateId = aggregateId, EventType = eventType, Payload = payload, OccurredAt = occurredAt });
        return id.Value;
    }

    /// <summary>Save event store record.</summary>
    public async Task<Guid> SaveEventStoreRecordAsync(Guid? id = null, string? aggregateId = null,
        string? aggregateType = null, string? eventType = null, string? eventData = null, int? version = null)
    {
        id ??= Guid.NewGuid();
        aggregateId ??= Guid.NewGuid().ToString();
        aggregateType ??= "TestAggregate";
        eventType ??= "TestEvent";
        eventData ??= "{}";
        version ??= 1;

        const string sql = @"
            INSERT INTO [EventStore] (Id, AggregateId, AggregateType, EventType, EventData, Version)
            VALUES (@Id, @AggregateId, @AggregateType, @EventType, @EventData, @Version)
        ";

        await _fixture.ExecuteAsync(sql, new { Id = id, AggregateId = aggregateId, AggregateType = aggregateType, EventType = eventType, EventData = eventData, Version = version });
        return id.Value;
    }

    /// <summary>Save file metadata.</summary>
    public async Task<Guid> SaveFileMetadataAsync(Guid? id = null, string? path = null,
        string? fileName = null, long? sizeBytes = null, string? contentType = null)
    {
        id ??= Guid.NewGuid();
        path ??= $"files/{Guid.NewGuid()}";
        fileName ??= "test-file.txt";
        sizeBytes ??= 1024;
        contentType ??= "text/plain";

        const string sql = @"
            INSERT INTO [FileMetadata] (Id, Path, FileName, SizeBytes, ContentType)
            VALUES (@Id, @Path, @FileName, @SizeBytes, @ContentType)
        ";

        await _fixture.ExecuteAsync(sql, new { Id = id, Path = path, FileName = fileName, SizeBytes = sizeBytes, ContentType = contentType });
        return id.Value;
    }

    public class DomainEventBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _aggregateId = Guid.NewGuid().ToString();
        private string _eventType = "TestEvent";
        private string _payload = "{}";
        private DateTimeOffset _occurredAt = DateTimeOffset.UtcNow;

        private readonly TestDataBuilder _parent;

        public DomainEventBuilder(TestDataBuilder parent)
        {
            _parent = Guard.NotNull(parent, nameof(parent));
        }

        public DomainEventBuilder WithId(Guid id) { _id = id; return this; }
        public DomainEventBuilder WithAggregateId(string aggregateId) { _aggregateId = aggregateId; return this; }
        public DomainEventBuilder WithEventType(string eventType) { _eventType = eventType; return this; }
        public DomainEventBuilder WithPayload(string payload) { _payload = payload; return this; }
        public DomainEventBuilder WithOccurredAt(DateTimeOffset occurredAt) { _occurredAt = occurredAt; return this; }

        public async Task<Guid> SaveAsync()
            => await _parent.SaveDomainEventAsync(_id, _aggregateId, _eventType, _payload, _occurredAt);
    }

    public class EventStoreRecordBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _aggregateId = Guid.NewGuid().ToString();
        private string _aggregateType = "TestAggregate";
        private string _eventType = "TestEvent";
        private string _eventData = "{}";
        private int _version = 1;

        private readonly TestDataBuilder _parent;

        public EventStoreRecordBuilder(TestDataBuilder parent)
        {
            _parent = Guard.NotNull(parent, nameof(parent));
        }

        public EventStoreRecordBuilder WithId(Guid id) { _id = id; return this; }
        public EventStoreRecordBuilder WithAggregateId(string aggregateId) { _aggregateId = aggregateId; return this; }
        public EventStoreRecordBuilder WithAggregateType(string aggregateType) { _aggregateType = aggregateType; return this; }
        public EventStoreRecordBuilder WithEventType(string eventType) { _eventType = eventType; return this; }
        public EventStoreRecordBuilder WithEventData(string eventData) { _eventData = eventData; return this; }
        public EventStoreRecordBuilder WithVersion(int version) { _version = version; return this; }

        public async Task<Guid> SaveAsync()
            => await _parent.SaveEventStoreRecordAsync(_id, _aggregateId, _aggregateType, _eventType, _eventData, _version);
    }

    public class BackgroundJobBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _type = "TestJob";
        private string _status = "Enqueued";

        private readonly TestDataBuilder _parent;

        public BackgroundJobBuilder(TestDataBuilder parent)
        {
            _parent = Guard.NotNull(parent, nameof(parent));
        }

        public BackgroundJobBuilder WithId(string id) { _id = id; return this; }
        public BackgroundJobBuilder WithType(string type) { _type = type; return this; }
        public BackgroundJobBuilder WithStatus(string status) { _status = status; return this; }

        public async Task<string> SaveAsync()
        {
            const string sql = @"
                INSERT INTO [BackgroundJobs] (Id, Type, Status)
                VALUES (@Id, @Type, @Status)
            ";
            await _parent._fixture.ExecuteAsync(sql, new { Id = _id, Type = _type, Status = _status });
            return _id;
        }
    }

    public class FileMetadataBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _path = $"files/{Guid.NewGuid()}";
        private string _fileName = "test-file.txt";
        private long _sizeBytes = 1024;
        private string _contentType = "text/plain";

        private readonly TestDataBuilder _parent;

        public FileMetadataBuilder(TestDataBuilder parent)
        {
            _parent = Guard.NotNull(parent, nameof(parent));
        }

        public FileMetadataBuilder WithId(Guid id) { _id = id; return this; }
        public FileMetadataBuilder WithPath(string path) { _path = path; return this; }
        public FileMetadataBuilder WithFileName(string fileName) { _fileName = fileName; return this; }
        public FileMetadataBuilder WithSizeBytes(long sizeBytes) { _sizeBytes = sizeBytes; return this; }
        public FileMetadataBuilder WithContentType(string contentType) { _contentType = contentType; return this; }

        public async Task<Guid> SaveAsync()
            => await _parent.SaveFileMetadataAsync(_id, _path, _fileName, _sizeBytes, _contentType);
    }
}
