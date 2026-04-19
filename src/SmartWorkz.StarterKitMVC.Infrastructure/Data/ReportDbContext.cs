using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Domain.Entities.Report;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

    // Report Schema Entities
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportData> ReportDatas { get; set; }
    public DbSet<Analytics> Analytics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("Report");

        // Report Configuration
        modelBuilder.Entity<Report>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Report>()
            .Property(r => r.Id).HasColumnName("ReportId");

        modelBuilder.Entity<Report>()
            .HasMany(r => r.ReportSchedules)
            .WithOne(rs => rs.Report)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Report>()
            .HasMany(r => r.ReportDataCollection)
            .WithOne(rd => rd.Report)
            .HasForeignKey(rd => rd.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Report>()
            .HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();

        // ReportSchedule Configuration
        modelBuilder.Entity<ReportSchedule>()
            .HasKey(rs => rs.Id);
        modelBuilder.Entity<ReportSchedule>()
            .Property(rs => rs.Id).HasColumnName("ReportScheduleId");

        modelBuilder.Entity<ReportSchedule>()
            .HasIndex(rs => new { rs.TenantId, rs.IsActive });

        modelBuilder.Entity<ReportSchedule>()
            .HasIndex(rs => new { rs.TenantId, rs.NextRun });

        // ReportData Configuration
        modelBuilder.Entity<ReportData>()
            .HasKey(rd => rd.Id);
        modelBuilder.Entity<ReportData>()
            .Property(rd => rd.Id).HasColumnName("ReportDataId");

        modelBuilder.Entity<ReportData>()
            .HasIndex(rd => new { rd.TenantId, rd.ReportId });

        modelBuilder.Entity<ReportData>()
            .HasIndex(rd => new { rd.TenantId, rd.CreatedAt });

        // Analytics Configuration
        modelBuilder.Entity<Analytics>()
            .HasKey(a => a.AnalyticsId);

        modelBuilder.Entity<Analytics>()
            .HasIndex(a => new { a.TenantId, a.EventName });

        modelBuilder.Entity<Analytics>()
            .HasIndex(a => new { a.TenantId, a.UserId });

        modelBuilder.Entity<Analytics>()
            .HasIndex(a => new { a.TenantId, a.CreatedAt });
    }
}
