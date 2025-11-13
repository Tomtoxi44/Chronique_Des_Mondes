namespace Cdm.Migrations;

using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Migration-specific DbContext
/// </summary>
public class MigrationsContext : DbContext
{
    public MigrationsContext(DbContextOptions<MigrationsContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Roles table
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// UserRoles junction table
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Email unique index
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // IsActive index for quick filtering
            entity.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            // CreatedAt index for reporting
            entity.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            // Set default values
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(u => u.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);
        });

        // Configure UserRole entity (composite key)
        modelBuilder.Entity<UserRole>(entity =>
        {
            // Composite primary key
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            // User relationship
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role relationship
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default value for AssignedAt
            entity.Property(ur => ur.AssignedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Seed global roles: Player, GameMaster, Admin
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Player", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "GameMaster", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Admin", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
