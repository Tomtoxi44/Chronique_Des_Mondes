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
    /// World characters table (characters adapted to worlds)
    /// </summary>
    public DbSet<WorldCharacter> WorldCharacters { get; set; } = null!;

    /// <summary>
    /// Chapters table
    /// </summary>
    public DbSet<Chapter> Chapters { get; set; } = null!;

    /// <summary>
    /// Events table
    /// </summary>
    public DbSet<Event> Events { get; set; } = null!;

    /// <summary>
    /// Achievements table
    /// </summary>
    public DbSet<Achievement> Achievements { get; set; } = null!;

    /// <summary>
    /// User achievements junction table
    /// </summary>
    public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

    /// <summary>
    /// Gets or sets the notifications.
    /// </summary>
    public DbSet<Notification> Notifications { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sessions.
    /// </summary>
    public DbSet<Session> Sessions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the session participants.
    /// </summary>
    public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;

    /// <summary>
    /// Gets or sets the non-player characters (NPCs).
    /// </summary>
    public DbSet<NonPlayerCharacter> NonPlayerCharacters { get; set; } = null!;

    /// <summary>
    /// Gets or sets the refresh tokens.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    /// <summary>
    /// Gets or sets the session chat messages.
    /// </summary>
    public DbSet<SessionMessage> SessionMessages { get; set; } = null!;

    /// <summary>
    /// Gets or sets the session dice rolls.
    /// </summary>
    public DbSet<SessionDiceRoll> SessionDiceRolls { get; set; } = null!;

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

            // World relationship (prevent cascade delete cycles)
            entity.HasOne(c => c.World)
                .WithMany(w => w.Campaigns)
                .HasForeignKey(c => c.WorldId)
                .OnDelete(DeleteBehavior.Restrict);

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

        // Configure WorldCharacter entity (characters adapted to worlds)
        modelBuilder.Entity<WorldCharacter>(entity =>
        {
            // CharacterId index
            entity.HasIndex(wc => wc.CharacterId)
                .HasDatabaseName("IX_WorldCharacters_CharacterId");

            // WorldId index
            entity.HasIndex(wc => wc.WorldId)
                .HasDatabaseName("IX_WorldCharacters_WorldId");

            // Unique constraint: one character can only join a world once
            entity.HasIndex(wc => new { wc.CharacterId, wc.WorldId })
                .IsUnique()
                .HasDatabaseName("IX_WorldCharacters_CharacterId_WorldId");

            // Character relationship
            entity.HasOne(wc => wc.Character)
                .WithMany(c => c.WorldCharacters)
                .HasForeignKey(wc => wc.CharacterId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // World relationship
            entity.HasOne(wc => wc.World)
                .WithMany(w => w.WorldCharacters)
                .HasForeignKey(wc => wc.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default values
            entity.Property(wc => wc.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(wc => wc.IsActive)
                .HasDefaultValue(true);
        });

        // Configure Chapter entity
        modelBuilder.Entity<Chapter>(entity =>
        {
            // CampaignId index
            entity.HasIndex(ch => ch.CampaignId)
                .HasDatabaseName("IX_Chapters_CampaignId");

            // ChapterNumber index for ordering
            entity.HasIndex(ch => new { ch.CampaignId, ch.ChapterNumber })
                .HasDatabaseName("IX_Chapters_CampaignId_ChapterNumber");

            // IsCompleted index
            entity.HasIndex(ch => ch.IsCompleted)
                .HasDatabaseName("IX_Chapters_IsCompleted");

            // Campaign relationship (prevent cascade cycles)
            entity.HasOne(ch => ch.Campaign)
                .WithMany(c => c.Chapters)
                .HasForeignKey(ch => ch.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            // Set default values
            entity.Property(ch => ch.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(ch => ch.IsActive)
                .HasDefaultValue(true);

            entity.Property(ch => ch.IsCompleted)
                .HasDefaultValue(false);
        });

        // Configure Event entity
        modelBuilder.Entity<Event>(entity =>
        {
            // Level index
            entity.HasIndex(e => e.Level)
                .HasDatabaseName("IX_Events_Level");

            // WorldId index
            entity.HasIndex(e => e.WorldId)
                .HasDatabaseName("IX_Events_WorldId");

            // CampaignId index
            entity.HasIndex(e => e.CampaignId)
                .HasDatabaseName("IX_Events_CampaignId");

            // ChapterId index
            entity.HasIndex(e => e.ChapterId)
                .HasDatabaseName("IX_Events_ChapterId");

            // IsActive index
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Events_IsActive");

            // World relationship (prevent cascade cycles)
            entity.HasOne(e => e.World)
                .WithMany(w => w.Events)
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Restrict);

            // Campaign relationship (prevent cascade cycles)
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chapter relationship (prevent cascade cycles)
            entity.HasOne(e => e.Chapter)
                .WithMany(ch => ch.Events)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Creator relationship
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Set default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsPermanent)
                .HasDefaultValue(true);
        });

        // Configure Achievement entity
        modelBuilder.Entity<Achievement>(entity =>
        {
            // Level index
            entity.HasIndex(a => a.Level)
                .HasDatabaseName("IX_Achievements_Level");

            // WorldId index
            entity.HasIndex(a => a.WorldId)
                .HasDatabaseName("IX_Achievements_WorldId");

            // CampaignId index
            entity.HasIndex(a => a.CampaignId)
                .HasDatabaseName("IX_Achievements_CampaignId");

            // ChapterId index
            entity.HasIndex(a => a.ChapterId)
                .HasDatabaseName("IX_Achievements_ChapterId");

            // Rarity index
            entity.HasIndex(a => a.Rarity)
                .HasDatabaseName("IX_Achievements_Rarity");

            // IsActive index
            entity.HasIndex(a => a.IsActive)
                .HasDatabaseName("IX_Achievements_IsActive");

            // World relationship (prevent cascade cycles)
            entity.HasOne(a => a.World)
                .WithMany(w => w.Achievements)
                .HasForeignKey(a => a.WorldId)
                .OnDelete(DeleteBehavior.Restrict);

            // Campaign relationship (prevent cascade cycles)
            entity.HasOne(a => a.Campaign)
                .WithMany(c => c.Achievements)
                .HasForeignKey(a => a.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chapter relationship (prevent cascade cycles)
            entity.HasOne(a => a.Chapter)
                .WithMany(ch => ch.Achievements)
                .HasForeignKey(a => a.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Creator relationship
            entity.HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Set default values
            entity.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(a => a.IsActive)
                .HasDefaultValue(true);

            entity.Property(a => a.IsAutomatic)
                .HasDefaultValue(false);

            entity.Property(a => a.IsSecret)
                .HasDefaultValue(false);

            entity.Property(a => a.Rarity)
                .HasDefaultValue(AchievementRarity.Common);
        });

        // Configure UserAchievement entity
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            // UserId index
            entity.HasIndex(ua => ua.UserId)
                .HasDatabaseName("IX_UserAchievements_UserId");

            // AchievementId index
            entity.HasIndex(ua => ua.AchievementId)
                .HasDatabaseName("IX_UserAchievements_AchievementId");

            // Unique constraint: a user can only unlock the same achievement once
            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique()
                .HasDatabaseName("IX_UserAchievements_UserId_AchievementId");

            // UnlockedAt index for recent achievements
            entity.HasIndex(ua => ua.UnlockedAt)
                .HasDatabaseName("IX_UserAchievements_UnlockedAt");

            // User relationship
            entity.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Achievement relationship
            entity.HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            // AwardedBy relationship
            entity.HasOne(ua => ua.AwardedByUser)
                .WithMany()
                .HasForeignKey(ua => ua.AwardedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Set default values
            entity.Property(ua => ua.UnlockedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(ua => ua.IsManuallyAwarded)
                .HasDefaultValue(false);
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(s => s.CampaignId)
                .HasDatabaseName("IX_Sessions_CampaignId");

            entity.HasIndex(s => s.Status)
                .HasDatabaseName("IX_Sessions_Status");

            entity.HasOne(s => s.Campaign)
                .WithMany()
                .HasForeignKey(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.StartedBy)
                .WithMany()
                .HasForeignKey(s => s.StartedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.CurrentChapter)
                .WithMany()
                .HasForeignKey(s => s.CurrentChapterId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            entity.Property(s => s.StartedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(s => s.Status)
                .HasDefaultValue(SessionStatus.Active);
        });

        // Configure SessionParticipant entity
        modelBuilder.Entity<SessionParticipant>(entity =>
        {
            entity.HasIndex(sp => sp.SessionId)
                .HasDatabaseName("IX_SessionParticipants_SessionId");

            entity.HasIndex(sp => sp.WorldCharacterId)
                .HasDatabaseName("IX_SessionParticipants_WorldCharacterId");

            entity.HasIndex(sp => new { sp.SessionId, sp.WorldCharacterId })
                .IsUnique()
                .HasDatabaseName("IX_SessionParticipants_SessionId_WorldCharacterId");

            entity.HasOne(sp => sp.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(sp => sp.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.WorldCharacter)
                .WithMany()
                .HasForeignKey(sp => sp.WorldCharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(sp => sp.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(sp => sp.Status)
                .HasDefaultValue(SessionParticipantStatus.Invited);
        });

        // Configure NonPlayerCharacter entity
        modelBuilder.Entity<NonPlayerCharacter>(entity =>
        {
            entity.HasIndex(npc => npc.ChapterId)
                .HasDatabaseName("IX_NonPlayerCharacters_ChapterId");

            entity.HasIndex(npc => npc.IsActive)
                .HasDatabaseName("IX_NonPlayerCharacters_IsActive");

            entity.HasOne(npc => npc.Chapter)
                .WithMany()
                .HasForeignKey(npc => npc.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(npc => npc.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(npc => npc.IsActive)
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
            .Where(e => (e.Entity is User || e.Entity is Campaign || e.Entity is World || e.Entity is Character || e.Entity is WorldCharacter || e.Entity is Chapter || e.Entity is Event || e.Entity is Achievement) 
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
            else if (entry.Entity is WorldCharacter worldCharacter)
            {
                if (entry.State == EntityState.Modified)
                {
                    worldCharacter.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Chapter chapter)
            {
                if (entry.State == EntityState.Added)
                {
                    chapter.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    chapter.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Event evt)
            {
                if (entry.State == EntityState.Added)
                {
                    evt.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    evt.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Achievement achievement)
            {
                if (entry.State == EntityState.Added)
                {
                    achievement.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    achievement.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
