// -----------------------------------------------------------------------
// <copyright file="CreateAchievementDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for creating a new Achievement.
/// </summary>
public class CreateAchievementDto
{
    /// <summary>
    /// Gets or sets the achievement name (3-200 characters).
    /// </summary>
    [Required(ErrorMessage = "Achievement name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement description (3-1000 characters).
    /// </summary>
    [Required(ErrorMessage = "Achievement description is required")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 1000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement level (World, Campaign, or Chapter).
    /// </summary>
    [Required(ErrorMessage = "Achievement level is required")]
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
    public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;

    /// <summary>
    /// Gets or sets the icon URL for this achievement.
    /// </summary>
    [MaxLength(500, ErrorMessage = "Icon URL cannot exceed 500 characters")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the reward description.
    /// </summary>
    [MaxLength(500, ErrorMessage = "Reward description cannot exceed 500 characters")]
    public string? RewardDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is awarded automatically.
    /// </summary>
    public bool IsAutomatic { get; set; } = false;

    /// <summary>
    /// Gets or sets the automatic unlock condition (JSON format).
    /// </summary>
    public string? AutomaticCondition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement is hidden until unlocked.
    /// </summary>
    public bool IsSecret { get; set; } = false;
}
