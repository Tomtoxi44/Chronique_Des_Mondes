// -----------------------------------------------------------------------
// <copyright file="UserAchievement.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents the junction table between users and achievements.
/// Tracks which achievements have been unlocked by which users.
/// </summary>
[Table("UserAchievements")]
public class UserAchievement
{
    /// <summary>
    /// Gets or sets the user achievement unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who unlocked the achievement.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the achievement ID that was unlocked.
    /// </summary>
    [Required]
    public int AchievementId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the achievement was unlocked (UTC).
    /// </summary>
    [Required]
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether this achievement was awarded automatically or manually by the GM.
    /// </summary>
    [Required]
    public bool IsManuallyAwarded { get; set; } = false;

    /// <summary>
    /// Gets or sets the user ID of the Game Master who manually awarded this achievement (if applicable).
    /// </summary>
    public int? AwardedBy { get; set; }

    /// <summary>
    /// Gets or sets an optional message from the GM when manually awarding.
    /// </summary>
    [MaxLength(500)]
    public string? AwardMessage { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property to the achievement.
    /// </summary>
    [ForeignKey(nameof(AchievementId))]
    public virtual Achievement Achievement { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property to the Game Master who awarded this (if manual).
    /// </summary>
    [ForeignKey(nameof(AwardedBy))]
    public virtual User? AwardedByUser { get; set; }
}
