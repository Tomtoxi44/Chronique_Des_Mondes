namespace Cdm.Migrations;

using Cdm.Common.Enums;
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
    /// Non-player characters table
    /// </summary>
    public DbSet<NonPlayerCharacter> NonPlayerCharacters { get; set; } = null!;

    /// <summary>
    /// Sessions table
    /// </summary>
    public DbSet<Session> Sessions { get; set; } = null!;

    /// <summary>
    /// Session participants table
    /// </summary>
    public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;

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
            entity.HasIndex(w => w.UserId).HasDatabaseName("IX_Worlds_UserId");
            entity.HasIndex(w => w.GameType).HasDatabaseName("IX_Worlds_GameType");
            entity.HasIndex(w => w.IsActive).HasDatabaseName("IX_Worlds_IsActive");

            entity.HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(w => w.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(w => w.IsActive).HasDefaultValue(true);
        });

        // Configure Character entity
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasIndex(c => c.UserId).HasDatabaseName("IX_Characters_UserId");
            entity.HasIndex(c => c.IsActive).HasDatabaseName("IX_Characters_IsActive");
            entity.HasIndex(c => c.IsLocked).HasDatabaseName("IX_Characters_IsLocked");
            entity.HasIndex(c => c.Name).HasDatabaseName("IX_Characters_Name");

            entity.HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.IsLocked).HasDefaultValue(false);
        });

        // Configure WorldCharacter entity (characters adapted to worlds)
        modelBuilder.Entity<WorldCharacter>(entity =>
        {
            entity.HasIndex(wc => wc.CharacterId).HasDatabaseName("IX_WorldCharacters_CharacterId");
            entity.HasIndex(wc => wc.WorldId).HasDatabaseName("IX_WorldCharacters_WorldId");
            entity.HasIndex(wc => new { wc.CharacterId, wc.WorldId })
                .IsUnique()
                .HasDatabaseName("IX_WorldCharacters_CharacterId_WorldId");

            entity.HasOne(wc => wc.Character)
                .WithMany(c => c.WorldCharacters)
                .HasForeignKey(wc => wc.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(wc => wc.World)
                .WithMany(w => w.WorldCharacters)
                .HasForeignKey(wc => wc.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(wc => wc.JoinedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(wc => wc.IsActive).HasDefaultValue(true);
        });

        // Configure Chapter entity
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasIndex(ch => ch.CampaignId).HasDatabaseName("IX_Chapters_CampaignId");
            entity.HasIndex(ch => new { ch.CampaignId, ch.ChapterNumber })
                .HasDatabaseName("IX_Chapters_CampaignId_ChapterNumber");
            entity.HasIndex(ch => ch.IsCompleted).HasDatabaseName("IX_Chapters_IsCompleted");

            entity.HasOne(ch => ch.Campaign)
                .WithMany(c => c.Chapters)
                .HasForeignKey(ch => ch.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ch => ch.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(ch => ch.IsActive).HasDefaultValue(true);
            entity.Property(ch => ch.IsCompleted).HasDefaultValue(false);
        });

        // Configure Event entity
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => e.Level).HasDatabaseName("IX_Events_Level");
            entity.HasIndex(e => e.WorldId).HasDatabaseName("IX_Events_WorldId");
            entity.HasIndex(e => e.CampaignId).HasDatabaseName("IX_Events_CampaignId");
            entity.HasIndex(e => e.ChapterId).HasDatabaseName("IX_Events_ChapterId");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Events_IsActive");

            entity.HasOne(e => e.World)
                .WithMany(w => w.Events)
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Chapter)
                .WithMany(ch => ch.Events)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPermanent).HasDefaultValue(true);
        });

        // Configure Achievement entity
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasIndex(a => a.Level).HasDatabaseName("IX_Achievements_Level");
            entity.HasIndex(a => a.WorldId).HasDatabaseName("IX_Achievements_WorldId");
            entity.HasIndex(a => a.CampaignId).HasDatabaseName("IX_Achievements_CampaignId");
            entity.HasIndex(a => a.ChapterId).HasDatabaseName("IX_Achievements_ChapterId");
            entity.HasIndex(a => a.Rarity).HasDatabaseName("IX_Achievements_Rarity");
            entity.HasIndex(a => a.IsActive).HasDatabaseName("IX_Achievements_IsActive");

            entity.HasOne(a => a.World)
                .WithMany(w => w.Achievements)
                .HasForeignKey(a => a.WorldId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Campaign)
                .WithMany(c => c.Achievements)
                .HasForeignKey(a => a.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Chapter)
                .WithMany(ch => ch.Achievements)
                .HasForeignKey(a => a.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(a => a.IsActive).HasDefaultValue(true);
            entity.Property(a => a.IsAutomatic).HasDefaultValue(false);
            entity.Property(a => a.IsSecret).HasDefaultValue(false);
            entity.Property(a => a.Rarity).HasDefaultValue(AchievementRarity.Common);
        });

        // Configure UserAchievement entity
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasIndex(ua => ua.UserId).HasDatabaseName("IX_UserAchievements_UserId");
            entity.HasIndex(ua => ua.AchievementId).HasDatabaseName("IX_UserAchievements_AchievementId");
            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique()
                .HasDatabaseName("IX_UserAchievements_UserId_AchievementId");
            entity.HasIndex(ua => ua.UnlockedAt).HasDatabaseName("IX_UserAchievements_UnlockedAt");

            entity.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ua => ua.AwardedByUser)
                .WithMany()
                .HasForeignKey(ua => ua.AwardedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ua => ua.UnlockedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(ua => ua.IsManuallyAwarded).HasDefaultValue(false);
        });

        // Configure NonPlayerCharacter entity
        modelBuilder.Entity<NonPlayerCharacter>(entity =>
        {
            entity.HasIndex(npc => npc.ChapterId).HasDatabaseName("IX_NonPlayerCharacters_ChapterId");
            entity.HasIndex(npc => npc.IsActive).HasDatabaseName("IX_NonPlayerCharacters_IsActive");

            entity.HasOne(npc => npc.Chapter)
                .WithMany()
                .HasForeignKey(npc => npc.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(npc => npc.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(npc => npc.IsActive).HasDefaultValue(true);
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(s => s.CampaignId).HasDatabaseName("IX_Sessions_CampaignId");
            entity.HasIndex(s => s.StartedById).HasDatabaseName("IX_Sessions_StartedById");
            entity.HasIndex(s => s.Status).HasDatabaseName("IX_Sessions_Status");

            entity.HasOne(s => s.Campaign)
                .WithMany()
                .HasForeignKey(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.StartedBy)
                .WithMany()
                .HasForeignKey(s => s.StartedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.CurrentChapter)
                .WithMany()
                .HasForeignKey(s => s.CurrentChapterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(s => s.Status).HasDefaultValue(SessionStatus.Active);
        });

        // Configure SessionParticipant entity
        modelBuilder.Entity<SessionParticipant>(entity =>
        {
            entity.HasIndex(p => p.SessionId).HasDatabaseName("IX_SessionParticipants_SessionId");
            entity.HasIndex(p => p.WorldCharacterId).HasDatabaseName("IX_SessionParticipants_WorldCharacterId");

            entity.HasOne(p => p.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.WorldCharacter)
                .WithMany()
                .HasForeignKey(p => p.WorldCharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Status).HasDefaultValue(SessionParticipantStatus.Invited);
        });

        // Seed global roles: Player, GameMaster, Admin
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Player", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "GameMaster", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Admin", CreatedAt = new DateTime(2025, 11, 4, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
