// -----------------------------------------------------------------------
// <copyright file="EventDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for an Event.
/// </summary>
public class EventDto
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the event name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description/narrative.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the event level (World, Campaign, or Chapter).
    /// </summary>
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
    public EventEffectType EffectType { get; set; }

    /// <summary>
    /// Gets or sets the target stat name.
    /// </summary>
    public string? TargetStat { get; set; }

    /// <summary>
    /// Gets or sets the modifier value.
    /// </summary>
    public int? ModifierValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is permanent.
    /// </summary>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// Gets or sets the expiration date for temporary events.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the event creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this event.
    /// </summary>
    public int CreatedBy { get; set; }
}
