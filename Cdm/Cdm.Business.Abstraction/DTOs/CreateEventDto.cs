// -----------------------------------------------------------------------
// <copyright file="CreateEventDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for creating a new Event.
/// </summary>
public class CreateEventDto
{
    /// <summary>
    /// Gets or sets the event name (3-200 characters).
    /// </summary>
    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description/narrative (max 2000 characters).
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the event level (World, Campaign, or Chapter).
    /// </summary>
    [Required(ErrorMessage = "Event level is required")]
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
    /// Gets or sets the effect type.
    /// </summary>
    [Required(ErrorMessage = "Effect type is required")]
    public EventEffectType EffectType { get; set; }

    /// <summary>
    /// Gets or sets the target stat name (required for stat/health modifiers).
    /// </summary>
    [MaxLength(100, ErrorMessage = "Target stat name cannot exceed 100 characters")]
    public string? TargetStat { get; set; }

    /// <summary>
    /// Gets or sets the modifier value (positive or negative).
    /// </summary>
    public int? ModifierValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is permanent or temporary.
    /// </summary>
    public bool IsPermanent { get; set; } = true;

    /// <summary>
    /// Gets or sets the expiration date for temporary events.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
