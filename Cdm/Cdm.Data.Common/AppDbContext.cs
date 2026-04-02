namespace Cdm.Data.Common;

using Cdm.Common.Enums;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Main database context for Chronique des Mondes
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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

    /// <summary>
    /// Campaigns table
    /// </summary>
    public DbSet<Campaign> Campaigns { get; set; } = null!;

    /// <summary>
    /// Worlds table
    /// </summary>
    public DbSet<World> Worlds { get; set; } = null!;

    /// <summary>
    /// Characters table
    /// </summary>
    public DbSet<Character> Characters { get; set; } = null!;

    /// <summary>
    /// Character game profiles table
    /// </summary>
    public DbSet<CharacterGameProfile> CharacterGameProfiles { get; set; } = null!;

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
        });

        // Configure Campaign entity
        modelBuilder.Entity<Campaign>(entity =>
        {
            // CreatedBy index for performance on "My Campaigns" queries
            entity.HasIndex(c => c.CreatedBy)
                .HasDatabaseName("IX_Campaigns_CreatedBy");

            // GameType index for filtering by game system
            entity.HasIndex(c => c.GameType)
                .HasDatabaseName("IX_Campaigns_GameType");

            // Composite index for public campaigns listing
            entity.HasIndex(c => new { c.Visibility, c.IsActive })
                .HasDatabaseName("IX_Campaigns_Visibility_IsActive");

            // Name index for search
            entity.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Campaigns_Name");

            // User relationship (CreatedBy)
            entity.HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default values
            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);

            entity.Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(c => c.Visibility)
                .HasDefaultValue(Visibility.Private);

            entity.Property(c => c.MaxPlayers)
                .HasDefaultValue(6);

            entity.Property(c => c.GameType)
                .HasDefaultValue(GameType.Generic);
        });

        // Configure World entity
        modelBuilder.Entity<World>(entity =>
        {
            // UserId index
            entity.HasIndex(w => w.UserId)
                .HasDatabaseName("IX_Worlds_UserId");

            // GameType index
            entity.HasIndex(w => w.GameType)
                .HasDatabaseName("IX_Worlds_GameType");

            // IsActive index
            entity.HasIndex(w => w.IsActive)
                .HasDatabaseName("IX_Worlds_IsActive");

            // User relationship
            entity.HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default values
            entity.Property(w => w.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(w => w.IsActive)
                .HasDefaultValue(true);
        });

        // Configure Character entity
        modelBuilder.Entity<Character>(entity =>
        {
            // UserId index
            entity.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Characters_UserId");

            // IsActive index
            entity.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Characters_IsActive");

            // Name index for search
            entity.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Characters_Name");

            // User relationship
            entity.HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default values
            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);
        });

        // Configure CharacterGameProfile entity
        modelBuilder.Entity<CharacterGameProfile>(entity =>
        {
            // CharacterId index
            entity.HasIndex(cgp => cgp.CharacterId)
                .HasDatabaseName("IX_CharacterGameProfiles_CharacterId");

            // CampaignId index
            entity.HasIndex(cgp => cgp.CampaignId)
                .HasDatabaseName("IX_CharacterGameProfiles_CampaignId");

            // GameType index
            entity.HasIndex(cgp => cgp.GameType)
                .HasDatabaseName("IX_CharacterGameProfiles_GameType");

            // Unique constraint: one character can only join a campaign once
            entity.HasIndex(cgp => new { cgp.CharacterId, cgp.CampaignId })
                .IsUnique()
                .HasDatabaseName("IX_CharacterGameProfiles_CharacterId_CampaignId");

            // Character relationship
            entity.HasOne(cgp => cgp.Character)
                .WithMany(c => c.GameProfiles)
                .HasForeignKey(cgp => cgp.CharacterId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Campaign relationship
            entity.HasOne(cgp => cgp.Campaign)
                .WithMany()
                .HasForeignKey(cgp => cgp.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default values
            entity.Property(cgp => cgp.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(cgp => cgp.IsActive)
                .HasDefaultValue(true);
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamp
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamp
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => (e.Entity is User || e.Entity is Campaign || e.Entity is World || e.Entity is Character || e.Entity is CharacterGameProfile) 
                && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                {
                    user.CreatedAt = DateTime.UtcNow;
                }
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Campaign campaign)
            {
                if (entry.State == EntityState.Added)
                {
                    campaign.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    campaign.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is World world)
            {
                if (entry.State == EntityState.Added)
                {
                    world.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    world.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Character character)
            {
                if (entry.State == EntityState.Added)
                {
                    character.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    character.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is CharacterGameProfile profile)
            {
                if (entry.State == EntityState.Modified)
                {
                    profile.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
