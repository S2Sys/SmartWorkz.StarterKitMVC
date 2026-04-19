using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Domain.Entities.Transaction;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

    // Transaction Schema Entities
    public DbSet<TransactionLog> TransactionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("Transaction");

        // TransactionLog Configuration
        modelBuilder.Entity<TransactionLog>()
            .HasKey(tl => tl.Id);
        modelBuilder.Entity<TransactionLog>()
            .Property(tl => tl.Id).HasColumnName("TransactionLogId");

        modelBuilder.Entity<TransactionLog>()
            .HasIndex(tl => new { tl.TenantId, tl.TransactionType });

        modelBuilder.Entity<TransactionLog>()
            .HasIndex(tl => new { tl.TenantId, tl.Status });

        modelBuilder.Entity<TransactionLog>()
            .HasIndex(tl => new { tl.TenantId, tl.CreatedAt });

        modelBuilder.Entity<TransactionLog>()
            .HasIndex(tl => new { tl.TenantId, tl.CreatedBy });
    }
}
