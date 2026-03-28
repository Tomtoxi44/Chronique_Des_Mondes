// -----------------------------------------------------------------------
// <copyright file="Event.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cdm.Common.Enums;

/// <summary>
/// Represents an event that affects characters in a world, campaign, or chapter.
/// Events are modifiers created by the Game Master that automatically impact character stats.
/// </summary>
[Table("Events")]
public class Event
{
    /// <summary>
    /// Gets or sets the event unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the event name (max 200 characters).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description/narrative (max 2000 characters).
    /// </summary>
    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the event level (World = 0, Campaign = 1, Chapter = 2).
    /// </summary>
    [Required]
    public EventLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the world ID if this is a world-level event.
    /// </summary>
    public int? WorldId { get; set; }

    /// <summary>
    /// Gets or sets the campaign ID if this is a campaign-level event.
    /// </summary>
    public int? CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the chapter ID if this is a chapter-level event.
    /// </summary>
    public int? ChapterId { get; set; }

    /// <summary>
    /// Gets or sets the effect type (StatModifier, HealthModifier, DiceModifier, Narrative).
    /// </summary>
    [Required]
    public EventEffectType EffectType { get; set; }

    /// <summary>
    /// Gets or sets the target stat name (e.g., "Strength", "Dexterity", "MaxHealth").
    /// Only used for StatModifier and HealthModifier effect types.
    /// </summary>
    [MaxLength(100)]
    public string? TargetStat { get; set; }

    /// <summary>
    /// Gets or sets the modifier value (positive or negative).
    /// Example: -2 for "-2 in Dexterity", +5 for "+5 bonus to spell attacks".
    /// </summary>
    public int? ModifierValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this event is permanent or temporary.
    /// </summary>
    [Required]
    public bool IsPermanent { get; set; } = true;

    /// <summary>
    /// Gets or sets the expiration date for temporary events.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the event creation timestamp (UTC).
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this event (the Game Master).
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
}
