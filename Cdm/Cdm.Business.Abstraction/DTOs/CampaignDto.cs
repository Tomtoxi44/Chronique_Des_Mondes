namespace Cdm.Business.Abstraction.DTOs;

using System;
using Cdm.Common.Enums;

/// <summary>
/// Data Transfer Object for a Campaign
/// </summary>
public class CampaignDto
{
    /// <summary>
    /// Gets or sets the campaign identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the campaign name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the world ID this campaign belongs to
    /// </summary>
    public int WorldId { get; set; }

    /// <summary>
    /// Gets or sets the game system type (inherited from World)
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility (Private/Public)
    /// </summary>
    public Visibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players
    /// </summary>
    public int MaxPlayers { get; set; }

    /// <summary>
    /// Gets or sets the cover image URL
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the campaign creator
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the campaign creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the campaign last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the campaign status (Planning, Active, OnHold, Completed, Cancelled)
    /// </summary>
    public CampaignStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the invite token (null if not generated yet)
    /// </summary>
    public string? InviteToken { get; set; }

    /// <summary>
    /// Gets or sets whether the campaign is active (deprecated - use Status)
    /// </summary>
    public bool IsActive { get; set; }
}
