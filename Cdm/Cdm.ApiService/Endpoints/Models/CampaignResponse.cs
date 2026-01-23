// -----------------------------------------------------------------------
// <copyright file="CampaignResponse.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints.Models;

using Cdm.Common.Enums;

/// <summary>
/// Response model for campaign data.
/// </summary>
public class CampaignResponse
{
    /// <summary>
    /// Gets or sets the campaign identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the campaign name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the game type for the campaign.
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility level.
    /// </summary>
    public Visibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players allowed in the campaign.
    /// </summary>
    public int MaxPlayers { get; set; }

    /// <summary>
    /// Gets or sets the URL of the campaign cover image.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who created the campaign.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the campaign creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the campaign last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the campaign is active.
    /// </summary>
    public bool IsActive { get; set; }
}
