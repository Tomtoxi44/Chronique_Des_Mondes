// -----------------------------------------------------------------------
// <copyright file="Achievement.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cdm.Common.Enums;

/// <summary>
/// Represents an achievement that players can unlock in a world, campaign, or chapter.
/// Achievements are created by the Game Master and can be awarded automatically or manually.
/// </summary>
[Table("Achievements")]
public class Achievement
{
    /// <summary>
    /// Gets or sets the achievement unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the achievement name (max 200 characters).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement description (how to obtain it) (max 1000 characters).
    /// </summary>
    [Required]
    [MaxLength(1000)]
    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement level (World = 0, Campaign = 1, Chapter = 2).
    /// </summary>
    [Required]
    public AchievementLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the world ID if this is a world-level achievement.
    /// </summary>
    public int? WorldId { get; set; }

    /// <summary>
    /// Gets or sets the campaign ID if this is a campaign-level achievement.
    /// </summary>
    public int? CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the chapter ID if this is a chapter-level achievement.
    /// </summary>
    public int? ChapterId { get; set; }

    /// <summary>
    /// Gets or sets the achievement rarity (Common, Rare, Epic, Legendary).
    /// </summary>
    [Required]
    public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;

    /// <summary>
    /// Gets or sets the icon URL for this achievement.
    /// </summary>
    [MaxLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the reward description (experience points, title, item, etc.).
    /// </summary>
    [MaxLength(500)]
    public string? RewardDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is awarded automatically.
    /// If false, it must be awarded manually by the Game Master.
    /// </summary>
    [Required]
    public bool IsAutomatic { get; set; } = false;

    /// <summary>
    /// Gets or sets the automatic unlock condition (JSON format for complex conditions).
    /// Example: {"Type": "ReachLevel", "Value": 10} or {"Type": "InflictDamage", "Value": 100}.
    /// Only used if IsAutomatic is true.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? AutomaticCondition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is hidden from players until unlocked.
    /// </summary>
    [Required]
    public bool IsSecret { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the achievement creation timestamp (UTC).
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this achievement (the Game Master).
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the world (if world-level).
    /// </summary>
    [ForeignKey(nameof(WorldId))]
    public virtual World? World { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the campaign (if campaign-level).
    /// </summary>
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign? Campaign { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the chapter (if chapter-level).
    /// </summary>
    [ForeignKey(nameof(ChapterId))]
    public virtual Chapter? Chapter { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the creator.
    /// </summary>
    [ForeignKey(nameof(CreatedBy))]
    public virtual User Creator { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user achievements (players who unlocked this achievement).
    /// </summary>
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
