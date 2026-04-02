// -----------------------------------------------------------------------
// <copyright file="UserAchievementDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for a UserAchievement (an achievement unlocked by a user).
/// </summary>
public class UserAchievementDto
{
    /// <summary>
    /// Gets or sets the user achievement identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who unlocked the achievement.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the username of the user who unlocked the achievement.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement ID that was unlocked.
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Gets or sets the achievement name.
    /// </summary>
    public string AchievementName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement description.
    /// </summary>
    public string AchievementDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement rarity.
    /// </summary>
    public AchievementRarity Rarity { get; set; }

    /// <summary>
    /// Gets or sets the achievement icon URL.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the achievement was unlocked (UTC).
    /// </summary>
    public DateTime UnlockedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this achievement was awarded manually by the GM.
    /// </summary>
    public bool IsManuallyAwarded { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the Game Master who manually awarded this achievement (if applicable).
    /// </summary>
    public int? AwardedBy { get; set; }

    /// <summary>
    /// Gets or sets the name of the Game Master who manually awarded this achievement (if applicable).
    /// </summary>
    public string? AwardedByName { get; set; }

    /// <summary>
    /// Gets or sets an optional message from the GM when manually awarding.
    /// </summary>
    public string? AwardMessage { get; set; }
}
