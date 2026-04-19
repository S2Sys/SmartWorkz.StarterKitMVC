using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // Auth Schema Entities
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<TwoFactorToken> TwoFactorTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("Auth");

        // User Configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .Property(u => u.Id).HasColumnName("UserId");

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserPermissions)
            .WithOne(up => up.User)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.LoginAttempts)
            .WithOne(la => la.User)
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.TenantUsers)
            .WithOne(tu => tu.User)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.PasswordResetTokens)
            .WithOne(prt => prt.User)
            .HasForeignKey(prt => prt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.EmailVerificationTokens)
            .WithOne(evt => evt.User)
            .HasForeignKey(evt => evt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.TwoFactorTokens)
            .WithOne(tft => tft.User)
            .HasForeignKey(tft => tft.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Role Configuration
        modelBuilder.Entity<Role>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Role>()
            .Property(r => r.Id).HasColumnName("RoleId");

        modelBuilder.Entity<Role>()
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // Permission Configuration
        modelBuilder.Entity<Permission>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Permission>()
            .Property(p => p.Id).HasColumnName("PermissionId");

        modelBuilder.Entity<Permission>()
            .HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Permission>()
            .HasMany(p => p.UserPermissions)
            .WithOne(up => up.Permission)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Permission>()
            .HasIndex(p => new { p.TenantId, p.Name })
            .IsUnique();

        // UserRole Configuration
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => ur.Id);
        modelBuilder.Entity<UserRole>()
            .Property(ur => ur.Id).HasColumnName("UserRoleId");

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        // RolePermission Configuration
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => rp.Id);
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.Id).HasColumnName("RolePermissionId");

        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        // UserPermission Configuration
        modelBuilder.Entity<UserPermission>()
            .HasKey(up => up.Id);
        modelBuilder.Entity<UserPermission>()
            .Property(up => up.Id).HasColumnName("UserPermissionId");

        modelBuilder.Entity<UserPermission>()
            .HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique();

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>()
            .HasKey(rt => rt.Id);
        modelBuilder.Entity<RefreshToken>()
            .Property(rt => rt.Id).HasColumnName("RefreshTokenId");

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.TenantId, rt.Token })
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.TenantId, rt.ExpiresAt });

        // LoginAttempt Configuration
        modelBuilder.Entity<LoginAttempt>()
            .HasKey(la => la.LoginAttemptId);

        modelBuilder.Entity<LoginAttempt>()
            .HasIndex(la => new { la.TenantId, la.UserId, la.AttemptedAt });

        modelBuilder.Entity<LoginAttempt>()
            .HasIndex(la => new { la.TenantId, la.IPAddress });

        // AuditTrail Configuration
        modelBuilder.Entity<AuditTrail>()
            .HasKey(at => at.AuditTrailId);

        modelBuilder.Entity<AuditTrail>()
            .HasIndex(at => new { at.TenantId, at.UserId });

        modelBuilder.Entity<AuditTrail>()
            .HasIndex(at => new { at.TenantId, at.CreatedAt });

        modelBuilder.Entity<AuditTrail>()
            .HasIndex(at => new { at.TenantId, at.EntityType, at.EntityId });

        // TenantUser Configuration
        modelBuilder.Entity<TenantUser>()
            .HasKey(tu => tu.Id);
        modelBuilder.Entity<TenantUser>()
            .Property(tu => tu.Id).HasColumnName("TenantUserId");

        modelBuilder.Entity<TenantUser>()
            .HasIndex(tu => new { tu.TenantId, tu.UserId })
            .IsUnique();

        // PasswordResetToken Configuration
        modelBuilder.Entity<PasswordResetToken>()
            .HasKey(prt => prt.Id);
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.Id).HasColumnName("PasswordResetTokenId");

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(prt => new { prt.TenantId, prt.Token })
            .IsUnique();

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(prt => new { prt.TenantId, prt.ExpiresAt });

        // EmailVerificationToken Configuration
        modelBuilder.Entity<EmailVerificationToken>()
            .HasKey(evt => evt.Id);
        modelBuilder.Entity<EmailVerificationToken>()
            .Property(evt => evt.Id).HasColumnName("EmailVerificationTokenId");

        modelBuilder.Entity<EmailVerificationToken>()
            .HasIndex(evt => new { evt.TenantId, evt.Token })
            .IsUnique();

        modelBuilder.Entity<EmailVerificationToken>()
            .HasIndex(evt => new { evt.TenantId, evt.ExpiresAt });

        // TwoFactorToken Configuration
        modelBuilder.Entity<TwoFactorToken>()
            .HasKey(tft => tft.Id);
        modelBuilder.Entity<TwoFactorToken>()
            .Property(tft => tft.Id).HasColumnName("TwoFactorTokenId");

        modelBuilder.Entity<TwoFactorToken>()
            .HasIndex(tft => new { tft.TenantId, tft.Token })
            .IsUnique();

        modelBuilder.Entity<TwoFactorToken>()
            .HasIndex(tft => new { tft.TenantId, tft.ExpiresAt });
    }
}
