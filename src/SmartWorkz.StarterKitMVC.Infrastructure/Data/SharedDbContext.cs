using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class SharedDbContext : DbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

    // Shared Schema Entities
    public DbSet<SeoMeta> SeoMetas { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<FileStorage> FileStorages { get; set; }
    public DbSet<EmailQueue> EmailQueues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("Shared");

        // SeoMeta Configuration - Polymorphic
        modelBuilder.Entity<SeoMeta>()
            .HasKey(s => s.SeoMetaId);

        modelBuilder.Entity<SeoMeta>()
            .HasIndex(s => new { s.TenantId, s.EntityType, s.EntityId })
            .IsUnique();

        modelBuilder.Entity<SeoMeta>()
            .HasIndex(s => new { s.TenantId, s.Slug })
            .IsUnique();

        // Tag Configuration - Polymorphic
        modelBuilder.Entity<Tag>()
            .HasKey(t => t.TagId);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.TenantId, t.EntityType, t.EntityId });

        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.TenantId, t.TagName })
            .IsUnique();

        // Translation Configuration
        modelBuilder.Entity<Translation>()
            .HasKey(t => t.TranslationId);

        modelBuilder.Entity<Translation>()
            .HasIndex(t => new { t.TenantId, t.LanguageId, t.EntityType, t.EntityId })
            .IsUnique();

        // Notification Configuration
        modelBuilder.Entity<Notification>()
            .HasKey(n => n.NotificationId);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.TenantId, n.RecipientId });

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.TenantId, n.ReadAt });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>()
            .HasKey(al => al.AuditLogId);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.TenantId, al.EntityType, al.EntityId });

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.TenantId, al.ChangedBy });

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.TenantId, al.ChangedAt });

        // FileStorage Configuration
        modelBuilder.Entity<FileStorage>()
            .HasKey(fs => fs.FileStorageId);

        modelBuilder.Entity<FileStorage>()
            .HasIndex(fs => new { fs.TenantId, fs.FilePath })
            .IsUnique();

        modelBuilder.Entity<FileStorage>()
            .HasIndex(fs => new { fs.TenantId, fs.EntityType, fs.EntityId });

        // EmailQueue Configuration
        modelBuilder.Entity<EmailQueue>()
            .HasKey(eq => eq.EmailQueueId);

        modelBuilder.Entity<EmailQueue>()
            .HasIndex(eq => new { eq.TenantId, eq.Status });

        modelBuilder.Entity<EmailQueue>()
            .HasIndex(eq => new { eq.TenantId, eq.CreatedAt });
    }
}
