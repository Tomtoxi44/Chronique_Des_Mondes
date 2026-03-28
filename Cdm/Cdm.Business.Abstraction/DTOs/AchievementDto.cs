// -----------------------------------------------------------------------
// <copyright file="AchievementDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for an Achievement.
/// </summary>
public class AchievementDto
{
    /// <summary>
    /// Gets or sets the achievement identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the achievement name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement level (World, Campaign, or Chapter).
    /// </summary>
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
    /// Gets or sets the achievement rarity.
    /// </summary>
    public AchievementRarity Rarity { get; set; }

    /// <summary>
    /// Gets or sets the icon URL for this achievement.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the reward description.
    /// </summary>
    public string? RewardDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is awarded automatically.
    /// </summary>
    public bool IsAutomatic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is hidden until unlocked.
    /// </summary>
    public bool IsSecret { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the achievement creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this achievement.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the number of users who have unlocked this achievement.
    /// </summary>
    public int UnlockCount { get; set; }
}
